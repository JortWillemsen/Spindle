using System.Numerics;
using SilkSonic.Lighting;
using SilkSonic.Physics;
using SilkSonic.Primitives;
using Plane = SilkSonic.Primitives.Plane;

namespace SilkSonic;

public class Scene
{
	private List<Primitive> Primitives = new();
	private List<LightSource> LightSources = new();

	public void Clear() => Primitives.Clear();

	public void AddPrimitive(Primitive primitive) => Primitives.Add(primitive);

	public void AddSphere(Vector3 position, Material material, float radius)
		=> AddPrimitive(new Sphere(position, material, radius));

	public void AddPlane(Vector3 position, Material material, Vector3 normal)
		=> AddPrimitive(new Plane(position, material, normal));

	public void AddQuad(Vector3 position, Material material, Vector3 normal, Vector3 min, Vector3 max)
		=> AddPrimitive(new Quad(position, material, normal, min, max));

	public void AddLightSource(LightSource lightSource) => LightSources.Add(lightSource);
}