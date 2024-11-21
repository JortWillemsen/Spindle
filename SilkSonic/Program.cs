using System.Numerics;
using SilkSonic.Cameras;
using SilkSonic.Physics;
using SilkSonic.Renderer;
using SilkSonic.Display;
using SilkSonic.Lighting;

namespace SilkSonic;

public class Program
{
	const int DisplayWidth = 690, DisplayHeight = 512;
		
	// All these configurations can later also be done through the commandline
	public static void Main(string[] args)
	{
		// Vector3 color = new(20, 60, 180);
		// Material material = new(color, Vector3.One, Vector3.One);

		Scene scene = new();

		// todo: these colors are normalised to [0..1]
		Material blue     = new(new Vector3(0, 0, 1),           Vector3.One / 2, Vector3.One / 2);
		Material greenish = new(new Vector3(80, 189, 58) / 255, Vector3.One / 2, Vector3.One / 2);
		Material purple   = new(new Vector3(1, 0, 1),           Vector3.One / 2, Vector3.One / 2);
		Material mirror   = new(new Vector3(1, 1, 1),           Vector3.One / 2, Vector3.One, MaterialType.Mirror);

		scene.AddSphere(new Vector3(3,  2,    0), blue, 1.3f);
		scene.AddSphere(new Vector3(0,  0.5f, 0), mirror, 1f);
		scene.AddSphere(new Vector3(-2, 1f,   0), purple, 1f);
		scene.AddSphere(new Vector3(-1,  3f,  3), greenish, 2f);

		scene.AddPlane(new Vector3(0, -1f, 0), greenish, new Vector3(0, -1, 0));

		scene.AddLightSource(new Spotlight(new Vector3(-3,  3, 0f), new Vector3(1, 1, 1)*3));

		// scene.AddPlane(-2 * Vector3.UnitY, material, Vector3.UnitY);
		// scene.AddSphere(Vector3.UnitY, material, 2.5f);

		CameraManager cameraManager = new(DisplayWidth, DisplayHeight, CameraLayout.Single);
		cameraManager.AddBasicCamera(new Vector3(0, 0, -5), Vector3.UnitZ, Vector3.UnitY, new Rectangle(0, 0, DisplaySize.Width, DisplaySize.Height), 60);

		IRenderer renderer = new Pathtracer(scene);

		IDisplay display = args.Length > 0
			? new PhotoDisplay(renderer, cameraManager, args[0]) // Console is used
			: new OpenGLDisplay(renderer, cameraManager, DisplayWidth, DisplayHeight);

		display.Show();
	}
}