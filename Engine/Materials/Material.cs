using System.Numerics;
using Engine.Exceptions;

namespace Engine.Materials;

public abstract class Material
{
    // Amount of light that is retained after bouncing
    public float Albedo { get; protected set; }
    public Vector3 Color { get; protected set; }

    protected Material(float albedo, Vector3 color)
    {
        if (albedo is < 0 or > 1)
            throw new InvalidGeometryException("Invalid absorption value");
        
        Albedo = albedo;
        Color = color;
    }
}
