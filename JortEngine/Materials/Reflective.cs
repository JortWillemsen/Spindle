using Engine.Geometry.Interfaces;

namespace Engine.Materials;

public class Reflective : Material
{
    public Vector3 Albedo { get; private set; }
    public float Roughness { get; private set; }

    public Reflective(Vector3 albedo, float roughness)
    {
        Albedo = albedo;
        Roughness = roughness;
    }

    public override Scatter Scatter(Ray rayIn, SuccessRecord record)
    {
        // Optimal reflection
        var reflectedDir = rayIn.Direction.Reflect(record.Normal);
        
        // Adding roughness as a modifier on the reflection
        reflectedDir = Vector3.Unit(reflectedDir) + Roughness * Vector3.UnitRandom();

        if (Vector3.Dot(reflectedDir, record.Normal) > 0)
        {
            return new DoScatter
            {
                Albedo = Albedo,
                Outgoing = new Ray(record.Point, reflectedDir)
            }; 
        }

        return new NoScatter
        {
            Albedo = Albedo,
        };
    }
}