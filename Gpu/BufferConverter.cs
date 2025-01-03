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
            ImageHeight = camera.ImageSize.Height,
            ImageWidth = camera.ImageSize.Width,
            NumSpheres = scene.Objects.OfType<Sphere>().Count(),
            NumTriangles = scene.Objects.OfType<Triangle>().Count(),
        };
        
        var rays = GenerateRayBuffers(manager, camera);

        var triangles = scene.Objects
            .OfType<Triangle>()
            .Select(t => new ClTriangle { V1 = t.Vertex1, V2 = t.Vertex2, V3 = t.Vertex3, Normal = t._normal })
            .ToArray();
        
        var spheres = scene.Objects
            .OfType<Sphere>()
            .Select(s => new ClSphere { Origin = s.Position, Radius = s.Radius})
            .ToArray();

        return new ClBuffers
        {
            SceneInfo = new InputBuffer<ClSceneInfo>(manager, new ClSceneInfo[1] { sceneInfo }),
            Rays = new InputBuffer<ClRay>(manager, rays),
            Spheres = new InputBuffer<ClSphere>(manager, spheres),
            Triangles = new InputBuffer<ClTriangle>(manager, triangles),
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
        ClRay[] rays = new ClRay[camera.ImageSize.Width * camera.ImageSize.Height];
        
        for (int i = 0; i < camera.ImageSize.Width; i++)
        {
            for (int j = 0; j < camera.ImageSize.Height; j++)
            {
                var ray = camera.GetRayTowardsPixel(i, j);

                var index = (i * camera.ImageSize.Height) + j;
                rays[index] = new ClRay { Origin = ray.Origin, Direction = ray.Direction};

                /*for (int s = 0; s < camera.NumberOfSamples; s++)
                {
                    // Calculating a ray
                    var ray = camera.GetRayTowardsPixel(i, j);

                    // find the index of the sample in the arrays
                    var sampleIndex = (i * camera.ImageSize.Height + j) * camera.NumberOfSamples + s;

                    // Setting the values
                    rays[sampleIndex] = new ClRay { Origin = ray.Origin, Direction = ray.Direction};
                }*/
            }
        }

        return rays;
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
