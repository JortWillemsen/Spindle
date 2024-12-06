using System.Numerics;
using Engine.Geometry;

namespace Engine.Scenes;

/// <summary>
/// A Bounding Box aligned to the 3D axes.
/// </summary>
public class AxisAlignedBoundingBox : IBoundingBox
{
    /// <summary>
    /// The corner towards all positive axes.
    /// </summary>
    public Vector3 UpperBounds;

    /// <summary>
    /// The corner towards all negative axes.
    /// </summary>
    public Vector3 LowerBounds;

    // All fields from interface
    /// <inheritdoc />
    public IBoundingBox LeftChild { get; }

    /// <inheritdoc />
    public IBoundingBox RightChild { get; }

    /// <inheritdoc />
    public IIntersectable Primitives { get; }

    /// <inheritdoc />
    public bool IsLeaf { get; }

    /// <summary>
    /// Creates an AABB.
    /// </summary>
    /// <param name="upperBounds">The upper boundary of what this AABB encapsulates.</param>
    /// <param name="lowerBounds">The lower boundary of what this AABB encapsulates.</param>
    public AxisAlignedBoundingBox(Vector3 upperBounds, Vector3 lowerBounds)
    {
        UpperBounds = upperBounds;
        LowerBounds = lowerBounds;

        // TODO: assign fields
    }

    /// <inheritdoc />
    public PossibleIntersection FindIntersection(Ray ray, Interval interval) => throw new NotImplementedException();
}
