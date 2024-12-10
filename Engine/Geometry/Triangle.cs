using Engine.BoundingBoxes;
using Engine.Materials;
using System.Numerics;

namespace Engine.Geometry;

public class Triangle : Geometry
{
    public Vector3 Vertex1 { get; }
    public Vector3 Vertex2 { get; }
    public Vector3 Vertex3 { get; }

    private readonly Vector3 _normal;

    public Triangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Material material) : base(
        (vertex1 + vertex2 + vertex3) / 3, material)
    {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
        Vertex3 = vertex3;
        _normal = Vector3.Normalize(Vector3.Cross(Vertex2 - Vertex1, Vertex3 - Vertex1));
    }

    public override Vector3 GetNormalAt(Vector3 pointOnGeometry) => _normal;

    /// <inheritdoc />
    public override bool TryIntersect(Ray ray, Interval interval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo)
    {
        intersectionDebugInfo.NumberOfIntersectionTests++;
        intersection = Intersection.Undefined;

        // Moller-Trumbore intersection algorithm
        Vector3 edge1 = Vertex2 - Vertex1;
        Vector3 edge2 = Vertex3 - Vertex1;
        Vector3 h = Vector3.Cross(ray.Direction, edge2);
        float a = Vector3.Dot(edge1, h);

        if (a > -float.Epsilon && a < float.Epsilon)
            return false; // This ray is parallel to this triangle.

        float f = 1.0f / a;
        Vector3 s = ray.Origin - Vertex1;
        float u = f * Vector3.Dot(s, h);

        if (u < 0.0f || u > 1.0f)
            return false;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(ray.Direction, q);

        if (v < 0.0f || u + v > 1.0f)
            return false;

        // At this stage we can compute t to find out where the intersection point is on the line.
        float t = f * Vector3.Dot(edge2, q);

        if (t > interval.Min && t < interval.Max)
        {
            intersection = new Intersection
            {
                Distance = t,
                Point = ray.Origin + ray.Direction * t,
                Normal = _normal,
                Ray = ray,
                Geometry = this
            };
            return true;
        }

        return false;
    }

    public override IBoundingBox GetBoundingBox()
    {
        Vector3 lowerBounds = new Vector3(
            Math.Min(Vertex1.X, Math.Min(Vertex2.X, Vertex3.X)),
            Math.Min(Vertex1.Y, Math.Min(Vertex2.Y, Vertex3.Y)),
            Math.Min(Vertex1.Z, Math.Min(Vertex2.Z, Vertex3.Z))
        );

        Vector3 upperBounds = new Vector3(
            Math.Max(Vertex1.X, Math.Max(Vertex2.X, Vertex3.X)),
            Math.Max(Vertex1.Y, Math.Max(Vertex2.Y, Vertex3.Y)),
            Math.Max(Vertex1.Z, Math.Max(Vertex2.Z, Vertex3.Z))
        );

        return new AxisAlignedBoundingBox(lowerBounds, upperBounds);
    }
}
