using Engine.Geometry;
using System.Drawing;
using System.Numerics;
using Engine.Renderers;

namespace Engine.Cameras;

/// <summary>
/// Creates a heat-map for the number of BVH traversals
/// </summary>
public class TraversalStepsCamera : Camera
{
    public int Samples { get; private set; }

    /// <summary>
    /// The coloring scale is based on 0 until this value.
    /// Values higher than this are clipped to the most intense color.
    /// </summary>
    public int DisplayedTraversalStepsRange { get; set; }

    // ReSharper disable once InconsistentNaming
    /// <inheritdoc />
    public TraversalStepsCamera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth, int samples, int displayedTraversalStepsRange)
        : base(position, up, front, imageSize, FOV, maxDepth)
    {
        Samples = samples;
        DisplayedTraversalStepsRange = displayedTraversalStepsRange;
    }

    public override void RenderShot(IRenderer renderer, in Span<int> pixels)
    {
        for (var j = 0; j < this.ImageSize.Height; j++)
        {
            for (var i = 0; i < this.ImageSize.Width; i++)
            {
                IntersectionDebugInfo intersectionDebugInfo = new();
                Vector3 pixelColor = Vector3.Zero;
                int sample = 0;

                while (sample < Samples)
                {
                    var ray = GetRayTowardsPixel(i, j);
                    renderer.TraceRay(ray, MaxDepth, ref pixelColor, ref intersectionDebugInfo);
                    sample++;
                }

                Vector3 color = Vector3.Lerp(new Vector3(0, 255, 0), new Vector3(255, 0, 0),
                    Math.Clamp((float) intersectionDebugInfo.NumberOfTraversals / DisplayedTraversalStepsRange, 0, 1f));

                pixels[j * ImageSize.Width + i] = ColorInt.Make(color);
            }
        }
    }
}
