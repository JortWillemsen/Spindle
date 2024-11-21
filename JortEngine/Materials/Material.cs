using Engine.Geometry.Interfaces;

namespace Engine.Materials;

public abstract class Material
{
    public abstract Scatter Scatter(Ray rayIn, SuccessRecord record);
}

public abstract class Scatter
{
    public Vector3 Albedo { get; set; }
}

public class DoScatter : Scatter
{
    public Ray Outgoing { get; set; }

}

public class NoScatter : Scatter
{
    
}