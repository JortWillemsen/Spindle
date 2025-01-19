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
    public ReadWriteBuffer<uint>          NewRayQueue        { get; private set; }
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
        LocalSize = new nuint[2] { 32, 32 }; // TODO: let OpenCL descide by passing NULL, NULL (Or just NULL?)

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
        QueueStates = new ReadWriteBuffer<ClQueueStates>(Manager, new[] { new ClQueueStates() });
        NewRayQueue = new ReadWriteBuffer<uint>(Manager, new uint[4_000_000 / sizeof(uint)]);
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
            SceneBuffers.SceneInfo,
            QueueStates,
            NewRayQueue,
            ExtendRayQueue,
            numOfRays);

        ExtendPhase = new ExtendPhase(
            Manager,
            "/../../../../Gpu/Programs/extend.cl",
            "extend",
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            QueueStates,
            ExtendRayQueue,
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
            QueueStates,
            NewRayQueue,
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
        const uint warpSize = 32u; // TODO: how can we always let this match the workgroup size if we let OpenCL descide it? (See comment where local_size is defined)

        // Logic phase
        LogicPhase.EnqueueExecute(Manager, GlobalSize, LocalSize);

        // Generate phase
        Manager.EnqueueReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesNewRay);
        // Based on states of queues, set kernel size (let no thread be idle)
        uint raysToBeGenerated = Math.Max(queueStatesNewRay[0].NewRayLength / warpSize, 1) * warpSize; // Find the biggest multiple of warpSize
        if (queueStatesNewRay[0].NewRayLength <= 0)
        {
            Manager.EnqueueReadBufferToHost(ImageBuffer, out int[] finalImage); // TODO: turn into uint
            return finalImage;
        };
        GeneratePhase.EnqueueExecute(Manager, new nuint[] { raysToBeGenerated }, new nuint[] { warpSize }, dimensions: 1);

        // Manager.EnqueueReadBufferToHost(GeneratePhase.RayBuffer, out ClRay[] pathStates1);
        // Manager.EnqueueReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesAfterwards1);
        // Manager.EnqueueReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue1);
        // Manager.EnqueueReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug1);
        // Manager.EnqueueReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug1);
        // Manager.EnqueueReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug1);

        // Extend phase
        Manager.EnqueueReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesExtendRay);
        // Based on states of queues, set kernel size (let no thread be idle)
        uint raysToBeExtended = Math.Max(queueStatesExtendRay[0].ExtendRayLength / warpSize, 1) * warpSize; // Find the biggest multiple of warpSize
        ExtendPhase.EnqueueExecute(Manager, new nuint[] { raysToBeExtended }, new nuint[] { warpSize }, dimensions: 1);

        // Manager.EnqueueReadBufferToHost(GeneratePhase.RayBuffer, out ClRay[] pathStates2);
        // Manager.EnqueueReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesAfterwards2);
        // Manager.EnqueueReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue2);
        // Manager.EnqueueReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug2);
        // Manager.EnqueueReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug2);
        // Manager.EnqueueReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug2);


        // Display result
        LogicPhase.EnqueueExecute(Manager, GlobalSize, LocalSize);

        // wait for all queued commands to finish
        var err = Manager.Cl.Finish(Manager.Queue.Id);

        if (err != (int)ErrorCodes.Success)
        {
            throw new Exception($"Error {err}: finishing queue");
        }


        // Manager.EnqueueReadBufferToHost(GeneratePhase.RayBuffer, out ClRay[] pathStates);
        // Manager.EnqueueReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesAfterwards);
        // Manager.EnqueueReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue);
        // Manager.EnqueueReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug);
        // Manager.EnqueueReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug);
        // Manager.EnqueueReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug);
        Manager.EnqueueReadBufferToHost(ImageBuffer, out int[] colors); // TODO: turn into uint

        return colors;
    }
}
