using Engine.BoundingBoxes;
using System.Numerics;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Materials;
using Engine.MeshImporters;

namespace Engine.Scenes;

public class Scene : IIntersectable
{
    public List<Geometry.Geometry> Objects { get; private set; }
    public List<LightSource>       Lights  { get; private set; }
    
    public Scene(List<Geometry.Geometry> objects, List<LightSource> lights, params MeshImporter[] meshImporters)
        : this(objects, lights)
    {
        foreach (MeshImporter meshImporter in meshImporters)
            AddObjects(meshImporter.Import());
    }

    public Scene(List<Geometry.Geometry> objects, List<LightSource> lights)
    {
        Objects = objects;
        Lights = lights;
    }

    public void AddObject(Geometry.Geometry obj)
    {
        Objects.Add(obj);
    }

    public void AddObjects(IEnumerable<Geometry.Geometry> objects)
    {
        Objects.AddRange(objects);
    }

    public IIntersectable[] GetIntersectables()
    {
        var intersectables = new List<IIntersectable>();

        return intersectables.Concat(Objects).Concat(Lights).ToArray();
    }
    
    public void AddSphere(Vector3 position, Material material, float radius)
        => AddObject(new Sphere(position, material, radius));

    public void AddLightSource(LightSource lightSource) => Lights.Add(lightSource);
    
    // Find nearest intersection of all objects in the scene
    public virtual bool TryIntersect(Ray ray, Interval interval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo)
    {
        var intersected = false;
        // Current closest intersection, currently infinite for we have no intersection.
        var closest = interval.Max;

        // TODO: This feels dirty
        var storedIntersection = Intersection.Undefined;
        
        // Loop over all the geometry in the scene to determine what the ray hits.
        foreach (Geometry.Geometry obj in Objects)
        {
            // If we don't intersect, we continue checking the remaining objects.
            if (!obj.TryIntersect(ray, new Interval(interval.Min, closest), out var newIntersection, ref intersectionDebugInfo))
               continue;
            
            // When we do hit, we set the closest to the new intersection (intersection2)
            intersected = true;
            closest = newIntersection.Distance;
            storedIntersection = newIntersection;
        }

        intersection = storedIntersection;
        return intersected;
    }

    public IBoundingBox GetBoundingBox()
    {
        return new AxisAlignedBoundingBox(Objects.Select(o => o.GetBoundingBox()).ToArray());
    }
}
