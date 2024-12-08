using Engine.Geometry;

namespace Engine.Scenes;

public interface IBoundingBox : IIntersectable
{
    public IBoundingBox Combine(IBoundingBox[] boxes);
    public Interval AxisByInt(int axis);
}
