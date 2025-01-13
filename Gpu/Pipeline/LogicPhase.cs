namespace Gpu.Pipeline;

public class LogicPhase : Phase
{
    /// <summary>
    /// Accumulates radiance contributions and queues new ray generations.
    /// </summary>
    /// <param name="manager">OpenCLManager used for buffer creation</param>
    /// <param name="path">Path to OpenCL program</param>
    /// <param name="kernel">Kernel to execute</param>
    /// <param name="shadowRaysBuffer">Buffer of shadow rays to trace</param>
    /// <param name="sceneInfoBuffer">Buffer that contains scene info</param>
    /// <param name="spheresBuffer">Buffer that contains spheres used for intersection calculations</param>
    /// <param name="trianglesBuffer">Buffer that contains triangles used for intersection calculations</param>
    public LogicPhase(
        OpenCLManager manager, 
        string path,
        string kernel,
        Buffer shadowRaysBuffer,
        Buffer sceneInfoBuffer,
        Buffer spheresBuffer,
        Buffer trianglesBuffer)
    {
        manager.AddProgram(path, "logic.cl")
            .AddKernel("logic.cl",
                kernel, 
                shadowRaysBuffer,
                sceneInfoBuffer, 
                spheresBuffer,
                trianglesBuffer/*,
                intersectionsBuffer*/);

        KernelId = manager.GetKernelId(kernel);
    }
}
