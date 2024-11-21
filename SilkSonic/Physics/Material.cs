using System.Numerics;

namespace SilkSonic.Physics;

public record struct Material(
	Vector3 DiffuseColor, // todo: System.Drawing.Color? Of ColorInt?
	Vector3 SpecularColor,
	Vector3 AmbientColor,
	MaterialType MaterialType = MaterialType.Normal,
	float N = 0)
{ }

public enum MaterialType
{
	Normal, // todo: diffuse?
	Mirror
}