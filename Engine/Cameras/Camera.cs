using System.Drawing;
using System.Numerics;
using Engine.Renderers;
using SilkSonic;

namespace Engine.Cameras;

public abstract class Camera
{
    public Vector3 Position { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Front { get; private set; }
    public Vector3 Right { get; private set; }
    
    public Rectangle DisplayRegion { get; private set; }
    public float FocalLength { get; private set; } 
    
    public float AspectRatio => (float)DisplayRegion.Width / DisplayRegion.Height;
    
    public int MaxDepth { get; private set; }
    public int Samples { get; private set; }

    private const float FrustumHeight = 1f;
    
    // ReSharper disable once InconsistentNaming
    protected Camera(Vector3 position, Vector3 up, Vector3 front, Rectangle displayRegion, float FOV, int maxDepth, int samples)
    {
        Position = position;
        Up = up.Normalized();
        Front = front.Normalized();
        DisplayRegion = displayRegion;
        MaxDepth = maxDepth;
        Samples = samples;
        
        Right = Vector3.Cross(up, front).Normalized();
        FocalLength = 45f / FOV;
    }
    
    public Vector3 FrustumTopLeft    => Front * FocalLength - FrustumHorizontal / 2 + FrustumVertical / 2;
    public Vector3 FrustumHorizontal => Right * FrustumHeight * AspectRatio;
    public Vector3 FrustumVertical   => Up * FrustumHeight;
    
    public Vector3 GetDirectionTowardsPixel(float xPercentage, float yPercentage)
        => FrustumTopLeft + FrustumHorizontal * xPercentage - FrustumVertical * yPercentage;
    public Ray GetRayTowardsPixel(int x, int y)
    {
        var antiAliasingOffset = _sampleSquare();
        return new Ray(
            antiAliasingOffset + Position,
            antiAliasingOffset + GetDirectionTowardsPixel((float)x / DisplayRegion.Width, (float)y / DisplayRegion.Height));
    }


    public abstract Vector3[] RenderShot(IRenderer renderer);

    public void SetDisplayRegion(Rectangle viewport) => DisplayRegion = viewport;

    /// <summary>
    /// Calculates the offset for Anti-aliasing
    /// </summary>
    /// <returns>The offset for the ray origin and destination</returns>
    private Vector3 _sampleSquare()
    {
        var r = Utils.GetRandom();
        return new Vector3(
            (Utils.RandomFloat(r) - .5f) * (AspectRatio / DisplayRegion.Width), // FrustumHorizontal.Length() == AspectRatio
            (Utils.RandomFloat(r) - .5f) * (FrustumHeight / DisplayRegion.Height),
            0f);
    }
    
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