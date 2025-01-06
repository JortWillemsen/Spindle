using Engine.Renderers;
using Gpu;
using System.Drawing;
using System.Numerics;

namespace Engine.Cameras;

public class OpenCLCamera : Camera
{
    public OpenCLManager Manager { get; private set; }
    public int Samples { get; private set; }
    public OpenCLCamera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth, int samples)
        : base(position, up, front, imageSize, FOV, maxDepth)
    {
        Samples = samples;
    }

    public override void RenderShot(IRenderer renderer, in Span<int> pixels)
    {
        
        var globalWorkSize = new nuint[2] { (nuint) ImageSize.Width, (nuint) ImageSize.Height };
        var localWorkSize = new nuint[2] { 1, 1};
        
        Manager = new OpenCLManager()
            .AddProgram("/../../../../Gpu/Programs/WavefrontPathTracer.cl", "WavefrontPathTracer.cl")
            /*
            .SetBuffers(renderer.Scene, this)
            */
            .AddKernel("WavefrontPathTracer.cl", "trace")
            .SetWorkSize(globalWorkSize, localWorkSize);

        var output = new int[] { };

        output.CopyTo(pixels); // Assumes equal length
    }
}
