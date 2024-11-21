using Engine.Exceptions;
using Engine.Geometry.Interfaces;

namespace Engine.Materials;

public class LambertianDiffuse : Material
{
    public Vector3 Albedo { get; private set; }
    public float Absorption { get; private set; }

    public LambertianDiffuse(Vector3 albedo, float absorption)
    {
        if (absorption < 0 || absorption > 1)
            throw new InvalidGeometryException("Invalid absorption value");
        
        Albedo = albedo;
        Absorption = absorption;
    }

    public override Scatter Scatter(Ray rIn, SuccessRecord rec)
    {
        var scatterDir = rec.Normal + Vector3.UnitRandom();

        if (scatterDir.NearZero())
            scatterDir = rec.Normal;
        
        return new DoScatter
        {
            Albedo = Absorption * Albedo,
            Outgoing = new Ray(rec.Point, scatterDir)
        };
    }
}