using Engine.Exceptions;
using Engine.Geometry.Interfaces;
using Engine.Materials;

namespace Engine.Geometry;

public class Sphere : Geometry
{
    public Vector3 Center { get; private set; }
    public float Radius { get; private set; }

    public Sphere(Vector3 center, float radius, Material material) : base(material)
    {
        if (radius <= 0)
            throw new InvalidGeometryException("Radius of a sphere cannot be <= 0");
        
        Center = center;
        Radius = Math.Max(0, radius);
    }
    
    // Function that returns point on the ray where it hit the sphere. 

    public override HitRecord Hit(Ray r, Interval t)
    {
        // Calculate intersection point
        var oc = Center - r.Origin;

        // Solve quadratic formula to determine hit
        var a = r.Direction.LengthSquared();
        var h = Vector3.Dot(r.Direction, oc);
        var c = oc.LengthSquared() - Radius * Radius;

        var discriminant = h*h - a*c;

        // No hit
        if (discriminant < 0)
            return new FailRecord();

        var squaredDiscriminant =  (float) Math.Sqrt(discriminant);

        var root = (h - squaredDiscriminant) / a;

        // Find the nearest root that lies in the acceptable interval
        if (!t.Surrounds(root))
        {
            root = (h + squaredDiscriminant) / a;

            if (!t.Surrounds(root))
                return new FailRecord();
        }

        // We do hit
        return new SuccessRecord(
            root, 
            r.At(root), 
            (r.At(root) - Center) / Radius, 
            r,
            this);

    }
}