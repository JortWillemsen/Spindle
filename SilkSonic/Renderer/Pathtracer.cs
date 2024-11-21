using System.Numerics;
using SilkSonic.Cameras;
using SilkSonic.Physics;

namespace SilkSonic.Renderer;

public class Pathtracer : IRenderer
{
	public Pathtracer(Scene scene)
	{
		Scene = scene;
	}

	/// <inheritdoc />
	public Scene Scene { get; set; }

	/// <inheritdoc />
	public void DetermineRayColor(Ray viewRay, Span<int> pixels)
	{
		throw new NotImplementedException();
	}
}