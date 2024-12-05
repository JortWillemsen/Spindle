using System.Numerics;

namespace Engine.Materials;

public class Diffuse : Material
{ 
    public Diffuse(float albedo, Vector3 color) : base(albedo, color)
    {
    }
}
