using Engine.AccelerationStructures.BoundingVolumeHierarchy;
using Engine.BoundingBoxes;
using Engine.Geometry;
using Engine.Scenes;
using System.Numerics;

namespace Engine.Strategies.BVH;

// ReSharper disable once InconsistentNaming
public class KDTreeStrategy : IBvhStrategy
{
    /// <summary>
    /// If a node has this number of primitives or less, it is considered a leaf.
    /// </summary>
    public int MaximumPrimitivesPerLeaf { get; set; }

    public KDTreeStrategy(int maximumPrimitivesPerLeaf)
    {
        MaximumPrimitivesPerLeaf = maximumPrimitivesPerLeaf;
    }

    /// <inheritdoc />
    public BvhNode Build(Scene scene)
    {
        BvhNode root = new BvhNode
        {
            BoundingBox = scene.GetBoundingBox(),
            IsLeaf = false,
            Primitives = new List<IIntersectable>(scene.Objects)
        };

        Stack<BvhNode> stack = new();
        stack.Push(root);

        // Build tree of BVH nodes
        while (stack.Count > 0)
        {
            BvhNode current = stack.Pop();
            if (current.IsLeaf)
                continue;

            (BvhNode left, BvhNode right) = SplitNode(current);
            current.Left = left;
            current.Right = right;
            stack.Push(left);
            stack.Push(right);
        }

        return root;
    }

    /// <summary>
    /// Splits the given node into two new nodes, and divides all primitives amongst them.
    /// </summary>
    /// <param name="node">The node to split.</param>
    /// <returns>Two nodes.</returns>
    /// <remarks>Does not set the children of <paramref name="node"/>.</remarks>
    private (BvhNode left, BvhNode right) SplitNode(BvhNode node)
    {
        // Store temporary bounds, to be updates after splitting
        Vector3 leftLowerBounds = node.GetBoundingBox().GetLowerBound();
        Vector3 rightLowerBounds = leftLowerBounds;
        Vector3 leftUpperBounds = node.GetBoundingBox().GetUpperBound();
        Vector3 rightUpperBounds = leftUpperBounds;

        (Axis splitAxis, float lowerBoundOffset) = FindSplitPlane(node);

        // Depending on axis, set the value of the left upper bound and right lower bound to
        // end and start at the split point, respectively.
        // The result is two adjacent BVH nodes, split along the correct axis.
        float splitPoint;
        switch (splitAxis)
        {
            case Axis.X:
                splitPoint = leftLowerBounds.X + lowerBoundOffset;
                leftUpperBounds.X = splitPoint;
                rightLowerBounds.X = splitPoint;
                break;
            case Axis.Y:
                splitPoint = leftLowerBounds.Y + lowerBoundOffset;
                leftUpperBounds.Y = splitPoint;
                rightLowerBounds.Y = splitPoint;
                break;
            case Axis.Z:
                splitPoint = leftLowerBounds.Z + lowerBoundOffset;
                leftUpperBounds.Z = splitPoint;
                rightLowerBounds.Z = splitPoint;
                break;
            default:
                throw new ArgumentException("Somehow you specified an axis we did not implement yet.");
        }

        // Determine which primitives are on which side of the split
        int numberOfChildren = node.Primitives.Count; // TODO: optimise memory usage here by reusing memory
        List<IIntersectable> leftPrimitives = new(numberOfChildren / 2);
        List<IIntersectable> rightPrimitives = new(numberOfChildren / 2);

        foreach (IIntersectable primitive in node.Primitives)
        {
            bool primitiveIsLeftOfSplit = primitive.GetCentroid()
                .AxisByInt((int)splitAxis) < splitPoint;

            if (primitiveIsLeftOfSplit)
                leftPrimitives.Add(primitive);
            else
                rightPrimitives.Add(primitive); // Objects intersecting the split point are added to the right as well
        }

        // Now that we know everything, we create the new nodes
        BvhNode left = new()
        {
            BoundingBox = new AxisAlignedBoundingBox(leftLowerBounds, leftUpperBounds),
            Primitives = leftPrimitives
        };
        BvhNode right = new()
        {
            BoundingBox = new AxisAlignedBoundingBox(rightLowerBounds, rightUpperBounds),
            Primitives = rightPrimitives
        };
        left.IsLeaf = ShouldBeLeaf(left);
        right.IsLeaf = ShouldBeLeaf(right);

        return (left, right);
    }

