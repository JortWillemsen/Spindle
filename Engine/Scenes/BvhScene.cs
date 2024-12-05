using Engine.Geometry;
using Engine.Lighting;

namespace Engine.Scenes;

// TODO: might we want to make this a decorator class?
/// <summary>
/// Variant of <see cref="Scene"/>, which implements a BVH acceleration structure, used to accelerate ray intersections.
/// </summary>
public class BvhScene : Scene
{
    /// <summary>
    /// All Bounding Boxes defining the Bounding Box Volumes used to accelerate ray intersections.
    /// </summary>
    protected List<IBoundingBox> _boundingBoxes;

    /// <summary>
    /// Creates a scene with BVH as an acceleration structure.
    /// </summary>
    /// <param name="objects"></param>
    /// <param name="lights"></param>
    public BvhScene(List<Geometry.Geometry> objects, List<LightSource> lights) : base(objects, lights)
    {
        _boundingBoxes = CreateBoundingBoxes();
    }

    /// <summary>
    /// Divides all geometry in the scene into Bounding Box Volumes, which can be used to accelerate ray intersections.
    /// </summary>
    /// <returns>The Bounding Boxes defining the Bounding Box Volumes.</returns>
    private List<IBoundingBox> CreateBoundingBoxes()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override bool TryIntersect(Ray ray, Interval interval, out Intersection intersection)
    {
        throw new NotImplementedException();
    }
}
