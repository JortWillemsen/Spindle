using Engine.Geometry;
using Engine.Lighting;
using Engine.Scenes;

namespace Engine.Strategies.BVH;

public class SplitDirectionStrategy : IBVHStrategy
{
    public IBoundingBox Build(Scene scene)
    {
        // Create a bounding box around each primitive.
        // TODO: Add light sources
        var primBoxes = scene.Objects.Select(o => o.BuildBoundingBox());
        
        // Create the root bounding box.
        var root = new AxisAlignedBoundingBox(scene.Objects.ToArray(), primBoxes.ToArray());

        return PopulateChildren(root);
    }

    private AxisAlignedBoundingBox PopulateChildren(AxisAlignedBoundingBox box)
    {
        // Base case, we return if the bounding box is around 1 primitive
        if (box.Primitives.Length <= 1)
        {
            box.IsLeaf = true;
            return box;
        }
        
        int axisToSplit = 0;

        // Determine which axis to split i.e. the longest axis.
        for (int axis = 0; axis < 3; axis++)
        {
            // If the size of the current axis is longer than the selected axis, we substitute it.
            if (box.AxisByInt(axis).Size > box.AxisByInt(axisToSplit).Size)
            {
                axisToSplit = axis;
            }
        }
        
        // Find the split intervals
        var (fst, snd) = box.AxisByInt(axisToSplit).Split();
        
        var primsFst = new List<Geometry.Geometry>();
        var primsSnd = new List<Geometry.Geometry>();

        // Divide the primitives based on their position in 3D space.
        foreach (var primitive in box.Primitives)
        {
            // Find out if intersectable is geometry or light.
            // Check if it falls inside the first or second box and append accordingly.
            if (fst.Surrounds(primitive.Position.AxisByInt(axisToSplit)))
            {
                primsFst.Add(primitive);
                continue;
            }
                        
            primsSnd.Add(primitive);
        }

        var boxFst = new AxisAlignedBoundingBox(
            primsFst.ToArray(), 
            primsFst.Select(o => o.BuildBoundingBox()).ToArray());
        var boxSnd = new AxisAlignedBoundingBox(
            primsSnd.ToArray(), 
            primsSnd.Select(o => o.BuildBoundingBox()).ToArray());

        // Recursive step
        box.LeftChild = PopulateChildren(boxFst);
        box.RightChild = PopulateChildren(boxSnd);

        return box;
    }
}
