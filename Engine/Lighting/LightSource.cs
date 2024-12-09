using Engine.BoundingBoxes;
using Engine.Geometry;
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

    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection) => throw new NotImplementedException();
    public abstract IBoundingBox GetBoundingBox();
}
