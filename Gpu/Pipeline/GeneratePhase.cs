using Gpu.OpenCL;

namespace Gpu.Pipeline;

public class GeneratePhase : Phase
{ 
    public Buffer RayBuffer { get; private set; }
    public Buffer DebugBuffer { get; private set; }

    /// <summary>
    /// Generates primary rays on the GPU.
    /// Writes to a ray buffer that is filled on the GPU.
    /// </summary>
    /// <param name="manager">OpenCL manager for buffer creation</param>
    /// <param name="path">Path to program file</param>
    /// <param name="kernel">Name of kernel to execute</param>
    /// <param name="sceneInfoBuffer">Contains info about camera and such</param>
    /// <param name="numOfRays">Used to find the length of the ray buffer</param>
    public GeneratePhase(
        OpenCLManager manager,
        string path,
        string kernel,
        Buffer sceneInfoBuffer,
        Buffer queueStates,
        Buffer newRayQueue,
        Buffer extendRayQueue,
        int numOfRays)
    {
        RayBuffer = new ReadWriteBuffer<ClRay>(manager, new ClRay[numOfRays]);

        DebugBuffer = new ReadWriteBuffer<ClFloat3>(manager, new ClFloat3[numOfRays]);

        manager.AddProgram(path, "generate.cl")
            .AddBuffers(RayBuffer, DebugBuffer)
            .AddKernel("generate.cl", kernel, sceneInfoBuffer, queueStates, newRayQueue, extendRayQueue, RayBuffer, DebugBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
