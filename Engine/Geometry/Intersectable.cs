using Engine.BoundingBoxes;
using System.Numerics;

namespace Engine.Geometry;

public interface IIntersectable
{
    /// <summary>
    /// Determines whether a given ray intersects with this object, given that the ray intersects with this object
    /// within a range defined by <paramref name="distanceInterval"/>.
    /// </summary>
    /// <param name="ray">The ray which must be tested for intersections.</param>
    /// <param name="distanceInterval">No intersections will be tested where the distance traveled by the ray exceeds this distanceInterval.</param>
    /// <param name="intersection">The determined intersection, with all quick information about it.</param>
    /// <param name="intersectionDebugInfo">When not in release, is filled with debug info.</param>
    /// <returns>Whether an intersection was found, and thus <paramref name="intersection"/> is correctly defined.</returns>
    public bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo);

    public IBoundingBox GetBoundingBox();
    public Vector3 GetCentroid();
}

public record struct Intersection(float Distance, Vector3 Point, Vector3 Normal, Ray Ray, Geometry Geometry)
{
    public static Intersection Undefined => new(0f, Vector3.Zero, Vector3.Zero, null!, null!); // TODO: null reference, remove this whole field when you can
}
