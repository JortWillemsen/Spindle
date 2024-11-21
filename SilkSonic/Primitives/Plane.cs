using System.Numerics;
using SilkSonic.Physics;

namespace SilkSonic.Primitives;

public class Plane : Primitive
{
	// todo: build based on Numerics.Plane?
	public Vector3 Normal { get; protected set; }

	public Plane(Vector3 position, Material material, Vector3 normal) : base(position, material) {
		Normal = normal.Normalized();
	}

	public override bool TryIntersect(Ray ray, out Intersection intersection)
	{
		// Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
		float denominator = Vector3.Dot(ray.Direction, Normal);
		if (denominator > 1e-6)
		{
			Vector3 p0l0 = Position - ray.EntryPoint;
			float t = Vector3.Dot(p0l0, Normal) / denominator;
			intersection = new Intersection(ray, t, this);
			return t >= 0;
		}

		intersection = new Intersection(ray, 0, this);
		return false;
	}

	public override Vector3 GetNormalAt(Vector3 pointOnObject) => Normal;
}