using Engine.Geometry;
using Engine.Scenes;

namespace Engine.AccelerationStructures.BoundingVolumeHierarchy;

public class BvhNode : IIntersectable
{
    public IBoundingBox BoundingBox;

    public bool IsLeaf;
    public BvhNode Right, Left;
    public IIntersectable[] Primitives;
    
    public IBoundingBox GetBoundingBox() => BoundingBox;

    public bool TryIntersect(Ray ray, Interval interval, out Intersection intersection)
    {
        // If we intersect with the bounding box, we need to check the children
        if (BoundingBox.TryIntersect(ray, interval, out intersection))
        {
            // If we are a leaf we intersect the primitives
            if (IsLeaf)
            {
                return TryIntersectPrimitives(ray, interval, out intersection);
            }
            
            if (Left.TryIntersect(ray, interval, out var leftIntersection))
            {
                // If we intersect on the left, we only check for intersections on the right that are closer.
                if (Right.TryIntersect(ray, new Interval(interval.Min, leftIntersection.Distance), out var rightIntersection))
                {
                    intersection = rightIntersection;
                    return true;
                }

                // We did not intersect on the right so the left intersection is the only one we got.
                intersection = leftIntersection;
                return true;
            }
            
            // We did not intersect on the left so we return whatever right intersects with.
            return Right.TryIntersect(ray, interval, out intersection);

        }

        return false;
    }

    private bool TryIntersectPrimitives(Ray ray, Interval interval, out Intersection intersection)
    {
        var intersected = false;
        // Current closest intersection, currently infinite for we have no intersection.
        var closest = interval.Max;

        // TODO: This feels dirty
        var storedIntersection = Intersection.Undefined;
        
        // Loop over all the geometry in the scene to determine what the ray hits.
        foreach (var obj in Primitives)
        {
            // If we don't intersect, we continue checking the remaining objects.
            if (!obj.TryIntersect(ray, new Interval(interval.Min, closest), out var newIntersection))
                continue;
            
            // When we do hit, we set the closest to the new intersection (intersection2)
            intersected = true;
            closest = newIntersection.Distance;
            storedIntersection = newIntersection;
        }

        intersection = storedIntersection;
        return intersected;
    }
}
