using Engine.Scenes;
using Gpu.OpenCL;

namespace Gpu.Pipeline;

public class ExtendPhase : Phase
{
    // public Buffer IntersectionsBuffer { get; private set; }
    public Buffer DebugBuffer { get; private set; }

    /// <summary>
    /// Calculates intersection of primary / extension rays.
    /// Writes to an intersection buffer that contains all intersection information for the shade phase
    /// </summary>
    /// <param name="manager">OpenCLManager used for buffer creation</param>
    /// <param name="path">Path to OpenCL program</param>
    /// <param name="kernel">Name of kernel to execute</param>
    /// <param name="rayBuffer">Takes a ray buffer that contains the rays to trace</param>
    /// <param name="sceneInfoBuffer">Buffer that contains scene info</param>
    /// <param name="spheresBuffer">Buffer that contains spheres used for intersection calculations</param>
    /// <param name="trianglesBuffer">Buffer that contains triangles used for intersection calculations</param>
    public ExtendPhase(
        OpenCLManager manager, 
        string path,
        string kernel,
        Buffer sceneInfoBuffer,
        Buffer spheresBuffer,
        Buffer trianglesBuffer,
        Buffer rayBuffer)
    {
        // var intersections = new ClIntersection[rayBuffer.GetLength()];
        // IntersectionsBuffer = new ReadWriteBuffer<ClIntersection>(manager, intersections);

        DebugBuffer = new ReadWriteBuffer<ClFloat3>(manager, new ClFloat3[16]);

        manager.AddProgram(path, "extend.cl")
            // .AddBuffers(IntersectionsBuffer)
            .AddBuffers(DebugBuffer)
            .AddKernel(
                "extend.cl",
                kernel, 
                sceneInfoBuffer,
                spheresBuffer,
                trianglesBuffer,
                rayBuffer,
                DebugBuffer/*,
                IntersectionsBuffer*/);

        KernelId = manager.GetKernelId(kernel);
    }
}
