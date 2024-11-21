using System.Numerics;
using SilkSonic;

namespace Engine.Cameras;

public abstract class Camera
{
    public float FocalLength { get; private set; } 
    public float AspectRatio { get; private set; }
    public int ImageWidth { get; private set; }
    public int ImageHeight { get; private set; }
    public int MaxDepth { get; private set; }
    public int Samples { get; private set; }
    public float ViewportWidth { get; private set; }
    public float ViewportHeight { get; private set; }
    
    public Vector3 Position { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Front { get; private set; }
    
    public Vector3 Right { get; private set; }
    public Vector3 U { get; private set; }
    public Vector3 V { get; private set; }
    
    protected Camera(float aspectRatio, float focalLength, int imageWidth, int imageHeight, int maxDepth, int samples, Vector3 position, Vector3 up, Vector3 front)
    {
        AspectRatio = aspectRatio;
        FocalLength = focalLength;
        ImageWidth = imageWidth;
        ImageHeight = imageHeight;
        MaxDepth = maxDepth;
        Samples = samples;
        Position = position;
        Up = up;
        Front = front;

        Right = Vector3.Cross(up, front).Normalized();
        
        ViewportWidth = 2f * ((float) ImageWidth /  ImageHeight);
        
        // Maybe in constructor
        ViewportHeight = 2f;
        
        U = new Vector3(ViewportWidth, 0f, 0f);
        V = new Vector3(0f, -ViewportHeight, 0f);
    }
    
    // TODO: Merge into one function to calculate the position of a pixel on the viewport with U, V;
    public Vector3 PixelDeltaU => U / ImageWidth;
    public Vector3 PixelDeltaV => V / ImageHeight;
    
    public Vector3 UpperLeft => Position - new Vector3(0f, 0f, FocalLength) - U / 2 - V / 2;

    public Vector3 Pixel0 => UpperLeft + .5f * (PixelDeltaU + PixelDeltaV);

    // TODO: Fix method
    public int[] RenderShot(IRenderer renderer)
    {
        var numOfLines = (this.ImageWidth * this.ImageHeight) + 3;
        var lines = new string[numOfLines];
        
        var pixels = new int[this.ImageWidth * this.ImageHeight];
        
        for (var j = 0; j < this.ImageHeight; j++)
        {
            var curCursorLine = Console.CursorTop;
            
            for (var i = 0; i < this.ImageWidth; i++)
            {
                var pixelColor = Color.Zero();

                for (int sample = 0; sample < cam.Samples; sample++)
                {
                    var r = cam.GetRay(i, j);
                    pixelColor += cam.RayColor(r, cam.MaxDepth, scene);
                }
                
                lines[j * cam.ImageWidth + 3 + i] = WriteColor(1f / cam.Samples * pixelColor);
            }
        }

        return lines;
    } 
    
    public Ray GetRay(int i, int j)
    {
        var offset = _sampleSquare();

        var pixelSample = Pixel0 
                          + (i + offset.X()) * PixelDeltaU
                          + (j + offset.Y()) * PixelDeltaV;

        return new Ray(Position, pixelSample - Position);
    }
    
    public Vector3 RayColor(Ray r, int depth, Scene scene)
    {
        if (depth <= 0)
            return Vector3.Zero();
        
        var hit = scene.Hit(r, new Interval(0.001f, Utils.Infinity));
        
        if (hit is SuccessRecord success)
        {
            var scatter = success.Geometry.Mat.Scatter(r, success);
            
            if (scatter is DoScatter doScatter)
                return scatter.Albedo * RayColor(doScatter.Outgoing, depth - 1, scene);

            return Vector3.Zero();
        }
        
        var unitDir = Vector3.Unit(r.Direction);

        var a = .5f * (unitDir.Y() + 1f);
        return (1f - a) * new Vector3(1f, 1f, 1f) + a * new Vector3(.5f, .7f, 1f );
    }
    
    private static Vector3 _sampleSquare()
    {
        var r = Utils.GetRandom();
        return new Vector3(Utils.RandomFloat(r) - .5f, Utils.RandomFloat(r) - .5f, 0f);
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