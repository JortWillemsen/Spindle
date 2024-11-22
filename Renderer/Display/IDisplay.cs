using Engine.Renderers;

namespace Renderer.Display;

public interface IDisplay
{
	public IRenderer     Renderer      { get; set; }
	public CameraManager CameraManager { get; set; }

	public void Show();
}