using System.Drawing;
using Engine.Geometry;
using Engine.Geometry.Interfaces;
using Engine.Materials;

namespace Engine;

public class Camera
{
    public float FocalLength { get; private set; } 
    public float AspectRatio { get; private set; }
    public int ImageWidth { get; private set; }
    public int ImageHeight { get; private set; }
    public int MaxDepth { get; private set; }
    
    public int Samples { get; private set; }
    
    public float ViewportWidth { get; private set; }
    public float ViewportHeight { get; private set; }
    
    public Vector3 Origin { get; private set; }
    public Vector3 U { get; private set; }
    public Vector3 V { get; private set; }
    
    public Camera(float aspectRatio, float focalLength, int imageWidth, int imageHeight, int maxDepth, int samples, Vector3 origin)
    {
        AspectRatio = aspectRatio;
        FocalLength = focalLength;
        ImageWidth = imageWidth;
        ImageHeight = imageHeight;
        MaxDepth = maxDepth;
        Samples = samples;
        Origin = origin;
        
        ViewportWidth = 2f * ((float) ImageWidth /  ImageHeight);
        ViewportHeight = 2f;
        
        U = new Vector3(ViewportWidth, 0f, 0f);
        V = new Vector3(0f, -ViewportHeight, 0f);
    }
    
    public Vector3 PixelDeltaU => U / ImageWidth;
    public Vector3 PixelDeltaV => V / ImageHeight;
    
    public Vector3 UpperLeft => Origin - new Vector3(0f, 0f, FocalLength) - U / 2 - V / 2;

    public Vector3 Pixel0 => UpperLeft + .5f * (PixelDeltaU + PixelDeltaV);
    
    public Ray GetRay(int i, int j)
    {
        var offset = _sampleSquare();

        var pixelSample = Pixel0 
                          + (i + offset.X()) * PixelDeltaU
                          + (j + offset.Y()) * PixelDeltaV;

        return new Ray(Origin, pixelSample - Origin);
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
}