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

    private const float _frustumHeight = 1f;

    protected event Action OnTransform;

    // ReSharper disable once InconsistentNaming
    protected Camera(Vector3 position, Vector3 up, Vector3 front, Size imageSize, float FOV, int maxDepth)
    {
        Position = position;
        Up = up.Normalized();
        Front = front.Normalized();
        ImageSize = imageSize;
        MaxDepth = maxDepth;

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
                (float)x / ImageSize.Width, (float)y / ImageSize.Height)); // TODO: bruh we never normalised this. Also: we should do width -1 and size -1 (or 100% case is never reached)
    }


    public abstract void RenderShot(IRenderer renderer, in Span<int> pixels);

    public virtual void SetImageSize(Size size) => ImageSize = size;

    /// <summary>
    /// Calculates the offset for Anti-aliasing
    /// </summary>
    /// <returns>The offset for the ray origin and destination</returns>
    public Vector3 _sampleSquare()
    {
        return new Vector3(
            (Utils.RandomFloat() - .5f) * (AspectRatio / ImageSize.Width), // FrustumHorizontal.Length() == AspectRatio
            (Utils.RandomFloat() - .5f) * (_frustumHeight / ImageSize.Height),
            0f);
    }

    #region Controls

    public void MoveForward(float amount)
    {
        Position += Front * amount;
        OnTransform.Invoke();
    }

    public void MoveHorizontally(float amount)
    {
        Position += Right * amount;
        OnTransform.Invoke();
    }

    public void MoveVertically(float amount)
    {
        Position += Up * amount;
        OnTransform.Invoke();
    }

    private float _currentHorizontalRotation;
    public void RotateHorizontally(float degree)
    {
        _currentHorizontalRotation += degree;
        (float sin, float cos) = MathF.SinCos(_currentHorizontalRotation * MathF.PI / 180f);
        Front = new Vector3(sin, Front.Y, cos).Normalized();
        Right = Vector3.Cross(Up, Front).Normalized();
        OnTransform.Invoke();
    }
    private float _currentVerticalRotation;
    public void RotateVertically(float degree)
    {
        _currentVerticalRotation -= degree; // TODO: needs to be inverted somehow
        (float sin, float cos) = MathF.SinCos(_currentVerticalRotation * MathF.PI / 180f);
        Up    = new Vector3(Up.X, cos, sin);
        Front = Vector3.Cross(Right, Up).Normalized();
        OnTransform.Invoke();
    }

    public void SetFocalLength(float focalLength)
    {
        FocalLength = focalLength;
        OnTransform.Invoke();
    }

    public void IncreaseFocalLength(float amount)
    {
        FocalLength += amount;
        OnTransform.Invoke();
    }

    public void SetZoom(float zoom) => SetFocalLength(zoom);

    public void Zoom(float scale)
    {
        FocalLength *= scale;
        OnTransform.Invoke();
    }

    #endregion Controls

    /// <summary>
    /// Given an average, computes the new average with <paramref name="sample"/> included.
    /// </summary>
    /// <param name="currentAverage">The average to expand.</param>
    /// <param name="numberOfSamples">The number of samples included in the average (excluding the given sample).</param>
    /// <param name="sample">The value to include in the average.</param>
    /// <returns>The average but now based on more samples.</returns>
    protected static float ExpandAverage(float currentAverage, uint numberOfSamples, float sample) =>
        currentAverage * ((float)numberOfSamples / (numberOfSamples + 1)) + sample / (numberOfSamples + 1);

    /// <summary>
    /// Given an average, computes the new average with <paramref name="sample"/> included.
    /// </summary>
    /// <param name="currentAverage">The average to expand.</param>
    /// <param name="numberOfSamples">The number of samples included in the average (excluding the given sample).</param>
    /// <param name="sample">The value to include in the average.</param>
    /// <returns>The average but now based on more samples.</returns>
    protected static Vector3 ExpandAverage(Vector3 currentAverage, uint numberOfSamples, Vector3 sample) =>
        currentAverage * ((float)numberOfSamples / (numberOfSamples + 1)) + sample / (numberOfSamples + 1);
}
