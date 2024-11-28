using System.Numerics;
using Engine.Exceptions;

namespace Engine.Materials;

public class Diffuse : Material
{ 
    public float Absorption { get; private set; }
    
    public Diffuse(Vector3 albedo, float absorption) : base(albedo)
    {
        if (absorption is < 0 or > 1)
            throw new InvalidGeometryException("Invalid absorption value");
        
        Albedo = albedo;
        Absorption = absorption;
    }
}