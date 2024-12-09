using Engine.Geometry;
using System.Numerics;

namespace Engine.BoundingBoxes;

/// <summary>
/// A Bounding Box aligned to the 3D axes.
/// </summary>
public class AxisAlignedBoundingBox : IBoundingBox
{
    /// <summary>
    /// The axes of the bounding box in 3D space
    /// </summary>
    public Interval X, Y, Z;

    /// <summary>
    /// Creates an AABB that is empty
    /// </summary>
    public AxisAlignedBoundingBox()
    {
        X = new Interval();
        Y = new Interval();
        Z = new Interval();
    }
    
    /// <summary>
    /// Creates an AABB.
    /// </summary>
    /// <param name="lowerBounds">The lower boundary of what this AABB encapsulates.</param>
    /// <param name="upperBounds">The upper boundary of what this AABB encapsulates.</param>
    public AxisAlignedBoundingBox(Vector3 lowerBounds, Vector3 upperBounds)
    {
        X = (lowerBounds.X <= upperBounds.X)
            ? new Interval(lowerBounds.X, upperBounds.X)
            : new Interval(upperBounds.X, lowerBounds.X);
        Y = (lowerBounds.Y <= upperBounds.Y)
            ? new Interval(lowerBounds.Y, upperBounds.Y)
            : new Interval(upperBounds.Y, lowerBounds.Y);
        Z = (lowerBounds.Z <= upperBounds.Z)
            ? new Interval(lowerBounds.Z, upperBounds.Z)
            : new Interval(upperBounds.Z, lowerBounds.Z);
    }

    /// <summary>
    /// Creates an AABB that encapsulates multiple other AABBs
    /// </summary>
    /// <param name="boxes"></param>
    public AxisAlignedBoundingBox(params IBoundingBox[] boxes)
    {
        // TODO: Remove dirty cast
        var aabbs = boxes.OfType<AxisAlignedBoundingBox>().ToArray();
        
        X = new Interval(aabbs.Select(b => b.X).ToArray());
        Y = new Interval(aabbs.Select(b => b.Y).ToArray());
        Z = new Interval(aabbs.Select(b => b.Z).ToArray());
    }
    /// <inheritdoc />
    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection)
{
    for (int axis = 0; axis < 3; axis++)
    {
        // Finding the correct axis.
        Interval ax = axis switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "No such axis")
        };

        // We need the inverse of the direction since it is much faster to calculate using multiplication.
        float adInv = 1f / ray.Direction.AxisByInt(axis);

        // t values that determine the intersection of the ray with the minimum and maximum bounds of the box.
        float t0 = (ax.Min - ray.Origin.AxisByInt(axis)) * adInv;
        float t1 = (ax.Max - ray.Origin.AxisByInt(axis)) * adInv;

        if (t0 > t1)
        {
            // Swap t0 and t1 if t0 is greater than t1
            (t0, t1) = (t1, t0);
        }

        // Update the interval
        if (t0 > interval.Min) interval.Min = t0;
        if (t1 < interval.Max) interval.Max = t1;

        // If this is the case, we are still on track for an intersection, thus continue calculating other axis.
        if (interval.Max <= interval.Min)
        {
            intersection = Intersection.Undefined;
            return false;
        }
    }

    intersection = new Intersection { Distance = interval.Min };
    return true;
}


    public IBoundingBox GetBoundingBox() => this;

    public IBoundingBox Combine(IBoundingBox[] boxes)
    {
        //TODO: Dirty cast, need to figure out a way to abstract this logic.
        return new AxisAlignedBoundingBox((AxisAlignedBoundingBox[]) boxes);
    }

    public Interval AxisByInt(int axis)
    {
        return axis switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "No such axis")
        };
    }

    /// <inheritdoc />
    public Vector3 GetLowerBound() => new(X.Min, Y.Min, Z.Min);

    /// <inheritdoc />
    public Vector3 GetUpperBound() => new(X.Max, Y.Max, Z.Max);
}
