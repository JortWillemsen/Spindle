using Engine.Scenes;

namespace Engine.Strategies.BVH;

public interface IBVHStrategy
{
    public IBoundingBox Build(Scene scene);
}
