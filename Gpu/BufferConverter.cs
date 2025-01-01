using Engine.Cameras;
using Engine.Geometry;
using Engine.Renderers;
using Engine.Scenes;
using System.Numerics;

namespace Gpu;

public class BufferConverter
{
    public static ClBuffers ConvertToBuffers(OpenCLManager manager, Scene scene, SampledCamera camera)
    {
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
            Rays = new InputBuffer<ClRay>(manager, rays),
            Triangles = new InputBuffer<ClTriangle>(manager, triangles),
            Spheres = new InputBuffer<ClSphere>(manager, spheres)
        };
    }

    /// <summary>
    /// Returns a buffer full of starting positions for a ray
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    private static ClRay[] GenerateRayBuffers(OpenCLManager manager, SampledCamera camera)
    {
        var numOfSamples = camera.ImageSize.Width * camera.ImageSize.Height * camera.NumberOfSamples;
        // Create vector3 buffer that can contain all neccessary random pixel offsets for all samples
        ClRay[] rays = new ClRay[numOfSamples];
        
        for (int i = 0; i < camera.ImageSize.Width; i++)
        {
            for (int j = 0; j < camera.ImageSize.Height; j++)
            {
                for (int s = 0; s < camera.NumberOfSamples; s++)
                {
                    // Calculating a ray
                    var ray = camera.GetRayTowardsPixel(i, j);
                    
                    // find the index of the sample in the arrays
                    var sampleIndex = (i * camera.ImageSize.Height + j) * camera.NumberOfSamples + s;
                    
                    // Setting the values
                    rays[sampleIndex] = new ClRay { Origin = ray.Origin, Direction = ray.Direction};
                }
            }
        }

        return rays;
    }
}

// This is a structure of arrays that would represent the structure to complete a trace of one pixel
// pixel offset of the ray
public struct ClBuffers
{
    public Buffer Rays { get; set; }
    public Buffer Triangles { get; set; }
    public Buffer Spheres { get; set; }
}
