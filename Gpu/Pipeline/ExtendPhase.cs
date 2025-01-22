using Gpu.OpenCL;

namespace Gpu.Pipeline;

public class ExtendPhase : Phase
{
    public Buffer DebugBuffer { get; private set; }

    /// <summary>
    /// Calculates intersection of primary / extension rays.
    /// Updates ray buffer with intersection info
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
        Buffer queueStates,
        Buffer extendRayQueue,
        Buffer rayBuffer)
    {
        DebugBuffer = new ReadWriteBuffer<ClFloat3>(manager, new ClFloat3[rayBuffer.GetLength()]);

        manager.AddProgram(path, "extend.cl")
            .AddBuffers(DebugBuffer)
            .AddKernel(
                "extend.cl",
                kernel, 
                sceneInfoBuffer,
                spheresBuffer,
                trianglesBuffer,
                queueStates,
                extendRayQueue,
                rayBuffer,
                DebugBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
