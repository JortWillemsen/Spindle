using System.Numerics;

namespace Engine;

public static class ColorInt // todo: make extension methods out of these
{
	public static int Make(byte r, byte g, byte b) => (b << 16) | (g << 8) | r; // todo: Reversed, because somehow that is required
	public static int Make(Vector3 vec) {
		return Make((byte)(Math.Min(vec.X, 1) * 255), // todo: assume (safely...) that the values won't clip beyond 1
					(byte)(Math.Min(vec.Y, 1) * 255),
					(byte)(Math.Min(vec.Z, 1) * 255));
	}

	public static Vector3 GetVector(int c) => new(GetR(c), GetB(c), GetB(c));
	public static (byte r, byte g, byte b)  SplitRGB(int c)  => (GetR(c), GetG(c), GetB(c));
	public static byte    GetR(int color)  => (byte)(color >> 16);
	public static byte    GetG(int color)  => (byte)(color >> 8 );
	public static byte    GetB(int color)  => (byte)(color      );
}