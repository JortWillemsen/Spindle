using Engine.Cameras;
using Engine.Geometry;
using Engine.Renderers;
using Engine.Scenes;
using System.Numerics;

namespace Gpu;

public class BufferConverter
{
    public static ClBuffers ConvertToBuffers(OpenCLManager manager, Scene scene, OpenCLCamera camera)
    {
        var sceneInfo = new ClSceneInfo
        {
            ImageWidth = camera.ImageSize.Width,
            ImageHeight = camera.ImageSize.Height,
            NumSpheres = scene.Objects.OfType<Sphere>().Count(),
            NumTriangles = scene.Objects.OfType<Triangle>().Count(),
        };
        
        var rays = GenerateRayBuffers(manager, camera);

        var triangles = new List<ClTriangle>();

        foreach (var t in scene.Objects.OfType<Triangle>())
        {
            triangles.Add(new ClTriangle
            {
                V1 = ClFloat3.FromVector3(t.Vertex1),
                V2 = ClFloat3.FromVector3(t.Vertex2),
                V3 = ClFloat3.FromVector3(t.Vertex3),
                Normal = ClFloat3.FromVector3(t._normal)
            });
        }
        
        var spheres = new List<ClSphere>();
        
        foreach (var s in scene.Objects.OfType<Sphere>())
        {
            spheres.Add(new ClSphere
            {
                Position = ClFloat3.FromVector3(s.Position),
                Radius = s.Radius
            });
        }

        return new ClBuffers
        {
            SceneInfo = new InputBuffer<ClSceneInfo>(manager, new ClSceneInfo[1] { sceneInfo }),
            Rays = new InputBuffer<ClRay>(manager, rays),
            Spheres = new InputBuffer<ClSphere>(manager, spheres.ToArray()),
            Triangles = new InputBuffer<ClTriangle>(manager, triangles.ToArray()),
            Output = new OutputBuffer<int>(manager, new int[camera.ImageSize.Width * camera.ImageSize.Height])
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

// This is a structure of arrays that would represent the structure to complete a trace of one pixel
// pixel offset of the ray
public struct ClBuffers
{
    public Buffer SceneInfo { get; set; }
    public Buffer Rays { get; set; }
    public Buffer Triangles { get; set; }
    public Buffer Spheres { get; set; }
    public Buffer Output { get; set; }
}
