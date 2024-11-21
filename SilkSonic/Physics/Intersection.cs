using System.Numerics;
using SilkSonic.Primitives;

namespace SilkSonic.Physics;

public readonly ref struct Intersection
{
	public Ray       IntersectedRay   { get; }
	public Primitive Primitive        { get; }
	public Vector3   Point            { get; }
	public Vector3   Normal           { get; }
	public float     DistanceTraveled { get; }

	public Intersection(Ray intersectedRay, float distanceTraveled, Primitive primitive)
	{
		IntersectedRay = intersectedRay;
		Primitive = primitive;
		DistanceTraveled = distanceTraveled;

		Point = intersectedRay.GetPoint(distanceTraveled); // todo: perhaps compute this lazily?
		// if(primitive != null) // TODO
		// {
		Normal = primitive.GetNormalAt(Point);
		// }
	}
}