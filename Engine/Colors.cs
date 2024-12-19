using System.Numerics;

namespace Engine;

public static class ColorInt // todo: make extension methods out of these
{
    public static int Make(byte r, byte g, byte b) => (b << 16) | (g << 8) | r; // todo: Reversed, because somehow that is required for OpenGL
    public static int Make(Vector3 vec) {
        return Make((byte)(vec.X * 255),
                    (byte)(vec.Y * 255),
                    (byte)(vec.Z * 255));
    }

    public static Vector3 GetVector(int c) => new(GetR(c), GetB(c), GetB(c));
    public static (byte r, byte g, byte b)  SplitRgb(int c)  => (GetR(c), GetG(c), GetB(c));
    public static (byte r, byte g, byte b)  SplitRgb(Vector3 c)  => SplitRgb(Make(c));
    public static byte    GetR(int color)  => (byte)(color      );
    public static byte    GetG(int color)  => (byte)(color >> 8 );
    public static byte    GetB(int color)  => (byte)(color >> 16);
}
