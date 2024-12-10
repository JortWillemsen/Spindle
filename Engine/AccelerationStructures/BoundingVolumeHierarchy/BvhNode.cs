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

    public bool TryIntersect(Ray ray, Interval distanceInterval, out Intersection intersection, ref IntersectionDebugInfo intersectionDebugInfo)
    {
        intersectionDebugInfo.NumberOfTraversals++;

        // If we intersect with the bounding box, we need to check the children
        if (BoundingBox.TryIntersect(ray, distanceInterval, out var boxIntersection, ref intersectionDebugInfo))
        {
            // If we are a leaf we intersect the primitive
            if (IsLeaf)
            {
                // TODO: intersect with all primitives
                return Primitives[0].TryIntersect(ray, distanceInterval, out intersection, ref intersectionDebugInfo);
            }

            var intersectsLeftBox = Left.BoundingBox.TryIntersect(ray, distanceInterval, out var leftIntersection, ref intersectionDebugInfo);
            var intersectsRightBox = Right.BoundingBox.TryIntersect(ray, distanceInterval, out var rightIntersection, ref intersectionDebugInfo);
            
            if (intersectsLeftBox && !intersectsRightBox)
            {
                return Left.TryIntersect(ray, distanceInterval, out intersection, ref intersectionDebugInfo);
            }

            if (!intersectsLeftBox && intersectsRightBox)
            {
                return Right.TryIntersect(ray, distanceInterval, out intersection, ref intersectionDebugInfo);
            }

            if (!intersectsLeftBox && !intersectsRightBox)
            {
                intersection = Intersection.Undefined;
                return false;
            }

            // We do intersect with both boxes, thus we recurse the one that is closest to us.
            if (leftIntersection.Distance < rightIntersection.Distance)
            {
                return Left.TryIntersect(ray, distanceInterval, out intersection, ref intersectionDebugInfo)
                       ||
                       Right.TryIntersect(ray, distanceInterval, out intersection, ref intersectionDebugInfo);
            }
            
            return Right.TryIntersect(ray, distanceInterval, out intersection, ref intersectionDebugInfo)
                ||
                Left.TryIntersect(ray, distanceInterval, out intersection, ref intersectionDebugInfo);
        }

        intersection = Intersection.Undefined;
        return false;
    }
}
