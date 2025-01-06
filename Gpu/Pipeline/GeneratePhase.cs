namespace Gpu.Pipeline;

public class GeneratePhase : Phase
{ 
    public Buffer RayBuffer { get; private set; }
    
    /// <summary>
    /// Generates primary rays on the GPU.
    /// Writes to a ray buffer that is filled on the GPU.
    /// </summary>
    /// <param name="manager">OpenCL manager for buffer creation</param>
    /// <param name="path">Path to program file</param>
    /// <param name="kernel">Name of kernel to execute</param>
    /// <param name="numOfRays">Used to find the length of the ray buffer</param>
    public GeneratePhase(
        OpenCLManager manager, 
        String path, 
        String kernel,
        int numOfRays)
    {
        var rays = new ClRay[numOfRays];
        var rayBuffer = new ReadWriteBuffer<ClRay>(manager, rays);
        RayBuffer = rayBuffer;
        
        manager.AddProgram(path, "generate.cl")
            .AddBuffers(rayBuffer)
            .AddKernel("generate.cl",kernel, rayBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
