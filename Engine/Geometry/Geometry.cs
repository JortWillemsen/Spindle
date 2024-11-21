using System.Numerics;
using Engine.Geometry.Interfaces;
using Engine.Materials;

namespace Engine.Geometry;

public abstract class Geometry : Intersectable
{
    public Vector3 Position { get; protected set; }
    public Material Material { get; protected set; }
    
    protected Geometry(Position position, Material material)
    {
        Position = position;
        Material = material;
    }

    public abstract Vector3 GetNormalAt(Vector3 pointOnGeometry)
    {
        
    }

    public abstract bool TryIntersect(Ray ray, out Intersection intersection);
}