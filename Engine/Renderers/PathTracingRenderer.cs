using System.Numerics;
using Engine.Geometry;
using Engine.Materials;
using Engine.Scenes;

namespace Engine.Renderers;

public class PathTracingRenderer : IRenderer
{
    public Scene Scene { get; set; }

    public PathTracingRenderer(Scene scene)
    {
        Scene = scene;
    }
    
    public void TraceRay(Ray ray, int depth, ref Vector3 pixel, ref IntersectionDebugInfo intersectionInfo)
    {
        // TODO: Create shadow ray?
        if (depth <= 0)
        {
            pixel *= Vector3.Zero;
            return;
        }
        
        if (Scene.TryIntersect(ray, new Interval(0.001f, Utils.Infinity), out var intersection, ref intersectionInfo))
        {
            if (ScatterRay(ray, intersection, out var scatter))
            {
                TraceRay(scatter.Outgoing, depth - 1, ref pixel, ref intersectionInfo);
                pixel *= scatter.Color;

                return;
            }
        }
        
        var dirNormalized = ray.Direction.Normalized();

        float a = .5f * (dirNormalized.Y + 1f);
        pixel = (1f - a) * new Vector3(1f, 1f, 1f) + a * new Vector3(.5f, .7f, 1f );
    }

    public bool ScatterRay(Ray ray, Intersection intersection, out Scatter scatter)
    {
        if (intersection.Geometry.Material is Diffuse diffuse)
        {
            var scatterDir = Utils.RandomVectorHemisphere(intersection.Normal);

            if (scatterDir.NearZero())
                scatterDir = intersection.Normal;
        
            scatter = new Scatter()
            {
                Color = diffuse.Color * diffuse.Albedo,
                Outgoing = new Ray(intersection.Point, scatterDir)
            };
            return true;
        }

        if (intersection.Geometry.Material is Reflective reflective)
        {
            
            // Optimal reflection
            var reflectedDir = Vector3.Reflect(ray.Direction, intersection.Normal);
        
            // Adding roughness as a modifier on the reflection
            reflectedDir = Vector3.Normalize(reflectedDir) + reflective.Roughness * Utils.RandomVectorNormalized();

            if (Vector3.Dot(reflectedDir, intersection.Normal) > 0)
            {
                scatter = new Scatter
                {
                    Color = reflective.Color * reflective.Albedo,
                    Outgoing = new Ray(intersection.Point, reflectedDir)
                };

                return true;
            }

            scatter = new Scatter();
            return false;
        }

        scatter = new Scatter();
        return false;
    }
}
