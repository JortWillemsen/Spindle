using System.Numerics;

namespace Engine.Materials;

public class Reflective : Material
{
    public float Roughness { get; private set; }
    
    public Reflective(float albedo, Vector3 color, float roughness) : base(albedo, color)
    {
        Roughness = roughness;
    }
}