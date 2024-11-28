using System.Numerics;

namespace Engine.Materials;

public class Reflective : Material
{
    public float Roughness { get; private set; }
    
    public Reflective(Vector3 albedo, float roughness) : base(albedo)
    {
        Roughness = roughness;
    }
}