using Engine.Cameras;
using Engine.Renderers;
using Engine.Scenes;
using Gpu.Pipeline;
using System.Drawing;
using System.Numerics;

namespace Gpu.Cameras;

public class OpenCLCamera : Camera
{
    public WavefrontPipeline? Pipeline { get; private set; }
    public Scene? Scene { get; private set; } 

    public OpenCLCamera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth)
        : base(position, up, front, imageSize, FOV, maxDepth)
    {
        OnTransform += () => { Pipeline.OnMove(Scene, this);};
    }

    public override void RenderShot(IRenderer renderer, in Span<int> pixels)
    {
        Pipeline ??= new WavefrontPipeline(renderer.Scene, this);
        Scene ??= renderer.Scene;

        var output = Pipeline.Execute();

        output.CopyTo(pixels); // Assumes equal length
    }
}
