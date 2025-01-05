namespace Gpu.Pipeline;

public class ShadePhase : Phase
{
    public Buffer ExtensionRaysBuffer { get; private set; }
    public Buffer ShadowRaysBuffer { get; private set; }
    
    /// <summary>
    /// Calculates extension rays and shadow rays to trace in phases 2 and 4.
    /// Writes to three buffers, extensionRays (phase 2), shadowRays (phase 4) and colors.
    /// </summary>
    /// <param name="manager">OpenCLManager used for buffer creation</param>
    /// <param name="path">Path to OpenCL program</param>
    /// <param name="kernel">Kernel to execute</param>
    /// <param name="intersectionResults">Takes buffer of intersection results</param>
    /// <param name="rayBuffer">Takes ray buffer that is used in phase 2 for intersection calculation</param>
    /// <param name="colorsBuffer">Buffer that contains the color values for each pixel</param>

    public ShadePhase(
        OpenCLManager manager, 
        String path, String kernel,
        Buffer intersectionResults,
        Buffer rayBuffer,
        Buffer colorsBuffer)
    {
        var shadowRays = new ClShadowRay[intersectionResults.GetLength()];

        var extensionRaysBuffer = rayBuffer;
        var shadowRaysBuffer = new ReadWriteBuffer<ClShadowRay>(manager, shadowRays);

        
        manager.AddProgram(path)
            .AddBuffers(shadowRaysBuffer)
            .AddKernel(kernel, intersectionResults, extensionRaysBuffer, shadowRaysBuffer, colorsBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
