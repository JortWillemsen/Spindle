using System.Numerics;

namespace SilkSonic.Lighting;

public class Spotlight : LightSource
{
	/// <inheritdoc />
	public Spotlight(Vector3 position, Vector3 color) : base(position, color)
	{
	}
}