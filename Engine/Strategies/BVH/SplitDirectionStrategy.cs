using Engine.AccelerationStructures.BoundingVolumeHierarchy;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Scenes;

namespace Engine.Strategies.BVH;

public class SplitDirectionStrategy : IBvhStrategy
{
    public BvhNode Build(Scene scene)
    {
        // Create the root bounding box.
        var root = new BvhNode
        {
            IsLeaf = false, BoundingBox = scene.GetBoundingBox(), Primitives = scene.Objects.ToArray()
        };

        return PopulateChildren(root);
    }

    private BvhNode PopulateChildren(BvhNode root)
    {
        Stack<BvhNode> stack = new();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var parent = stack.Pop();

            // Base case
            if (parent.Primitives.Length <= 3)
            {
                parent.IsLeaf = true;
                continue;
            }

            int axisToSplit = 0;

            // Determine which axis to split i.e. the longest axis.
            for (int axis = 0; axis < 3; axis++)
            {
                // If the size of the current axis is longer than the selected axis, we substitute it.
                if (parent.BoundingBox.AxisByInt(axis).Size > parent.BoundingBox.AxisByInt(axisToSplit).Size)
                {
                    axisToSplit = axis;
                }
            }

            // Find the split intervals
            (Interval fst, _) = parent.BoundingBox.AxisByInt(axisToSplit).Split();

            var primsFst = new List<IIntersectable>();
            var primsSnd = new List<IIntersectable>();

            // Divide the primitives based on their position in 3D space.
            foreach (IIntersectable primitive in parent.Primitives)
            {
                Geometry.Geometry geometry = (Geometry.Geometry)primitive;
                // Check if it falls inside the first or second box and append accordingly.
                if (fst.Surrounds(geometry.Position.AxisByInt(axisToSplit)))
                {
                    primsFst.Add(primitive);
                    continue;
                }

                primsSnd.Add(primitive);
            }

            var boxFst = new AxisAlignedBoundingBox(primsFst.Select(i => i.GetBoundingBox()).ToArray());
            var boxSnd = new AxisAlignedBoundingBox(primsSnd.Select(i => i.GetBoundingBox()).ToArray());

            var left = new BvhNode { IsLeaf = false, Primitives = primsFst.ToArray(), BoundingBox = boxFst };

            var right = new BvhNode { IsLeaf = false, Primitives = primsSnd.ToArray(), BoundingBox = boxSnd };

            parent.Left = left;
            parent.Right = right;

            stack.Push(left);
            stack.Push(right);
        }

        return root;
    }
}
