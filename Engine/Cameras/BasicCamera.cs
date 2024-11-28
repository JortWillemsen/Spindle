using System.Drawing;
using System.Numerics;
using Engine.Renderers;

namespace Engine.Cameras;

public class BasicCamera : Camera
{
	// ReSharper disable once InconsistentNaming
	/// <inheritdoc />
	public BasicCamera(Vector3 position, Vector3 up, Vector3 front, Rectangle displayRegion, float FOV, int maxDepth, int samples)
		: base(position, up, front, displayRegion, FOV, maxDepth, samples)
	{
	}

	public override void RenderShot(IRenderer renderer, in Span<int> pixels)
	{
		for (var j = 0; j < this.DisplayRegion.Height; j++)
		{
			for (var i = 0; i < this.DisplayRegion.Width; i++)
			{
				int pixelColor = 0x0;

				for (var sample = 0; sample < this.Samples; sample++)
				{
					var ray = GetRayTowardsPixel(i, j);
					renderer.TraceRay(ray, MaxDepth, out var color);

					pixelColor += ColorInt.Make(color);
				}

				pixels[j * DisplayRegion.Width + i] = (int)(1f / Samples * pixelColor); // TODO: make other vector3s ints as well
			}
		}
	}
}