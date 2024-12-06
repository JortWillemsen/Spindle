using System.Drawing;
using Engine.Cameras;

namespace Renderer.Display;

/// <summary>
/// Represents a camera and his texture, a single unit to be displayed
/// </summary>
public class CameraSlot
{
    public Camera        Camera        { get; init; }
    public OpenGLTexture Texture       { get; init; } // TODO: not every display manager uses OpenGL
    public Rectangle     DisplayRegion { get; private set; }

    public CameraSlot(Camera camera, OpenGLTexture texture, Rectangle displayRegion)
    {
        Camera = camera;
        Texture = texture;
        DisplayRegion = displayRegion;
    }

    public void Update(Point point, Size cameraSlotSize)
    {
        Texture.SetSize(cameraSlotSize);
        Camera.SetImageSize(cameraSlotSize);
        DisplayRegion = new Rectangle(point, cameraSlotSize);
    }

    public void Deconstruct(out Camera camera, out OpenGLTexture texture, out Rectangle displayRegion)
    {
        camera = Camera;
        texture = Texture;
        displayRegion = DisplayRegion;
    }
}
