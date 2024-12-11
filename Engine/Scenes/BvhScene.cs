using Engine.AccelerationStructures.BoundingVolumeHierarchy;
using Engine.Geometry;
using Engine.Lighting;
using Engine.MeshImporters;
using Engine.Strategies.BVH;
using System.Diagnostics;

namespace Engine.Scenes;

// TODO: might we want to make this a decorator class?
/// <summary>
/// Variant of <see cref="Scene"/>, which implements a BVH acceleration structure, used to accelerate ray intersections.
/// </summary>
public class BvhScene : Scene
{
    private IBvhStrategy _bVHStrategy;
    /// <summary>
    /// The bounding volume hierarchy used for intersection calculation
    /// </summary>
    protected BvhNode _bvh;

    /// <summary>
    /// Creates a scene with BVH as an acceleration structure.
    /// </summary>
    /// <param name="strategy">Strategy used to create the BVH</param>
    /// <param name="objects"></param>
    /// <param name="lights"></param>
    public BvhScene(IBvhStrategy strategy, List<Geometry.Geometry> objects, List<LightSource> lights, Stopwatch sw) : base(objects, lights, sw)
    {
        _bVHStrategy = strategy;

        var start = _sw.ElapsedMilliseconds;
        _bvh = strategy.Build(this);
        var now = _sw.ElapsedMilliseconds - start;
        Console.WriteLine($"Done creating acceleration structure, {now}ms");

    }

    public BvhScene(IBvhStrategy strategy, List<Geometry.Geometry> objects, List<LightSource> lights, Stopwatch sw, params MeshImporter[] meshImporters)
        : base(objects, lights, sw, meshImporters)
    {
        _bVHStrategy = strategy;
        var start = _sw.ElapsedMilliseconds;
        _bvh = strategy.Build(this);
        var now = _sw.ElapsedMilliseconds - start;
        Console.WriteLine($"Done creating acceleration structure, {now}ms");
    }

    /// <inheritdoc />
    public override bool TryIntersect(Ray ray, Interval interval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo)
    {
        return _bvh.TryIntersect(ray, interval, out intersection, ref intersectionDebugInfo);
    }
}
