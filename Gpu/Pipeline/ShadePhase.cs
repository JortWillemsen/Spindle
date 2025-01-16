using Gpu.OpenCL;

namespace Gpu.Pipeline;

public class ShadePhase : Phase
{
    public Buffer ShadowRaysBuffer { get; private set; }

    public Buffer DebugBuffer { get; private set; }

    /// <summary>
    /// Calculates extension rays and shadow rays to trace in phases 2 and 4.
    /// Writes to three buffers, extensionRays (phase 2), shadowRays (phase 4) and colors.
    /// </summary>
    /// <param name="manager">OpenCLManager used for buffer creation</param>
    /// <param name="path">Path to OpenCL program</param>
    /// <param name="kernel">Kernel to execute</param>
    /// <param name="materials">A constant buffer containing the materials in the scene</param>
    /// <param name="randomStates">The current seeds or states for the random number generator</param>
    /// <param name="intersections">Takes buffer of intersection results</param>
    /// <param name="extensionRays">Takes ray buffer that is used in phase 2 for intersection calculation</param>
    /// <param name="pixelColors">Buffer that contains the color values for each pixel</param>
    public ShadePhase(
        OpenCLManager manager,
        string path, string kernel,
        Buffer materials,
        Buffer randomStates,
        Buffer extensionRays,
        Buffer pixelColors)
    {
        var shadowRays = new ClRay[extensionRays.GetLength()];
        ShadowRaysBuffer  = new ReadWriteBuffer<ClRay>(manager, shadowRays);
        DebugBuffer = new ReadWriteBuffer<ClFloat3>(manager, new ClFloat3[extensionRays.GetLength()]);

        manager.AddProgram(path, "shade.cl")
            .AddBuffers(ShadowRaysBuffer, DebugBuffer)
            .AddKernel(
                "shade.cl", 
                kernel,
                materials,
                randomStates,
                extensionRays,
                ShadowRaysBuffer,
                pixelColors,
                DebugBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
