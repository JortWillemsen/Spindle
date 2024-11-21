using System.Numerics;
using Silk.NET.Maths;
using SilkSonic.Renderer;

namespace SilkSonic.Cameras;

public class DebugCamera : Camera
{
	/// <inheritdoc />
	public DebugCamera(Vector3 position, int width, int height, Rectangle<int> textureScreenRegion)
		: base(position, width, height, textureScreenRegion)
	{
	}

	/// <inheritdoc />
	public override void RenderOneShot(in IRenderer renderer)
	{
		throw new NotImplementedException();
	}
}