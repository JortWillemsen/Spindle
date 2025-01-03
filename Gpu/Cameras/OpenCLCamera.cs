using Engine.Renderers;
using Gpu;
using System.Drawing;
using System.Numerics;

namespace Engine.Cameras;

public class OpenCLCamera : Camera
{
    public OpenCLManager Manager { get; private set; }
    public OpenCLCamera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth)
        : base(position, up, front, imageSize, FOV, maxDepth)
    {
    }

    public override void RenderShot(IRenderer renderer, in Span<int> pixels)
    {
        
        var globalWorkSize = new nuint[2] { (nuint) ImageSize.Width, (nuint) ImageSize.Height };
        var localWorkSize = new nuint[2] { 1, 1};
        
        Manager = new OpenCLManager()
            .SetProgram("/../../../../Gpu/Programs/WavefrontPathTracer.cl")
            .SetBuffers(renderer.Scene, this)
            .SetKernel("trace")
            .SetWorkSize(globalWorkSize, localWorkSize);

        var output = Manager.Execute();

        output.CopyTo(pixels); // Assumes equal length
    }
}
