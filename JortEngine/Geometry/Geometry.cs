using Engine.Geometry.Interfaces;
using Engine.Materials;

namespace Engine.Geometry;

public abstract class Geometry : Hittable
{
    public Material Mat { get; private set; }
    
    protected Geometry(Material mat)
    {
        Mat = mat;
    }

    public abstract Intersection Hit(Ray r, Interval t);
}