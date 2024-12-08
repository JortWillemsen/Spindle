using Engine.AccelerationStructures.BoundingVolumeHierarchy;
using Engine.Scenes;

namespace Engine.Strategies.BVH;

public interface IBvhStrategy
{
    public BvhNode Build(Scene scene);
}
