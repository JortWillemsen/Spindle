using Engine.Cameras;
using Engine.Renderers;
using System.Numerics;

namespace Gpu;

public class BufferConverter
{
    public static ClBuffers ConvertToBuffers(OpenCLManager manager, IRenderer renderer, SampledCamera camera)
    {
        var rays = GenerateRayBuffers(manager, camera);

        return new ClBuffers
        {
            Origins = new InputBuffer<Vector3>(manager, rays.Origins),
            Directions = new InputBuffer<Vector3>(manager, rays.Directions)
        };
    }

    /// <summary>
    /// Returns a buffer full of starting positions for a ray
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    private static RaySoA GenerateRayBuffers(OpenCLManager manager, SampledCamera camera)
    {
        var numOfSamples = camera.ImageSize.Width * camera.ImageSize.Height * camera.NumberOfSamples;
        // Create vector3 buffer that can contain all neccessary random pixel offsets for all samples
        Vector3[] origins = new Vector3[numOfSamples];
        Vector3[] directions = new Vector3[numOfSamples];
        
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
                    origins[sampleIndex] = ray.Origin;
                    directions[sampleIndex] = ray.Direction;
                }
            }
        }

        return new RaySoA { Directions = directions, Origins = origins };
    }
}

// This is a structure of arrays that would represent the structure to complete a trace of one pixel
// pixel offset of the ray
public struct ClBuffers
{
    public Buffer Origins { get; set; }
    public Buffer Directions { get; set; }
}

public struct RaySoA
{
    public Vector3[] Origins { get; set; }
    public Vector3[] Directions { get; set; }
}
