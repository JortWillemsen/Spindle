using System.Numerics;
using Engine.Exceptions;

namespace Engine.Materials;

public class Diffuse : Material
{ 
    public Diffuse(float albedo, Vector3 color) : base(albedo, color)
    {
    }
}