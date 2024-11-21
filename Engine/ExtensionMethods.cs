using System.Numerics;

namespace SilkSonic;

internal static class ExtensionMethods
{
	public static Vector3 Normalized(this Vector3 vector) => Vector3.Normalize(vector);
}