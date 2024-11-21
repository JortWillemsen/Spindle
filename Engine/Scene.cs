using System.Collections;
using System.Numerics;
using Engine.Geometry.Interfaces;
using SilkSonic.Lighting;
using SilkSonic.Primitives;

namespace Engine.Geometry;

public class Scene
{
    public List<Geometry> Objects { get; private set; }
    public List<LightSource> Lights { get; private set; }
    
    public Scene(params Intersectable[] objects)
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

    public void AddPlane(Vector3 position, Material material, Vector3 normal)
        => Add(new Plane(position, material, normal));

    public void AddQuad(Vector3 position, Material material, Vector3 normal, Vector3 min, Vector3 max)
        => Add(new Quad(position, material, normal, min, max));

    public void AddLightSource(LightSource lightSource) => LightSources.Add(lightSource);
}