using Engine.Renderers;
using Gpu;
using Gpu.Pipeline;
using System.Drawing;
using System.Numerics;

namespace Engine.Cameras;

public class OpenCLCamera : Camera
{
    public WavefrontPipeline Pipeline { get; private set; }

    public int Samples { get; private set; }
    public OpenCLCamera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth, int samples)
        : base(position, up, front, imageSize, FOV, maxDepth)
    {
        Samples = samples;
    }

    public override void RenderShot(IRenderer renderer, in Span<int> pixels)
    {
        Pipeline = new WavefrontPipeline(renderer.Scene, this);

        var output = Pipeline.Execute();

        //output.CopyTo(pixels); // Assumes equal length
    }
}
