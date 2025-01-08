namespace Gpu.Pipeline;

public class ShadePhase : Phase
{
    public Buffer ShadowRaysBuffer { get; private set; }
    public Buffer DiffuseAlbedosBuffer { get; private set; }
    public Buffer DiffuseColorBuffer { get; private set; }

    /// <summary>
    /// Calculates extension rays and shadow rays to trace in phases 2 and 4.
    /// Writes to three buffers, extensionRays (phase 2), shadowRays (phase 4) and colors.
    /// </summary>
    /// <param name="manager">OpenCLManager used for buffer creation</param>
    /// <param name="path">Path to OpenCL program</param>
    /// <param name="kernel">Kernel to execute</param>
    /// <param name="randomStates">The current seeds or states for the random number generator</param>
    /// <param name="intersectionResults">Takes buffer of intersection results</param>
    /// <param name="extensionRays">Takes ray buffer that is used in phase 2 for intersection calculation</param>
    /// <param name="pixelColors">Buffer that contains the color values for each pixel</param>
    public ShadePhase(
        OpenCLManager manager,
        string path, string kernel,
        Buffer randomStates,
        Buffer intersectionResults,
        Buffer extensionRays,
        Buffer pixelColors)
    {
        float[] mat_albedos = new float[]
        {
            6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f, 6.9f
        };

        ClFloat3[] mat_colors = new ClFloat3[]
        {
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
            new ClFloat3 { X = 80, Y = 69, Z = 111 },
        };

        var shadowRays = new ClShadowRay[intersectionResults.GetLength()];
        ShadowRaysBuffer  = new ReadWriteBuffer<ClShadowRay>(manager, shadowRays);

        var diffuseAlbedos = mat_albedos;
        DiffuseAlbedosBuffer = new ReadWriteBuffer<float>(manager, diffuseAlbedos);

        var diffuseColor = mat_colors;
        DiffuseColorBuffer = new ReadWriteBuffer<ClFloat3>(manager, diffuseColor);

        manager.AddProgram(path, "shade.cl")
            .AddBuffers(ShadowRaysBuffer, DiffuseAlbedosBuffer, DiffuseColorBuffer)
            .AddKernel(
                "shade.cl", 
                kernel,
                randomStates,
                intersectionResults, // TODO: Intersection zelf is al genoeg info
                DiffuseAlbedosBuffer,
                DiffuseColorBuffer,
                extensionRays, 
                ShadowRaysBuffer,
                pixelColors);

        KernelId = manager.GetKernelId(kernel);
    }
}
