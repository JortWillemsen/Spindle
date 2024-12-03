using System.Numerics;
using Engine.Geometry;
using Engine.Materials;

namespace Engine.Renderers;

public interface IRenderer
{
    public Scene Scene { get; protected set; }

    public void TraceRay(Ray ray, int depth, ref Vector3 pixel);

    public bool ScatterRay(Ray ray, Intersection intersection, out Scatter scatter);
}

public struct Scatter
{
    public Vector3 Color { get; set; }
    public Ray Outgoing { get; set; }
}
