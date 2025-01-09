using Engine.Cameras;
using Engine.Geometry;
using Engine.Scenes;
using Gpu.OpenCL;

namespace Gpu;

public class BufferConverter
{
    /// <summary>
    /// This method converts the scene originally designed for the CPU path tracer so that it can be used in the GPU path tracer
    /// </summary>
    public static ClSceneBuffers ConvertSceneToBuffers(OpenCLManager manager, Scene scene)
    {
        var sceneInfo = new ClSceneInfo
        {
            NumSpheres = scene.Objects.OfType<Sphere>().Count(),
            NumTriangles = scene.Objects.OfType<Triangle>().Count(),
        };
        
        // Gather all used materials
        List<ClMaterial> materials = new();
        Dictionary<Engine.Materials.Material, uint> materialIndex = new();
        foreach (var obj in scene.Objects)
        {
            if (materialIndex.ContainsKey(obj.Material)) continue;

            ClMaterial material;

            switch (obj.Material)
            {
                case Engine.Materials.Diffuse diffuseMat:
                    material.Type = MaterialType.Diffuse;
                    material.Color = ClFloat3.FromVector3(diffuseMat.Color);
                    material.Albedo = diffuseMat.Albedo;
                    break;
                case Engine.Materials.Reflective reflectiveMat:
                    material.Type = MaterialType.Reflective;
                    material.Color = ClFloat3.FromVector3(reflectiveMat.Color);
                    material.Albedo = reflectiveMat.Albedo;
                    break;
                default:
                    throw new Exception("Material type not yet supported: " + obj.Material.GetType().Name);
            }

            materialIndex.Add(obj.Material, (uint)materials.Count);
            materials.Add(material);
        }

        // Process all triangles
        var triangles = new List<ClTriangle>();
        foreach (var t in scene.Objects.OfType<Triangle>())
        {
            triangles.Add(new ClTriangle
            {
                V1 = ClFloat3.FromVector3(t.Vertex1),
                V2 = ClFloat3.FromVector3(t.Vertex2),
                V3 = ClFloat3.FromVector3(t.Vertex3),
                // Normal = ClFloat3.FromVector3(t._normal),
                Material = materialIndex[t.Material],
            });
        }

        var spheres = new List<ClSphere>();

        foreach (var s in scene.Objects.OfType<Sphere>())
        {
            spheres.Add(new ClSphere
            {
                Position = ClFloat3.FromVector3(s.Position),
                Radius = s.Radius,
                Material = materialIndex[s.Material],
            });
        }


        return new ClSceneBuffers
        {
            SceneInfo = new ReadWriteBuffer<ClSceneInfo>(manager, new [] {sceneInfo}),
            Spheres = new ReadWriteBuffer<ClSphere>(manager, spheres.ToArray()),
            Triangles = new ReadWriteBuffer<ClTriangle>(manager, triangles.ToArray()),
            Materials = new ReadOnlyBuffer<ClMaterial>(manager, materials.ToArray()),
        };
    }

    /// <summary>
    /// Returns a buffer full of starting positions for a ray
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    private static ClRay[] GenerateRayBuffers(OpenCLManager manager, OpenCLCamera camera)
    {
        // var numOfSamples = camera.ImageSize.Width * camera.ImageSize.Height * camera.NumberOfSamples;
        // Create vector3 buffer that can contain all neccessary random pixel offsets for all samples
        var rays = new List<ClRay>();
        
        for (int i = 0; i < camera.ImageSize.Width; i++)
        {
            for (int j = 0; j < camera.ImageSize.Height; j++)
            {
                var ray = camera.GetRayTowardsPixel(i, j);
                var clRay = new ClRay
                {
                    Direction = ClFloat3.FromVector3(ray.Direction), 
                    Origin = ClFloat3.FromVector3(ray.Origin)
                };
                rays.Add(clRay);
            }
        }

        return rays.ToArray();
    }
}

public struct ClSceneBuffers
{
    public Buffer SceneInfo { get; set; }
    public Buffer Triangles { get; set; }
    public Buffer Spheres { get; set; }
    public Buffer Materials { get; set; }
}
