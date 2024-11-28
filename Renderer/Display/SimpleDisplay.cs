using System.Numerics;
using Engine;
using Engine.Cameras;
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

        Span<int> pixels = stackalloc int[Camera.DisplayRegion.Width * Camera.DisplayRegion.Height];
        Camera.RenderShot(Renderer, pixels);
        
        var numOfLines = (Camera.DisplayRegion.Width * Camera.DisplayRegion.Height) + 3;
        var lines = new string[numOfLines];
        
        // PPM header information
        lines[0] = "P3";
        lines[1] = Camera.DisplayRegion.Width + " " + Camera.DisplayRegion.Height;
        lines[2] = "255";
        
        for (var j = 0; j < Camera.DisplayRegion.Height; j++)
        {
            // Clearing previous progress line
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, Console.CursorTop);
            
            // Writing new progress line
            Console.Write("Lines remaining: " + (Camera.DisplayRegion.Height - j) + " of " + Camera.DisplayRegion.Height);
            
            for (var i = 0; i < Camera.DisplayRegion.Width; i++)
            {
                lines[j * Camera.DisplayRegion.Width + 3 + i] = WriteColor(pixels[j * Camera.DisplayRegion.Width + i]);
            }
        }

        return lines;
    }

    public void RenderToFile(string fileName)
    {
        var path = Directory.GetCurrentDirectory() + fileName;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        
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

    public static string WriteColor(int pixel)
    {
        (byte r, int g, int b) = ColorInt.SplitRGB(pixel);
        return string.Join(" ", r, g, b);
    }

    public static float LinearToGamma(float x) // TODO: add this back later?
    {
        if (x > 0)
        {
            return (float) Math.Sqrt(x);
        }

        return 0;
    }
}