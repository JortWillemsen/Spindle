using Engine.Scenes;
using System.Numerics;

namespace Engine;

internal static class ExtensionMethods
{
    public static Vector3 Normalized(this Vector3 vector) => Vector3.Normalize(vector);

    public static bool NearZero(this Vector3 vector)
    {
        const float s = 1e-8f;
        return Math.Abs(vector.X) < s && Math.Abs(vector.Y) < s && Math.Abs(vector.Z) < s;
    }

    public static float AxisByInt(this Vector3 vector, int axis)
    {
        return axis switch
        {
            0 => vector.X,
            1 => vector.Y,
            2 => vector.Z,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "No such axis")
        };
    }
}
