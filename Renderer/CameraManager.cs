/*using System.Numerics;
using Engine.Cameras;
using Silk.NET.Maths;

namespace Renderer;

public class CameraManager
{
	// TODO: laat alle cameras één array aan ints delen voor garbage collection optimization, via slices
	public CameraManager(int displayWidth, int displayHeight, CameraLayout initialCameraLayout)
	{
		DisplayWidth = displayWidth;
		DisplayHeight = displayHeight;
		CameraLayout = initialCameraLayout;
		Cameras = new List<Camera>();
	}
	
	public int DisplayWidth  { get; private set; }
	public int DisplayHeight { get; private set; }
	public CameraLayout CameraLayout  { get; protected set; }
	public List<Camera> Cameras { get; protected set; }

	public void AddBasicCamera(Vector3 position)
	{
		Cameras.Add(new BasicCamera(
			position,
			DisplayWidth,
			DisplayHeight));
		// todo: set values based on layout
	}

	public void AddDefaultCameras(int amount)
	{
		throw new NotImplementedException(); // TODO
		// for (int i = 0; i < amount; i++)
		// 	Cameras.Add(new BasicCamera());
	}

	public IEnumerable<Camera> GetDisplayedCameras() // TODO: for now, we only show the first camera
	{
		yield return CameraLayout switch
		{
			CameraLayout.Single => Cameras.First(),
			CameraLayout.Matrix => throw new NotImplementedException(),
			_                   => throw new NotImplementedException()
		};
	}

	public void SetLayout(CameraLayout layout)
	{
		throw new NotImplementedException();
		// todo: set camera viewports to match layout
		// todo: set camera texture screenoffsets
	}

	public void FocusOnCamera(int cameraIndex)
	{
		throw new NotImplementedException();
		// TODO: set camera viewports to match layout
		// TODO: set camera texture screenoffsets
	}

	public void SetDisplaySize(Vector2D<int> displaySize)
	{
		DisplayWidth = displaySize.X;
		DisplayHeight = displaySize.Y;
		
		// TODO: set camera texture screenoffsets
		foreach (var camera in Cameras)
		{
			camera.SetViewport(displaySize.X, displaySize.Y);
		}
	}
}

public enum CameraLayout
{
	Single,
	Matrix
}*/