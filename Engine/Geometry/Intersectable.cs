using System.Numerics;
using OneOf;
using OneOf.Types;

namespace Engine.Geometry;

public interface IIntersectable
{
    public PossibleIntersection FindIntersection(Ray ray, Interval interval);
}


public record Intersection(float Distance, Vector3 Point, Vector3 Normal, Ray Ray, Geometry Geometry);

[GenerateOneOf]
public partial class PossibleIntersection : OneOfBase<Intersection, None>
{
    public bool FoundIndeedAnIntersection(out Intersection intersection) => TryPickT0(out intersection, out _);
}
