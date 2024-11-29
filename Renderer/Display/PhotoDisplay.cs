using System.Drawing;
using System.Numerics;
using Engine;
using Engine.Cameras;
using Engine.Renderers;

namespace Renderer.Display;

public class PhotoDisplay : IDisplay
{
    /// <inheritdoc />
    public IRenderer     Renderer      { get; set; }

    /// <inheritdoc />
    public CameraManager CameraManager { get; set; }

    /// <inheritdoc />
    public Size DisplaySize => CameraManager.DisplaySize;

    public PhotoDisplay(IRenderer renderer, CameraManager cameraManager)
    {
        Renderer = renderer;
        CameraManager = cameraManager;
    }

    /// <inheritdoc />
    public void Show(params string[] args)
    {
        if (args.Length == 0)
            throw new ArgumentException("No filepath specified");

        RenderToFile("/output/" + args[0]);
    }

    private string[] RenderLines()
    {
        Console.WriteLine("Image build started");
        Console.WriteLine("");

        var cameraSlot = CameraManager.GetDisplayedCameraSlots().First();
        cameraSlot.Camera.RenderShot(Renderer, cameraSlot.Texture.Pixels);
        
        var numOfLines = (CameraManager.DisplaySize.Width * CameraManager.DisplaySize.Height) + 3;
        var lines = new string[numOfLines];
        
        // PPM header information
        lines[0] = "P3";
        lines[1] = CameraManager.DisplaySize.Width + " " + CameraManager.DisplaySize.Height;
        lines[2] = "255";
        
        for (var j = 0; j < CameraManager.DisplaySize.Height; j++)
        {
            // Clearing previous progress line
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, Console.CursorTop);
            
            // Writing new progress line
            Console.Write("Lines remaining: " + (CameraManager.DisplaySize.Height - j) + " of " + CameraManager.DisplaySize.Height);
            
            for (var i = 0; i < CameraManager.DisplaySize.Width; i++)
            {
                lines[j * CameraManager.DisplaySize.Width + 3 + i] = WriteColor(cameraSlot.Texture.Pixels[j * CameraManager.DisplaySize.Width + i]);
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