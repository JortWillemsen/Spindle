using System.Numerics;
namespace Engine;

public class Ray
{
    public Vector3 Origin { get; private set; }
    public Vector3 Direction { get; private set; }
    
    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = direction;
    }

    public Vector3 At(float t)
    {
        return Origin + t * Direction;
    }
}