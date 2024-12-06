using Engine.Geometry;
using Engine.Lighting;
using Engine.Strategies;
using Engine.Strategies.BVH;

namespace Engine.Scenes;

// TODO: might we want to make this a decorator class?
/// <summary>
/// Variant of <see cref="Scene"/>, which implements a BVH acceleration structure, used to accelerate ray intersections.
/// </summary>
public class BvhScene : Scene
{
    private IBVHStrategy _bVHStrategy;
    /// <summary>
    /// All Bounding Boxes defining the Bounding Box Volumes used to accelerate ray intersections.
    /// </summary>
    protected IBoundingBox _boundingBox;

    /// <summary>
    /// Creates a scene with BVH as an acceleration structure.
    /// </summary>
    /// <param name="strategy">Strategy used to create the BVH</param>
    /// <param name="objects"></param>
    /// <param name="lights"></param>
    public BvhScene(IBVHStrategy strategy, List<Geometry.Geometry> objects, List<LightSource> lights) : base(objects, lights)
    {
        _bVHStrategy = strategy;
        _boundingBox = CreateBoundingBoxes();
    }

    /// <summary>
    /// Divides all geometry in the scene into Bounding Box Volumes, which can be used to accelerate ray intersections.
    /// </summary>
    /// <returns>The Bounding Boxes defining the Bounding Box Volumes.</returns>
    private IBoundingBox CreateBoundingBoxes()
    {
        return _bVHStrategy.Build(this);
    }

    /// <inheritdoc />
    public override bool TryIntersect(Ray ray, Interval interval, out Intersection intersection)
    {
        throw new NotImplementedException();
    }
}
