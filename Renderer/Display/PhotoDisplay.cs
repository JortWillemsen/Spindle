using SilkSonic.Cameras;
using SilkSonic.Renderer;

namespace SilkSonic.Display;

public class PhotoDisplay: IDisplay
{
	public PhotoDisplay(IRenderer renderer, CameraManager cameraManager, string filename) // todo: path?
	{
		Renderer = renderer;
		CameraManager = cameraManager;
	}

	/// <inheritdoc />
	public IRenderer Renderer { get; set; }

	/// <inheritdoc />
	public CameraManager  CameraManager  { get; set; }

	/// <inheritdoc />
	public void Show()
	{
		throw new NotImplementedException();
	}
}