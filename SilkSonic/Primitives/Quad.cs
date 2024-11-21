using System.Numerics;
using SilkSonic.Physics;

namespace SilkSonic.Primitives;

public class Quad : Plane
{
	public Vector3 Min { get; protected set;}
	public Vector3 Max { get; protected set;}
	public Quad(Vector3 position, Material material, Vector3 normal, Vector3 min, Vector3 max)
		: base(position, material, normal) {
		Min = min;
		Max = max;
	}

	public override bool TryIntersect(Ray ray, out Intersection intersection)
	{
		if(base.TryIntersect(ray, out intersection))
		{
			if((intersection.Point - Position).LengthSquared() <= 1 * 1)
			{
				return true;
			}
		}
		intersection = new Intersection(ray, 0, this);
		return false;
	}
}