using System.Numerics;

namespace Engine.Materials;

public abstract class Material
{
    public Vector3 Albedo { get; protected set; }

    protected Material(Vector3 albedo)
    {
        Albedo = albedo;
    }
}
