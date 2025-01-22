using Gpu.OpenCL;

namespace Gpu.Pipeline;

public class ShadowPhase : Phase
{ 
    public Buffer DebugBuffer { get; private set; }

    /// <summary>
    /// Generates primary rays on the GPU.
    /// Writes to a ray buffer that is filled on the GPU.
    /// </summary>
    /// <param name="manager">OpenCL manager for buffer creation</param>
    /// <param name="path">Path to program file</param>
    /// <param name="kernel">Name of kernel to execute</param>
    public ShadowPhase(
        OpenCLManager manager,
        string path,
        string kernel,
        Buffer sceneInfo,
        Buffer queueStates,
        Buffer shadowRayQueue,
        Buffer pathStates,
        Buffer spheres,
        Buffer triangles)
    {
        DebugBuffer = new ReadWriteBuffer<ClFloat3>(manager, new ClFloat3[pathStates.GetLength()]);

        manager.AddProgram(path, "shadow.cl")
            .AddBuffers(DebugBuffer)
            .AddKernel("shadow.cl", kernel,
                sceneInfo, queueStates, shadowRayQueue, pathStates, spheres, triangles);

        KernelId = manager.GetKernelId(kernel);
    }
}
