using System.Drawing;
using System.Numerics;
using Engine.Renderers;

namespace Engine.Cameras;

public class DebugCamera : Camera
{
	// ReSharper disable once InconsistentNaming
	/// <inheritdoc />
	public DebugCamera(Vector3 position, Vector3 up, Vector3 front, Rectangle displayRegion, float FOV, int maxDepth, int samples)
		: base(position, up, front, displayRegion, FOV, maxDepth, samples)
	{
	}

	public override void RenderShot(IRenderer renderer, in Span<int> pixels)
	{
		for (var j = 0; j < this.DisplayRegion.Height; j++)
		{
			for (var i = 0; i < this.DisplayRegion.Width; i++)
			{
				var pixelColor = 0x00ff00;
				pixels[i + this.DisplayRegion.Width * j] = pixelColor;
			}
		}
	}
}