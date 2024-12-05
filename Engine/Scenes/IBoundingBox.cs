using Engine.Geometry;

namespace Engine.Scenes;

public interface IBoundingBox : IIntersectable
{
    /// <summary>
    /// The first if two child Bounding Boxes.
    /// </summary>
    public IBoundingBox   LeftChild  { get; }
    /// <summary>
    /// The second of two child Bounding Boxes.
    /// </summary>
    public IBoundingBox   RightChild { get; }
    /// <summary>
    /// All objects in the scene in the Bounding Box Volume defined by this Bounding Box.
    /// </summary>
    public IIntersectable Primitives { get; }
    /// <summary>
    /// Whether this Bounding Box cannot be traversed to find another bounding box and thus does not store primitives.
    /// </summary>
    public bool           IsLeaf     { get; }
}
