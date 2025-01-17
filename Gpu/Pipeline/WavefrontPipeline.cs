using Engine.Cameras;
using Engine.Scenes;
using Gpu.Cameras;
using Gpu.OpenCL;
using Silk.NET.Maths;
using Silk.NET.OpenCL;
using System.Drawing;

namespace Gpu.Pipeline;

public class WavefrontPipeline
{
    public OpenCLManager Manager { get; private set; }
    public OpenCLCamera  Camera  { get; private set; }

    public nuint[]                        GlobalSize         { get; set; }
    public nuint[]                        LocalSize          { get; set; }
    public ClSceneBuffers                 SceneBuffers       { get; private set; }
    public ReadWriteBuffer<uint>          RandomStatesBuffer { get; private set; }
    public GeneratePhase                  GeneratePhase      { get; private set; }
    public LogicPhase                     LogicPhase         { get; private set; }
    public ShadePhase                     ShadePhase         { get; private set; }
    public ExtendPhase                    ExtendPhase        { get; private set; }
    public ReadWriteBuffer<int>           ImageBuffer        { get; private set; }
    public ReadWriteBuffer<uint>          ExtendRayQueue     { get; private set; }
    public ReadWriteBuffer<ClQueueStates> QueueStates        { get; private set; }
    public Buffer                         DebugBuffer        { get; private set; }

    public WavefrontPipeline(
        Scene scene, 
        OpenCLCamera camera)
    {
        Manager = new OpenCLManager();
        Camera = camera;
        SceneBuffers = BufferConverter.ConvertSceneToBuffers(Manager, scene, camera);

        GlobalSize = new nuint[2] { (nuint)camera.ImageSize.Width, (nuint)camera.ImageSize.Height };
        LocalSize = new nuint[2] { 32, 32 }; // TODO: update

        // Find number of rays, used for calculating buffer sizes
        var numOfRays = camera.ImageSize.Width * camera.ImageSize.Height;

        // Create buffer with random seeds
        Random random = new(20283497); // TODO: supply custom seed
        uint[] randomStates = Enumerable.Range(0, numOfRays).Select(_ => (uint) random.Next()).ToArray();
        RandomStatesBuffer = new ReadWriteBuffer<uint>(Manager, randomStates);

        // Add structs and functions that will be bound with every program
        Manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        Manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
        Manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");

        // Prepare output buffer
        var colors = new int[numOfRays];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = i ; // TODO: remove this at some point

        ImageBuffer = new ReadWriteBuffer<int>(Manager, colors); // TODO: musn't this be added as a buffer as well (Manager.AddBuffer)?

        // Queue of 4 MB can hold 1_000_000 path state indices
        QueueStates = new ReadWriteBuffer<ClQueueStates>(Manager, new[] { new ClQueueStates { ExtendRayLength = 0 } });
        ExtendRayQueue = new ReadWriteBuffer<uint>(Manager, new uint[4_000_000 / sizeof(uint)]);

        DebugBuffer = new ReadWriteBuffer<ClFloat3>(Manager, new ClFloat3[numOfRays]);

        // Add buffers to the manager
        Manager.AddBuffers(
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            SceneBuffers.Materials,
            RandomStatesBuffer,
            ImageBuffer,
            DebugBuffer,
            QueueStates,
            ExtendRayQueue);

        // Define pipeline dataflow (connect pipes)

        GeneratePhase = new GeneratePhase(
            Manager,
            "/../../../../Gpu/Programs/generate.cl",
            "generate",
            QueueStates,
            ExtendRayQueue,
            SceneBuffers.SceneInfo,
            numOfRays);

        ExtendPhase = new ExtendPhase(
            Manager,
            "/../../../../Gpu/Programs/extend.cl",
            "extend",
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            GeneratePhase.RayBuffer);

        ShadePhase = new ShadePhase(
            Manager,
            "/../../../../Gpu/Programs/shade.cl",
            "shade",
            SceneBuffers.Materials,
            RandomStatesBuffer,
            GeneratePhase.RayBuffer,
            ImageBuffer);

        LogicPhase = new LogicPhase(
            Manager,
            "/../../../../Gpu/Programs/logic.cl",
            "logic",
            ShadePhase.ShadowRaysBuffer,
            GeneratePhase.RayBuffer,
            SceneBuffers.Materials,
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            ImageBuffer);
    }

    public int[] Execute()
    {
        // Generate initial rays
        GeneratePhase.EnqueueExecute(Manager, GlobalSize, LocalSize);


        // Based on states of queues, set kernel size (let no thread be idle)
        Manager.ReadBufferToHost(QueueStates, out ClQueueStates[] queueStates);
        // TODO
        const uint warpSize = 32u;
        uint raysToBeExtended = queueStates[0].ExtendRayLength / warpSize * warpSize; // Find the biggest multiple of warpSize

        // ExtendPhase.EnqueueExecute(Manager, new nuint[] { raysToBeExtended }, new nuint[] { warpSize }, dimensions: 1);
        ExtendPhase.EnqueueExecute(Manager, GlobalSize, LocalSize);
        // ShadePhase.EnqueueExecute(Manager, GlobalSize, LocalSize);
        LogicPhase.EnqueueExecute(Manager, GlobalSize, LocalSize);

        // wait for all queued commands to finish
        var err = Manager.Cl.Finish(Manager.Queue.Id);
        
        if (err != (int)ErrorCodes.Success)
        {
            throw new Exception($"Error {err}: finishing queue");
        }


        Manager.ReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue);
        Manager.ReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug);
        Manager.ReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug);
        Manager.ReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug);
        Manager.ReadBufferToHost(ImageBuffer, out int[] colors); // TODO: turn into uint

        return colors;
        // return new[] { 2 };
    }
}
