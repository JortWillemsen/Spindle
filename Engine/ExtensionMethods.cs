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
}