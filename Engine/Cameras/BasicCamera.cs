using System.Drawing;
using System.Numerics;
using Engine.Renderers;

namespace Engine.Cameras;

public class BasicCamera : Camera
{
    // ReSharper disable once InconsistentNaming
    /// <inheritdoc />
    public BasicCamera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth, int samples)
        : base(position, up, front, imageSize, FOV, maxDepth, samples)
    {
    }

    public override void RenderShot(IRenderer renderer, in Span<int> pixels)
    {
        for (var j = 0; j < this.ImageSize.Height; j++)
        {
            for (var i = 0; i < this.ImageSize.Width; i++)
            {
                // int pixelColor = 0x0;
                Vector3 pixelColor = Vector3.Zero;
                int sample = 0;

                while (sample < Samples)
                {
                    var color = Vector3.One;
                    var ray = GetRayTowardsPixel(i, j);
                    renderer.TraceRay(ray, MaxDepth, ref color);

                    pixelColor += color;
                    // pixelColor = ColorInt.Make(ColorInt.GetVector(pixelColor) * ((float)sample / (sample + 1)) + color / (sample + 1)); // TODO: make other vector3s ints as well
                    // pixelColor = (int)(pixelColor * ((float) sample / (sample + 1)) + (float) ColorInt.Make(color) / (sample + 1)); // TODO: make other vector3s ints as well
                    sample++;
                }

                pixels[j * ImageSize.Width + i] = ColorInt.Make(pixelColor / Samples);
                // pixels[j * DisplayRegion.Width + i] = pixelColor;
            }
        }
    }
}
