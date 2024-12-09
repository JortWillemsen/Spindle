using Engine.BoundingBoxes;
using Engine.Geometry;
using System.Numerics;

namespace Engine.AccelerationStructures.BoundingVolumeHierarchy;

public class BvhNode : IIntersectable
{
    public IBoundingBox BoundingBox;

    public bool IsLeaf;
    public BvhNode Left, Right;
    public IIntersectable[] Primitives;
    
    public IBoundingBox GetBoundingBox() => BoundingBox;

    /// <inheritdoc />
    public Vector3 GetCentroid() => BoundingBox.GetCentroid();

    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection)
    {
        // If we intersect with the bounding box, we need to check the children
        if (BoundingBox.TryIntersect(ray, interval, out var boxIntersection))
        {
            // If we are a leaf we intersect the primitive
            if (IsLeaf)
            {
                return Primitives[0].TryIntersect(ray, interval, out intersection);
            }

            var intersectsLeftBox = Left.BoundingBox.TryIntersect(ray, interval, out var leftIntersection);
            var intersectsRightBox = Right.BoundingBox.TryIntersect(ray, interval, out var rightIntersection);
            
            if (intersectsLeftBox && !intersectsRightBox)
            {
                return Left.TryIntersect(ray, interval, out intersection);
            }

            if (!intersectsLeftBox && intersectsRightBox)
            {
                return Right.TryIntersect(ray, interval, out intersection);
            }

            if (!intersectsLeftBox && !intersectsRightBox)
            {
                intersection = Intersection.Undefined;
                return false;
            }

            // We do intersect with both boxes, thus we recurse the one that is closest to us.
            if (leftIntersection.Distance < rightIntersection.Distance)
            {
                return Left.TryIntersect(ray, interval, out intersection) // TODO: return the one which is closest, always
                       ||
                       Right.TryIntersect(ray, interval, out intersection);
            }
            
            return Right.TryIntersect(ray, interval, out intersection) // TODO: return the one which is closest, always
                || 
                Left.TryIntersect(ray, interval, out intersection);
        }

        intersection = Intersection.Undefined;
        return false;
    }
}
