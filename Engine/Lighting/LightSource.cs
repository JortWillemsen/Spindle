using System.Numerics;

namespace Engine.Lighting;

public abstract class LightSource
{
	public Vector3 Position { get; }
	public Vector3 Color    { get; }

	protected LightSource(Vector3 position, Vector3 color)
	{
		Position = position;
		Color = color * 30; // TODO
	}
}