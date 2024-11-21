using System.Numerics;

namespace Engine.Cameras;

public class BasicCamera : Camera
{
	/// <inheritdoc />
	public BasicCamera(Vector3 position, int width, int height, Rectangle<int> textureScreenRegion)
		: base(position, width, height, textureScreenRegion)
	{
	}

	/// <inheritdoc />
	public override void RenderOneShot(in IRenderer renderer)
	{
		Span<int> pixels = Texture.GetWritablePixels(ViewportWidth, ViewportHeight);

		// Ray viewRay = new(Position, Vector3.One); // todo: actually calculate ray
		// renderer.DetermineRayColor(viewRay, pixels);

		for (int y = 0; y < ViewportHeight; y++)
		{
			for (int x = 0; x < ViewportWidth; x++)
			{
				byte intensity = (byte)Math.Abs((int)(y - ViewportHeight / 2));
				pixels[(int)(x + y * ViewportWidth)] = ColorInt.Make(0, 0, intensity);
			}
		}
	}
}