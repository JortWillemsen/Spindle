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

    // All fields from interface
    /// <inheritdoc />
    public IBoundingBox LeftChild { get; set;  }

    /// <inheritdoc />
    public IBoundingBox RightChild { get; set;  }

    /// <inheritdoc />
    public Geometry.Geometry[] Primitives { get; }

    /// <inheritdoc />
    public bool IsLeaf { get; set;  }

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
    public AxisAlignedBoundingBox(Geometry.Geometry[] primitives, params AxisAlignedBoundingBox[] boxes)
    {
        Primitives = primitives;
        
        X = new Interval(boxes.Select(b => b.X).ToArray());
        Y = new Interval(boxes.Select(b => b.Y).ToArray());
        Z = new Interval(boxes.Select(b => b.Z).ToArray());
    }
    /// <inheritdoc />
    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection)
    {
        // Using the slab method with the formula t = (box bound - ray origin) / ray direction.
        
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
            float t0 = (ax.Min - ray.Origin.AxisByInt(axis) * adInv);
            float t1 = (ax.Max - ray.Origin.AxisByInt(axis) * adInv);
            
            if (t0 < t1)
            {
                // intersection is in reverse, thus we need to swap.
                if (t0 > interval.Min) interval.Min = t0;
                if (t1 < interval.Max) interval.Max = t1;
            }
            else
            {
                // Update the interval
                if (t1 > interval.Min) interval.Min = t1;
                if (t0 < interval.Max) interval.Max = t0;
            }

            // If this is the case, we are still on track for an intersection, thus continue calculating other axis.
            if (!(interval.Max <= interval.Min)) continue;
            
            // If not, we do not have an intersection.
            intersection = new Intersection();
            return false;
        }

        intersection = new Intersection();
        return true;
    }
}
