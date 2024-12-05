using System.Drawing;
using System.Numerics;
using Engine.Renderers;

namespace Engine.Cameras;

public abstract class Camera
{
    public Vector3 Position { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Front { get; private set; }
    public Vector3 Right { get; private set; }
    
    public Size ImageSize { get; private set; }
    public float FocalLength { get; private set; } 
    
    public float AspectRatio => (float)ImageSize.Width / ImageSize.Height;
    
    public int MaxDepth { get; private set; }
    public int Samples { get; private set; }

    private const float _frustumHeight = 1f;
    
    // ReSharper disable once InconsistentNaming
    protected Camera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth, int samples)
    {
        Position = position;
        Up = up.Normalized();
        Front = front.Normalized();
        ImageSize = imageSize;
        MaxDepth = maxDepth;
        Samples = samples;
        
        Right = Vector3.Cross(up, front).Normalized();
        FocalLength = 45f / FOV;
    }
    
    public Vector3 FrustumTopLeft    => Front * FocalLength - FrustumHorizontal / 2 + FrustumVertical / 2;
    public Vector3 FrustumHorizontal => Right * _frustumHeight * AspectRatio;
    public Vector3 FrustumVertical   => Up * _frustumHeight;
    
    public Vector3 GetDirectionTowardsPixel(float xPercentage, float yPercentage)
        => FrustumTopLeft + FrustumHorizontal * xPercentage - FrustumVertical * yPercentage;
    public Ray GetRayTowardsPixel(int x, int y)
    {
        var antiAliasingOffset = _sampleSquare();
        return new Ray(
            antiAliasingOffset + Position,
            antiAliasingOffset + GetDirectionTowardsPixel(
                (float)x / ImageSize.Width, (float)y / ImageSize.Height));
    }


    public abstract void RenderShot(IRenderer renderer, in Span<int> pixels);

    public void SetImageSize(Size size) => ImageSize = size;

    /// <summary>
    /// Calculates the offset for Anti-aliasing
    /// </summary>
    /// <returns>The offset for the ray origin and destination</returns>
    private Vector3 _sampleSquare()
    {
        return new Vector3(
            (Utils.RandomFloat() - .5f) * (AspectRatio / ImageSize.Width), // FrustumHorizontal.Length() == AspectRatio
            (Utils.RandomFloat() - .5f) * (_frustumHeight / ImageSize.Height),
            0f);
    }
    
    public void MoveForward(float amount)      => Position += Front * amount;
    public void MoveHorizontally(float amount) => Position += Right * amount;
    public void MoveVertically(float amount)   => Position += Up * amount;

    private float _currentHorizontalRotation;
    public void RotateHorizontally(float degree)
    {
        _currentHorizontalRotation += degree;
        (float sin, float cos) = MathF.SinCos(_currentHorizontalRotation * MathF.PI / 180f);
        Front = new Vector3(sin, Front.Y, cos).Normalized();
        Right = Vector3.Cross(Up, Front).Normalized();
    }
    private float _currentVerticalRotation;
    public void RotateVertically(float degree)
    {
        _currentVerticalRotation += degree; // TODO: needs to be inverted somehow
        (float sin, float cos) = MathF.SinCos(_currentVerticalRotation * MathF.PI / 180f);
        Up    = new Vector3(Up.X, cos, sin);
        Front = Vector3.Cross(Right, Up).Normalized();
    }

    public void SetFocalLength(float focalLength) => FocalLength = focalLength;
    public void IncreaseFocalLength(float amount) => FocalLength += amount;

    public void SetZoom(float zoom) => FocalLength = zoom;
    public void Zoom(float scale) => FocalLength *= scale;
}
