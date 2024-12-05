using System.Drawing;
using Engine.Renderers;

namespace Renderer.Display;

public interface IDisplay
{
    public IRenderer     Renderer      { get; set; }
    public CameraManager CameraManager { get; set; }
    public Size          DisplaySize   { get; }

    public void Show(params string[] args);
}
