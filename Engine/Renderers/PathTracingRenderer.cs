using System.Numerics;
using Engine.Geometry;
using Engine.Materials;
using Engine.Renderers;
using SilkSonic;

namespace Engine;

public class PathTracingRenderer : IRenderer
{
    public Scene Scene { get; set; }

    public PathTracingRenderer(Scene scene)
    {
        Scene = scene;
    }
    
    public void TraceRay(Ray ray, int depth, out Vector3 pixel)
    {
        // TODO: Create shadow ray?
        if (depth <= 0)
            pixel = Vector3.Zero;
        
        if (Scene.TryIntersect(ray, new Interval(0.001f, Utils.Infinity), out var intersection))
        {
            var scatter = this.ScatterRay(ray, intersection);

            TraceRay(scatter.Outgoing, depth - 1, out pixel);

            pixel *= scatter.Albedo;
            return;

        }
        
        var dirNormalized = ray.Direction.Normalized();

        var a = .5f * (dirNormalized.Y + 1f);
        pixel = (1f - a) * new Vector3(1f, 1f, 1f) + a * new Vector3(.5f, .7f, 1f );
    }

    public Scatter ScatterRay(Ray ray, Intersection intersection)
    {
        if (intersection.Geometry.Material is Diffuse diffuse)
        {
            var scatterDir = intersection.Normal + Utils.RandomVectorNormalized();

            if (scatterDir.NearZero())
                scatterDir = intersection.Normal;
        
            return new Scatter
            {
                Albedo = diffuse.Absorption * diffuse.Albedo,
                Outgoing = new Ray(intersection.Point, scatterDir)
            };
        }
        
        return new Scatter
        {
            Albedo = intersection.Geometry.Material.Albedo,
            Outgoing = new Ray(ray.Origin, Vector3.Zero)
        };
    }
}