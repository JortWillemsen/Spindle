using Engine.BoundingBoxes;
using System.Numerics;
using Engine.Materials;

namespace Engine.Geometry;

public class Plane : Geometry
{
    // todo: build based on Numerics.Plane?

    public Vector3 Normal { get; protected set; }

    /// <inheritdoc />
    public Plane(Vector3 position, Material material, Vector3 normal) : base(position, material)
    {
        Normal = normal;
    }

    /// <inheritdoc />
    public override bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo)
    {
        intersectionDebugInfo.NumberOfIntersectionTests++;
            
        // Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
        float denominator = Vector3.Dot(ray.Direction, Normal);
        if (denominator > 1e-6)
        {
            // ReSharper disable once InconsistentNaming
            Vector3 p0l0 = Position - ray.Origin;
            float t = Vector3.Dot(p0l0, Normal) / denominator;
            intersection = new Intersection(t, ray.At(t), Normal, ray, this);
            return t >= 0 && distanceInterval.Contains(t);
        }

        intersection = Intersection.Undefined;
        return false;
    }

    public override IBoundingBox GetBoundingBox() => throw new NotImplementedException();

    /// <inheritdoc />
    public override Vector3 GetCentroid() => Position;

    public override Vector3 GetNormalAt(Vector3 pointOnObject) => Normal;
}
