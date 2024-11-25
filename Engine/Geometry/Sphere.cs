using System.Numerics;
using Engine.Exceptions;
using Engine.Materials;

namespace Engine.Geometry;

public class Sphere : Geometry
{
    public float Radius { get; protected set; }

    public Sphere(Vector3 position, Material material, float radius) : base(position, material)
    {
        if (radius <= 0)
            throw new InvalidGeometryException("The radius of a sphere cannot be negative or zero");
        
        Radius = radius;
    }

    public override bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection)
    {
        // Source: lecture notes (variable names as well)
        Vector3 c  = Position - ray.Origin;
        float   t  = Vector3.Dot(c, ray.Direction);
        Vector3 q  = c - t * ray.Direction;
        float   p2 = q.LengthSquared();
        
        if (p2 > Radius * Radius)
        {
            intersection = new Intersection();
            return false;
        }
        t -= MathF.Sqrt(Radius * Radius - p2);

        intersection = new Intersection(t, ray.Origin + t * ray.Direction, GetNormalAt(c), ray, this);
        // return distanceInterval.Contains(t);
        return true; // TODO: actually use the interval
    }

    public override Vector3 GetNormalAt(Vector3 pointOnObject)
        => (pointOnObject - Position) / Radius; // Accurate enough for normalization

    public bool Contains(Vector3 point)
    {
        float deltaX = point.X - Position.X;
        float deltaY = point.Y - Position.Y;
        float deltaZ = point.Z - Position.Z;
        return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ <= Radius * Radius;
    }
}