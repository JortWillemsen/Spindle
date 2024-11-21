using Engine.Geometry.Interfaces;

namespace Engine.Materials;

public class Dielectric : Material
{
    private float RefrectiveIndex { get; set; }

    public Dielectric(float refrectiveIndex)
    {
        RefrectiveIndex = refrectiveIndex;
    }

    public override Scatter Scatter(Ray rayIn, SuccessRecord record)
    {
        var attenuation = new Vector3(1f, 1f, 1f);
        var ri = record.FrontFacing ? (1f / RefrectiveIndex) : RefrectiveIndex;

        var unitDir = Vector3.Unit(rayIn.Direction);

        var cosTheta = Math.Min(Vector3.Dot(-unitDir, record.Normal), 1f);
        var sinTheta = (float)Math.Sqrt(1f - cosTheta * cosTheta);

        var cannotRefract = ri * sinTheta > 1f;

        Vector3 dir;
        
        if (cannotRefract || Reflectance(cosTheta, ri) > Utils.RandomFloat(Utils.GetRandom()))
        {
            dir = unitDir.Reflect(record.Normal);
        }
        else
        {
            dir = unitDir.Refract(record.Normal, ri);
        }
        
        return new DoScatter
        {
            Albedo = attenuation,
            Outgoing = new Ray(record.Point, dir)
        };
    }
    
    static float Reflectance(float cosine, float refractiveIndex) {
        // Use Schlick's approximation for reflectance.
        var r0 = (1f - refractiveIndex) / (1f + refractiveIndex);
        r0 = r0*r0;
        
        return r0 + (1f-r0)* (float) Math.Pow((1f - cosine),5f);
    }
}