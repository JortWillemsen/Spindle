using Engine.Scenes;

namespace Engine.Strategies.BVH;

public interface IBvhStrategy
{
    public IBoundingBox Build(Scene scene);
}
