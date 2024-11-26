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

	public override Vector3[] RenderShot(IRenderer renderer)
	{
		var pixels = new Vector3[this.DisplayRegion.Width * this.DisplayRegion.Height];

		for (var j = 0; j < this.DisplayRegion.Height; j++)
		{
			for (var i = 0; i < this.DisplayRegion.Width; i++)
			{
				var pixelColor = Vector3.One;
				pixels[i + this.DisplayRegion.Width * j] = Vector3.Zero;
			}
		}

		return pixels;
	}
}