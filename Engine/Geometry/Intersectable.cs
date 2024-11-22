using System.Numerics;

namespace Engine.Geometry;

public interface IIntersectable
{
    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection);
}


public class Intersection
{
    public float Distance { get; private set; }
    public Vector3 Point { get; private set; }
    public Vector3 Normal { get; private set; }

    // TODO: Is this necessary
    public bool FrontFacing { get; private set; }
    public Geometry Geometry { get; private set; }

    public Intersection() { }
    
    public Intersection(float distance, Vector3 point, Vector3 outwardNormal, Ray ray, Geometry geometry)
    {
        Distance = distance;
        Point = point;
        Normal = outwardNormal;
        Geometry = geometry;
        SetFaceNormal(outwardNormal, ray);
    }

    public void SetFaceNormal(Vector3 outwardNormal, Ray ray)
    {
        var ff = Vector3.Dot(ray.Direction, outwardNormal) < 0;
        FrontFacing = ff;
        Normal = ff ? outwardNormal : -outwardNormal;
    }
}
