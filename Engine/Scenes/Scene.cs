using System.Numerics;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Materials;
using OneOf.Types;

namespace Engine.Scenes;

public class Scene : IIntersectable
{
    public List<Geometry.Geometry> Objects { get; private set; }
    public List<LightSource>       Lights  { get; private set; }
    
    public Scene(List<Geometry.Geometry> objects, List<LightSource> lights)
    {
        Objects = objects;
        Lights = lights;
    }

    public void Add(Geometry.Geometry obj)
    {
        Objects.Add(obj);
    }
    
    public void Add(LightSource obj)
    {
        Lights.Add(obj);
    }
    
    public void AddSphere(Vector3 position, Material material, float radius)
        => Add(new Sphere(position, material, radius));

    public void AddLightSource(LightSource lightSource) => Lights.Add(lightSource);
    
    // Find nearest intersection of all objects in the scene
    public virtual PossibleIntersection FindIntersection(Ray ray, Interval interval)
    {
        // Current closest intersection, currently infinite for we have no intersection.
        float closest = interval.Max;
        PossibleIntersection storedIntersection = new None();

        // Loop over all the geometry in the scene to determine what the ray hits.
        foreach (var obj in Objects)
        {
            // If we don't intersect, we continue checking the remaining objects.
            obj.FindIntersection(ray, new Interval(interval.Min, closest)).Switch(
                // When we do hit, we set the closest to the new intersection
                intersection =>
                {
                    closest = intersection.Distance;
                    storedIntersection = intersection;
                },
                // If we do not intersect, do nothing
                _ => { });
        }

        return storedIntersection;
    }
}
