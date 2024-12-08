using Engine.BoundingBoxes;
using System.Numerics;
using Engine.Exceptions;
using Engine.Materials;
using Engine.Scenes;

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

    public override bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo)
    {
        intersectionDebugInfo.NumberOfIntersectionTests++;

        // Calculate intersection point
        var oc = Position - ray.Origin;

        // Solve quadratic formula to determine hit
        var a = ray.Direction.LengthSquared();
        var h = Vector3.Dot(ray.Direction, oc);
        var c = oc.LengthSquared() - Radius * Radius;

        var discriminant = h*h - a*c;

        // No hit
        if (discriminant < 0)
        {
            intersection = Intersection.Undefined;
            return false;
        }

        var squaredDiscriminant =  (float) Math.Sqrt(discriminant);

        var root = (h - squaredDiscriminant) / a;

        // Find the nearest root that lies in the acceptable interval
        if (!distanceInterval.Surrounds(root))
        {
            root = (h + squaredDiscriminant) / a;

            if (!distanceInterval.Surrounds(root))
            {
                intersection = Intersection.Undefined;
                return false;
            }
        }

        // We do hit
        var intersectionPoint = ray.At(root);
        intersection = new Intersection(root, intersectionPoint, GetNormalAt(intersectionPoint), ray, this);
        return true;
    }
    
    public override IBoundingBox GetBoundingBox()
    {
        Vector3 radiusVector = new Vector3(Radius, Radius, Radius);

        return new AxisAlignedBoundingBox(Position - radiusVector, Position + radiusVector);
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
