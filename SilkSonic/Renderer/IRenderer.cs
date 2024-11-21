using System.Numerics;
using SilkSonic.Physics;

namespace SilkSonic.Renderer;

public interface IRenderer
{
	public Scene Scene { get; protected set; }

	public void DetermineRayColor(Ray viewRay, Span<int> pixels);
}