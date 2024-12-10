using Engine.BoundingBoxes;
using System.Numerics;
using Engine.Materials;
using Engine.Scenes;

namespace Engine.Geometry;

public abstract class Geometry : IIntersectable
{
    public Vector3 Position { get; protected set; }
    public Material Material { get; protected set; }
    
    protected Geometry(Vector3 position, Material material)
    {
        Position = position;
        Material = material;
    }

    public abstract Vector3 GetNormalAt(Vector3 pointOnGeometry);

    public abstract bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo);
    public abstract IBoundingBox GetBoundingBox();
}
