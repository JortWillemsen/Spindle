using System.Numerics;
using Silk.NET.Maths;
using SilkSonic.Display;
using SilkSonic.Renderer;

namespace SilkSonic.Cameras;

public abstract class Camera
{
	// TODO: I don't think every type of camera should per say have all these things. Remove from this base class
	protected Camera(Vector3 position, Vector3 front, Vector3 up, float focalDistance, int viewportWidth, int viewportHeight, Rectangle<int> textureScreenRegion)
	{
		Position = position;
		Front = front;
		Up = up;
		FocalDistance = focalDistance;
		ViewportWidth = viewportWidth;
		ViewportHeight = viewportHeight;
		
		Right = Vector3.Cross(Up, Front).Normalized(); //TODO: is normalizing not obsolete here?
		Texture = new Texture(ViewportWidth, ViewportHeight, textureScreenRegion);
	}

	public Vector3 Position       { get; private set; }
	public Vector3 Front          { get; private set; }
	public Vector3 Up             { get; private set; }
	public Vector3 Right          { get; private set; }
	public float   FocalDistance  { get; private set; }
	public int     ViewportWidth  { get; private set; }
	public int     ViewportHeight { get; private set; }
	public Texture Texture        { get; private set; }

	public void SetViewport(int width, int height)
	{
		ViewportWidth = width;
		ViewportHeight = height;
	}

	public abstract void RenderOneShot(in IRenderer renderer);
	
	public void MoveForward(float amount)      => Position += Front * amount;
	public void MoveHorizontally(float amount) => Position += Right * amount;
	public void MoveVertically(float amount)   => Position += Up * amount;
	
	public void RotateHorizontally(float degree)
	{
		(float sin, float cos) = MathF.SinCos(degree * MathF.PI / 180f);
		Front = new Vector3(sin, Front.Y, cos).Normalized();
		Right = Vector3.Cross(Up, Front).Normalized();
	}
	public void RotateVertically(float degree) {
		(float sin, float cos) = MathF.SinCos(degree * MathF.PI / 180f);
		Up    = new Vector3(Up.X, cos, sin);
		Front = Vector3.Cross(Right, Up).Normalized();
	}
}