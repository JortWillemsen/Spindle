using Engine.Cameras;
using Engine.Scenes;
using Silk.NET.OpenCL;
using System.Drawing;

namespace Gpu.Pipeline;

public class WavefrontPipeline
{
    public OpenCLManager Manager { get; private set; }
    public OpenCLCamera Camera { get; private set; }

    public nuint[]                GlobalSize         { get; set; }
    public nuint[]                LocalSize          { get; set; }
    public ClSceneBuffers         SceneBuffers       { get; private set; }
    public ReadWriteBuffer<uint>  RandomStatesBuffer { get; private set; }
    public GeneratePhase          GeneratePhase      { get; private set; }
    public ConnectPhase           ConnectPhase       { get; private set; }
    public ShadePhase             ShadePhase         { get; private set; }
    public ExtendPhase            ExtendPhase        { get; private set; }
    public ReadWriteBuffer<int>   ImageBuffer        { get; private set; }

    public WavefrontPipeline(
        Scene scene, 
        OpenCLCamera camera)
    {
        Manager = new OpenCLManager();
        Camera = camera;
        SceneBuffers = BufferConverter.ConvertSceneToBuffers(Manager, scene);
        RandomStatesBuffer = new ReadWriteBuffer<uint>(Manager, new uint[]
        {
            // TODO generate this with random seed in a dedicated method
            69, 420, 10, 20, 9000, 90001, 42069, 804930, 02380923809, 032093, 480239805, 8934820, 89023402, 4092094389,
            043289040, 03488035
        });

        GlobalSize = new nuint[2] { (nuint)camera.ImageSize.Width, (nuint)camera.ImageSize.Height };
        LocalSize = new nuint[2] { 1, 1 };
        
        // Add structs that will be bound with every program
        Manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        Manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
        Manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");

        // Add buffers to the manager
        Manager.AddBuffers(SceneBuffers.SceneInfo, SceneBuffers.Spheres, SceneBuffers.Triangles, RandomStatesBuffer);
        // Find number of rays, used for calculating buffer sizes
        var numOfRays = camera.ImageSize.Width * camera.ImageSize.Height;

        var colors = new int[numOfRays];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = i ;
        }
        
        ImageBuffer = new ReadWriteBuffer<int>(Manager, colors);
        
        GeneratePhase = new GeneratePhase(
            Manager, 
            "/../../../../Gpu/Programs/generate.cl", 
            "generate", 
            numOfRays);

        ExtendPhase = new ExtendPhase(
            Manager,
            "/../../../../Gpu/Programs/extend.cl",
            "extend",
            GeneratePhase.RayBuffer,
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles);

        ShadePhase = new ShadePhase(
            Manager,
            "/../../../../Gpu/Programs/shade.cl",
            "shade",
            RandomStatesBuffer,
            ExtendPhase.IntersectionsBuffer,
            GeneratePhase.RayBuffer,
            ImageBuffer);

        ConnectPhase = new ConnectPhase(
            Manager,
            "/../../../../Gpu/Programs/connect.cl",
            "connect",
            ShadePhase.ShadowRaysBuffer,
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            ExtendPhase.IntersectionsBuffer);
    }

    public int[] Execute()
    {
        // Generate initial rays
        // GeneratePhase.EnqueueExecute(Manager, GlobalSize, LocalSize);

        // Keep looping all phases until we run out of samples
        /*for (int sample = 0; sample < Camera.Samples; sample++)
        {
            ExtendPhase.EnqueueExecute(Manager, GlobalSize, LocalSize);
            ShadePhase.EnqueueExecute(Manager, GlobalSize, LocalSize);
            ConnectPhase.EnqueueExecute(Manager, GlobalSize, LocalSize);
        }*/
        
        ShadePhase.EnqueueExecute(Manager, GlobalSize, LocalSize);
        
        // wait for all queued commands to finish
        var err = Manager.Cl.Finish(Manager.Queue.Id);
        
        if (err != (int)ErrorCodes.Success)
        {
            throw new Exception($"Error {err}: finishing queue");
        }
        
        Manager.ReadBufferToHost(ImageBuffer, out int[] colors);
        
        return colors;
    }
}
