using Engine.Geometry;
using Engine.Scenes;

namespace Engine.AccelerationStructures.BoundingVolumeHierarchy;

public class BvhNode : IIntersectable
{
    public IBoundingBox BoundingBox;

    public bool IsLeaf;
    public BvhNode Right, Left;
    public IIntersectable[] Primitives;
    
    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection) => throw new NotImplementedException();
    public IBoundingBox GetBoundingBox() => BoundingBox;
}
