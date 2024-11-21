using System.Numerics;
using SilkSonic.Physics;

namespace SilkSonic.Primitives;

public abstract class Primitive
{
	public Vector3  Position { get; protected set; }
	public Material Material { get; protected set; }

	protected Primitive(Vector3 position, Material material) {
		Position = position;
		Material = material;
	}

	public abstract Vector3 GetNormalAt(Vector3 pointOnPrimitive);
	public abstract bool    TryIntersect(Ray ray, out Intersection intersection);
}