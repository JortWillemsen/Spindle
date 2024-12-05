using System.Numerics;

namespace Engine.Geometry;

public interface IIntersectable
{
    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection);
}


public record struct Intersection(float Distance, Vector3 Point, Vector3 Normal, Ray Ray, Geometry Geometry)
{
    public static Intersection Undefined => new(0f, Vector3.Zero, Vector3.Zero, null!, null!); // TODO: null reference, remove this whole field when you can
}
