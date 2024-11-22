using System.Numerics;
using Engine.Geometry;
using Engine.Materials;

namespace Engine.Renderers;

public interface IRenderer
{
    public Scene Scene { get; protected set; }

    public void TraceRay(Ray ray, int depth, out Vector3 pixel);

    public Scatter ScatterRay(Ray ray, Intersection intersection);
}

public struct Scatter
{
    public Vector3 Albedo { get; set; }
    public Ray Outgoing { get; set; }
}
