using System.Numerics;

namespace SilkSonic.Physics;

public struct Ray
{
	public Vector3 EntryPoint { get; set; }
	public Vector3 Direction  { get; set; }

	public Ray(Vector3 entryPoint, Vector3 direction) { // todo: create normalised alternative to prevent redundant normalisation
		EntryPoint = entryPoint;
		Direction = direction.Normalized();
	}

	public Vector3 GetPoint(float t) => EntryPoint + t * Direction;
}