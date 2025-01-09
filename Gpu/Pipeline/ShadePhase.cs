namespace Gpu.Pipeline;

public class ShadePhase : Phase
{
    public Buffer ShadowRaysBuffer { get; private set; }
    public Buffer DiffuseAlbedosBuffer { get; private set; }
    public Buffer DiffuseColorBuffer { get; private set; }

    public Buffer DebugBuffer { get; private set; }

    /// <summary>
    /// Calculates extension rays and shadow rays to trace in phases 2 and 4.
    /// Writes to three buffers, extensionRays (phase 2), shadowRays (phase 4) and colors.
    /// </summary>
    /// <param name="manager">OpenCLManager used for buffer creation</param>
    /// <param name="path">Path to OpenCL program</param>
    /// <param name="kernel">Kernel to execute</param>
    /// <param name="randomStates">The current seeds or states for the random number generator</param>
    /// <param name="intersections">Takes buffer of intersection results</param>
    /// <param name="extensionRays">Takes ray buffer that is used in phase 2 for intersection calculation</param>
    /// <param name="pixelColors">Buffer that contains the color values for each pixel</param>
    public ShadePhase(
        OpenCLManager manager,
        string path, string kernel,
        Buffer randomStates,
        Buffer intersections,
        Buffer extensionRays,
        Buffer pixelColors)
    {
        float[] mat_albedos = new float[]
        {
            .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f, .69f
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

        var shadowRays = new ClShadowRay[intersections.GetLength()];
        ShadowRaysBuffer  = new ReadWriteBuffer<ClShadowRay>(manager, shadowRays);

        DiffuseAlbedosBuffer = new ReadWriteBuffer<float>(manager, mat_albedos);

        DiffuseColorBuffer = new ReadWriteBuffer<ClFloat3>(manager, mat_colors);

        DebugBuffer = new ReadWriteBuffer<ClFloat3>(manager, new ClFloat3[16]);

        manager.AddProgram(path, "shade.cl")
            .AddBuffers(ShadowRaysBuffer, DiffuseAlbedosBuffer, DiffuseColorBuffer, DebugBuffer)
            .AddKernel(
                "shade.cl", 
                kernel,
                randomStates,
                intersections,
                DiffuseAlbedosBuffer,
                DiffuseColorBuffer,
                extensionRays, 
                ShadowRaysBuffer,
                pixelColors,
                DebugBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
