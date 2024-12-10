using Engine.BoundingBoxes;
using Engine.Geometry;
using Engine.Scenes;
using System.Numerics;

namespace Engine.Lighting;

public abstract class LightSource : IIntersectable
{
    public Vector3 Position { get; }
    public Vector3 Color    { get; }

    protected LightSource(Vector3 position, Vector3 color)
    {
        Position = position;
        Color = color * 30; // TODO
    }

    /// <inheritdoc />
    public bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection,
        ref IntersectionDebugInfo intersectionDebugInfo) =>
        throw new NotImplementedException();

    public abstract IBoundingBox GetBoundingBox();
}
