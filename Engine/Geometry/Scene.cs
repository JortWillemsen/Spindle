using System.Collections;
using System.Numerics;
using Engine.Materials;
using SilkSonic.Lighting;
using SilkSonic.Primitives;

namespace Engine.Geometry;

public class Scene : IIntersectable
{
    public List<Geometry> Objects { get; private set; }
    public List<LightSource> Lights { get; private set; }
    
    public Scene(params Geometry[] objects)
    {
        Objects = objects.ToList();
    }

    public void Add(Geometry obj)
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
    
    // Find closest intersection of all objects in the scene
    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection)
    {
        var intersected = false;
        // Current closest intersection, currently infinite for we have no intersection.
        var closest = interval.Max;

        // TODO: This feels dirty
        var intersection1 = new Intersection();
        
        // Loop over all the geometry in the scene to determine what the ray hits.
        foreach (var obj in Objects)
        {
            if (!obj.TryIntersect(ray, new Interval(interval.Min, closest), out intersection1))
               continue;
            
            intersected = true;
            closest = intersection1.Distance;

        }

        intersection = intersection1;
        return intersected;
    }
}