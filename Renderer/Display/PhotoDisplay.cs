using System.Drawing;
using Engine;
using Engine.Renderers;
using System.Diagnostics;

namespace Renderer.Display;

public class PhotoDisplay : IDisplay
{
    private Stopwatch _sw;
    /// <inheritdoc />
    public IRenderer     Renderer      { get; set; }

    /// <inheritdoc />
    public CameraManager CameraManager { get; set; }

    /// <inheritdoc />
    public Size DisplaySize => CameraManager.DisplaySize;

    public PhotoDisplay(IRenderer renderer, CameraManager cameraManager, Stopwatch sw)
    {
        _sw = sw;
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
        var since = _sw.ElapsedMilliseconds;
        cameraSlot.Camera.RenderShot(Renderer, cameraSlot.Texture.Pixels);
        var now = _sw.ElapsedMilliseconds - since;
        Console.WriteLine($"Done creating frame, {now}ms");
        
        var numOfLines = (CameraManager.DisplaySize.Width * CameraManager.DisplaySize.Height) + 3;
        var lines = new string[numOfLines];
        
        // PPM header information
        lines[0] = "P3";
        lines[1] = CameraManager.DisplaySize.Width + " " + CameraManager.DisplaySize.Height;
        lines[2] = "255";
        
        for (var j = 0; j < CameraManager.DisplaySize.Height; j++)
        {
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
        var since = _sw.ElapsedMilliseconds;
        using var outputFile = new StreamWriter(path);
        foreach (var line in lines)
        {
            outputFile.WriteLine(line);
        }
        var now = _sw.ElapsedMilliseconds - since;
        Console.WriteLine($"file creation finished, {now}ms");
        
        Console.WriteLine();
        Console.WriteLine("Image output finished at: \n");
        Console.WriteLine(path);
        Console.WriteLine();
        Console.WriteLine($"total time elapsed: {_sw.ElapsedMilliseconds}ms");
        _sw.Stop();
    }

    public static string WriteColor(int pixel)
    {
        (byte r, int g, int b) = ColorInt.SplitRgb(pixel);
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
