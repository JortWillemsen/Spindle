using SilkSonic.Cameras;
using SilkSonic.Renderer;

namespace SilkSonic.Display;

public interface IDisplay
{
	public IRenderer     Renderer      { get; set; }
	public CameraManager CameraManager { get; set; }

	public void Show();
}