using System.Numerics;
using Engine.Renderers;

namespace Engine.Cameras;

public class DebugCamera : Camera
{
	/// <inheritdoc />
	public DebugCamera(float aspectRatio, float focalLength, int imageWidth, int imageHeight, int maxDepth, int samples, Vector3 position, Vector3 up, Vector3 front)
		: base(aspectRatio, focalLength, imageWidth, imageHeight, maxDepth, samples, position, up, front)
	{
	}

	/// <inheritdoc />
	public override Vector3[] RenderShot(IRenderer renderer)
	{
		var pixels = new Vector3[this.ImageWidth * this.ImageHeight];

		for (var j = 0; j < this.ImageHeight; j++)
		{
			for (var i = 0; i < this.ImageWidth; i++)
			{
				var pixelColor = Vector3.One;
				pixels[i + ImageWidth * j] = Vector3.Zero;

				// for (var sample = 0; sample < this.Samples; sample++)
				// {
				// 	var ray = GetRay(i, j);
				// 	renderer.TraceRay(ray, MaxDepth, out var color);
				//
				// 	pixelColor += color;
				// }

				// pixels[j * ImageWidth + i] = Utils.RgbToInt(1f / Samples * pixelColor);
			}
		}

		return pixels;
	}
}