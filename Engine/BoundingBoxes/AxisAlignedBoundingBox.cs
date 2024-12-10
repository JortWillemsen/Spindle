using Engine.BoundingBoxes;
using System.Numerics;
using Engine.Geometry;

namespace Engine.Scenes;

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

        // Prevent infinitely small dimensions of a bounding box
        const float minimalDimensionSize = 1E-5f;
        if (MathF.Abs(lowerBounds.X - upperBounds.X) < minimalDimensionSize)
            X.Max += minimalDimensionSize;
        if (MathF.Abs(lowerBounds.Y - upperBounds.Y) < minimalDimensionSize)
            Y.Max += minimalDimensionSize;
        if (MathF.Abs(lowerBounds.Z - upperBounds.Z) < minimalDimensionSize)
            Z.Max += minimalDimensionSize;
    }

    /// <summary>
    /// Creates an AABB that encapsulates multiple other AABBs
    /// </summary>
    /// <param name="boxes"></param>
    public AxisAlignedBoundingBox(params IBoundingBox[] boxes)
    {
        if (boxes.Length <= 0)
        {
            X = new Interval(0, 0);
            Y = new Interval(0, 0);
            Z = new Interval(0, 0);
            return;
        }
        
        // TODO: Remove dirty cast
        var aabbs = boxes.OfType<AxisAlignedBoundingBox>().ToArray();
        
        X = new Interval(aabbs.Select(b => b.X).ToArray());
        Y = new Interval(aabbs.Select(b => b.Y).ToArray());
        Z = new Interval(aabbs.Select(b => b.Z).ToArray());
    }
    /// <inheritdoc />
    public bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo)
    {
        // We do not intersect with a primitive, nor do we traverse the BVH hierarchy.
        // intersectionDebugInfo.NumberOfTraversals++;

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

            // Update the distanceInterval
            if (t0 > distanceInterval.Min) distanceInterval.Min = t0;
            if (t1 < distanceInterval.Max) distanceInterval.Max = t1;

            // If this is the case, we are still on track for an intersection, thus continue calculating other axis.
            if (distanceInterval.Max <= distanceInterval.Min)
            {
                intersection = Intersection.Undefined;
                return false;
            }
        }

        intersection = new Intersection { Distance = distanceInterval.Min };
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
}
