using Engine.Scenes;

namespace Gpu.Pipeline;

public class ExtendPhase : Phase
{
    public Buffer IntersectionsBuffer { get; private set; }

    /// <summary>
    /// Calculates intersection of primary / extension rays
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
        String path, 
        String kernel,
        Buffer rayBuffer,
        Buffer sceneInfoBuffer,
        Buffer spheresBuffer,
        Buffer trianglesBuffer)
    {
        var intersections = new ClIntersection[rayBuffer.GetLength()];
        var intersectionsBuffer = new ReadWriteBuffer<ClIntersection>(manager, intersections);
        IntersectionsBuffer = intersectionsBuffer;
        
        manager.AddProgram(path, "extend.cl")
            .AddBuffers(intersectionsBuffer)
            .AddKernel(
                "extend.cl",
                kernel, 
                rayBuffer, 
                sceneInfoBuffer,
                spheresBuffer,
                trianglesBuffer, 
                intersectionsBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
