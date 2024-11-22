using Engine;
using Engine.Cameras;
using Engine.Geometry;
using Color = Engine.Vector3;

namespace Renderer;

public static class ImageRenderer
{
    private static string[] Render(Camera cam, Scene scene)
    {
        
    }

    public static void RenderToFile(Camera vp, Scene scene, string fileName)
    {
        var path = Directory.GetCurrentDirectory() + fileName;
        
        var lines = Render(vp, scene);
        using var outputFile = new StreamWriter(path);
        foreach (var line in lines)
        {
            outputFile.WriteLine(line);
        }
        
        Console.WriteLine();
        Console.WriteLine("Image output finished at: \n");
        Console.WriteLine(path);
    }

    public static string WriteColor(Color pixel)
    {
        var r = pixel.X();
        var g = pixel.Y();
        var b = pixel.Z();

        r = LinearToGamma(r);
        g = LinearToGamma(g);
        b = LinearToGamma(b);
        
        var intensity = new Interval(0f, 0.999f);
        
        int rByte = (int) (256 * intensity.Clamp(r));
        int gByte = (int) (256 * intensity.Clamp(g));
        int bByte = (int) (256 * intensity.Clamp(b));

        return string.Join(" ", rByte, gByte, bByte);
    }

    public static float LinearToGamma(float x)
    {
        if (x > 0)
        {
            return (float) Math.Sqrt(x);
        }

        return 0;
    }
}