    /// <summary>
    /// Whether <paramref name="node"/> should be a leaf.
    /// Indicates whether recursion might stop.
    /// </summary>
    /// <returns>Whether the given node is a leaf.</returns>
    private bool ShouldBeLeaf(BvhNode node) => node.Primitives.Count <= MaximumPrimitivesPerLeaf; // TODO: move all these defining helpers methods to the interface?

    /// <summary>
    /// Determines along what axis, where to split.
    /// It does so by finding the best split along all axes, where a split is better if
    /// it gets a lower cost according to <see cref="DetermineSplitCost"/>.
    /// </summary>
    /// <param name="nodeToSplit">The node which is to be split in two.</param>
    /// <returns>The axis along which to split, and where to split along the axis offset from the lower bound.</returns>
    /// <remarks>Assumes that <paramref name="nodeToSplit"/> contains at least one primitive.</remarks>
    private static (Axis axis, float lowerBoundOffset) FindSplitPlane(BvhNode nodeToSplit)
    {
        // Prepare results to saturate
        Axis splitAxis = Axis.X;
        float lowerBoundOffset = 0;
        float cost = float.PositiveInfinity;
        // TODO: what if all primitives are on exactly the same place and rotation?

        // Try to split along each axis most efficiently,
        // and store result if more efficient than known result
        foreach (Axis testAxis in new[] { Axis.X, Axis.Y, Axis.Z })
        {
            float nodeLowerBound = nodeToSplit.GetBoundingBox().GetLowerBound().AxisByInt((int)testAxis);

            // Try to split at after each primitive TODO: might be optimised
            foreach (IIntersectable primitive in nodeToSplit.Primitives)
            {
                float primitiveCentroidPointOnAxis = primitive.GetCentroid().AxisByInt((int)testAxis);
                float testOffset = primitiveCentroidPointOnAxis - nodeLowerBound + 1E-5f; // Determine offset of primitive from node lower bounds
                float testCost = DetermineSplitCost(testAxis, nodeToSplit, testOffset); // Test the split
                if (testCost >= cost) continue; // Not a better solution

                cost = testCost;
                lowerBoundOffset = testOffset;
                splitAxis = testAxis;
            }
        }

        return (splitAxis, lowerBoundOffset);
    }

    // TODO: do something smarter such that optimal balance between surface area and number of primitives is found.
    /// <summary>
    /// A lower cost means that all child primitives are better divided along the left and
    /// right side of the split, and that the split is closest to the center of the axis along which to split.
    /// </summary>
    /// <param name="splitAxis">The axis along which is split.</param>
    /// <param name="node">The node which is split.</param>
    /// <param name="offset">Distance to split at, offset from most negative node coordinate.</param>
    /// <returns>The cost of the split.</returns>
    private static float DetermineSplitCost(Axis splitAxis, BvhNode node, float offset)
    {
        Interval axisInterval = node.BoundingBox.AxisByInt((int)splitAxis);
        float axisCentre = axisInterval.Middle;
        float splitPoint = axisInterval.Min + offset;

        // Determine absolute distance to axis centre
        float distanceToCentre = MathF.Abs(splitPoint - axisCentre);

        // Determine the amount of imbalance between primitives left and right of the split
        float numberOfPrimitiveCentroidsLeftOfSplit = node.Primitives.Count(
            x => x.GetCentroid().AxisByInt((int)splitAxis) < splitPoint);
        float primitiveImbalance = MathF.Abs(numberOfPrimitiveCentroidsLeftOfSplit - (float)node.Primitives.Count / 2);

        // Determine cost
        return distanceToCentre * primitiveImbalance;
    }
}

enum Axis
{
    X = 0, Y = 1, Z = 2 // Numbered for AxisByInt() TODO: create other extensionmethod
}
