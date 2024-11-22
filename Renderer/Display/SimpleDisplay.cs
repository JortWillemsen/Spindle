using System.Drawing;
using System.Numerics;
using Engine;
using Engine.Cameras;
using Engine.Geometry;
using Engine.Renderers;

namespace Renderer.Display;

public class SimpleDisplay
{
    public IRenderer Renderer { get; set; }
    public Camera Camera { get; set; }

    public SimpleDisplay(IRenderer renderer, Camera camera)
    {
        Renderer = renderer;
        Camera = camera;
    }

    private string[] RenderLines()
    {
        Console.WriteLine("Image build started");
        Console.WriteLine("");

        var pixels = Camera.RenderShot(Renderer);
        
        var numOfLines = (Camera.ImageWidth * Camera.ImageHeight) + 3;
        var lines = new string[numOfLines];
        
        // PPM header information
        lines[0] = "P3";
        lines[1] = Camera.ImageWidth + " " + Camera.ImageHeight;
        lines[2] = "255";
        
        for (var j = 0; j < Camera.ImageHeight; j++)
        {
            var curCursorLine = Console.CursorTop;
            
            // Clearing previous progress line
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, Console.CursorTop);
            
            // Writing new progress line
            Console.Write("Lines remaining: " + (Camera.ImageHeight - j) + " of " + Camera.ImageHeight);
            
            for (var i = 0; i < Camera.ImageWidth; i++)
            {
                lines[j * Camera.ImageWidth + 3 + i] = WriteColor(Utils.IntToRgb(pixels[j * Camera.ImageWidth + i]));
            }
        }

        return lines;
    }

    public void RenderToFile(string fileName)
    {
        var path = Directory.GetCurrentDirectory() + fileName;
        
        var lines = RenderLines();
        using var outputFile = new StreamWriter(path);
        foreach (var line in lines)
        {
            outputFile.WriteLine(line);
        }
        
        Console.WriteLine();
        Console.WriteLine("Image output finished at: \n");
        Console.WriteLine(path);
    }

    public static string WriteColor(Vector3 pixel)
    {
        var r = pixel.X;
        var g = pixel.Y;
        var b = pixel.Z;

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