using System.Numerics;
using OneOf;
using OneOf.Types;

namespace Engine.Geometry;

public interface IIntersectable
{
    /// <summary>
    /// Determines whether a given ray intersects with this object, given that the ray intersects with this object
    /// within a range defined by <paramref name="interval"/>.
    /// </summary>
    /// <param name="ray">The ray which must be tested for intersections.</param>
    /// <param name="interval">No intersections will be tested where the distance traveled by the ray exceeds this interval.</param>
    /// <returns>One of Intersection and None</returns>
    public PossibleIntersection FindIntersection(Ray ray, Interval interval);
}

public record Intersection(float Distance, Vector3 Point, Vector3 Normal, Ray Ray, Geometry Geometry);

[GenerateOneOf]
public partial class PossibleIntersection : OneOfBase<Intersection, None>
{
    public bool FoundIndeedAnIntersection(out Intersection intersection) => TryPickT0(out intersection, out _);
}
