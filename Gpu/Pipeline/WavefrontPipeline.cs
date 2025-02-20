﻿using Engine.Scenes;
using Gpu.Cameras;
using Gpu.OpenCL;
using Silk.NET.OpenCL;

namespace Gpu.Pipeline;

public class WavefrontPipeline
{
    public OpenCLManager Manager { get; private set; }
    public OpenCLCamera  Camera  { get; private set; }

    public nuint[]                        GlobalSize           { get; set; }
    public nuint[]                        LocalSize            { get; set; }
    public ClSceneBuffers                 SceneBuffers         { get; private set; }
    public ReadWriteBuffer<uint>          RandomStatesBuffer   { get; private set; }
    public GeneratePhase                  GeneratePhase        { get; private set; }
    public LogicPhase                     LogicPhase           { get; private set; }
    public ShadeDiffusePhase              ShadeDiffusePhase    { get; private set; }
    public ShadeReflectivePhase           ShadeReflectivePhase { get; private set; }
    public ExtendPhase                    ExtendPhase          { get; private set; }
    public ShadowPhase                    ShadowPhase          { get; private set; }
    public ReadWriteBuffer<int>           ImageBuffer          { get; private set; }
    public ReadWriteBuffer<uint>          NewRayQueue          { get; private set; }
    public ReadWriteBuffer<uint>          ExtendRayQueue       { get; private set; }
    public ReadWriteBuffer<uint>          ShadeDiffuseQueue    { get; private set; }
    public ReadWriteBuffer<uint>          ShadeReflectiveQueue { get; private set; }
    public ReadWriteBuffer<uint>          ShadowRayQueue       { get; private set; }
    public ReadWriteBuffer<ClQueueStates> QueueStates          { get; private set; }
    public Buffer                         DebugBuffer          { get; private set; }

    public WavefrontPipeline(
        Scene scene, 
        OpenCLCamera camera)
    {
        Manager = new OpenCLManager();
        Camera = camera;
        SceneBuffers = BufferConverter.ConvertSceneToBuffers(Manager, scene, camera);

        GlobalSize = new nuint[2] { (nuint)camera.ImageSize.Width, (nuint)camera.ImageSize.Height };
        LocalSize = new nuint[2] { 32, 32 }; // TODO: let OpenCL decide by passing NULL, NULL (Or just NULL?)

        // Find number of rays, used for calculating buffer sizes
        var numOfRays = camera.ImageSize.Width * camera.ImageSize.Height;

        // Create buffer with random seeds
        Random random = new(20283497); // TODO: supply custom seed
        uint[] randomStates = Enumerable.Range(0, numOfRays).Select(_ => (uint) random.Next()).ToArray();
        RandomStatesBuffer = new ReadWriteBuffer<uint>(Manager, randomStates);

        // Add structs and functions that will be bound with every program
        Manager.AddUtilsProgram("structs.h", "structs.h");
        Manager.AddUtilsProgram("random.cl", "random.cl");
        Manager.AddUtilsProgram("utils.cl", "utils.cl");

        // Prepare output buffer
        ImageBuffer = new ReadWriteBuffer<int>(Manager, new int[numOfRays]);

        // Queue of 4 MB can hold 1_000_000 path state indices
        QueueStates = new ReadWriteBuffer<ClQueueStates>(Manager, new[] { new ClQueueStates() });
        NewRayQueue = new ReadWriteBuffer<uint>(Manager, new uint[4_000_000 / sizeof(uint)]);
        ExtendRayQueue = new ReadWriteBuffer<uint>(Manager, new uint[4_000_000 / sizeof(uint)]);
        ShadeDiffuseQueue = new ReadWriteBuffer<uint>(Manager, new uint[4_000_000 / sizeof(uint)]);
        ShadeReflectiveQueue = new ReadWriteBuffer<uint>(Manager, new uint[4_000_000 / sizeof(uint)]);
        ShadowRayQueue = new ReadWriteBuffer<uint>(Manager, new uint[4_000_000 / sizeof(uint)]);

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
            NewRayQueue,
            ExtendRayQueue,
            ShadeDiffuseQueue,
            ShadowRayQueue);

        // Define pipeline dataflow (connect pipes)

        GeneratePhase = new GeneratePhase(
            Manager, "generate.cl", "generate",
            SceneBuffers.SceneInfo,
            QueueStates,
            NewRayQueue,
            ExtendRayQueue,
            numOfRays);

        ShadeDiffusePhase = new ShadeDiffusePhase(
            Manager, "shade_diffuse.cl", "shade_diffuse",
            SceneBuffers.Materials,
            QueueStates,
            ShadeDiffuseQueue,
            ExtendRayQueue,
            ShadowRayQueue,
            RandomStatesBuffer,
            GeneratePhase.PathStates,
            SceneBuffers.Spheres);

        ShadeReflectivePhase = new ShadeReflectivePhase(
            Manager, "shade_reflective.cl", "shade_reflective",
            SceneBuffers.Materials,
            QueueStates,
            ShadeReflectiveQueue,
            ExtendRayQueue,
            ShadowRayQueue,
            RandomStatesBuffer,
            GeneratePhase.PathStates,
            SceneBuffers.Spheres);

        ExtendPhase = new ExtendPhase(
            Manager, "extend.cl", "extend",
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            QueueStates,
            ExtendRayQueue,
            GeneratePhase.PathStates);

        ShadowPhase = new ShadowPhase(
            Manager, "shadow.cl", "shadow",
            SceneBuffers.SceneInfo,
            QueueStates,
            ShadowRayQueue,
            GeneratePhase.PathStates,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles);

        LogicPhase = new LogicPhase(
            Manager, "logic.cl", "logic",
            QueueStates,
            NewRayQueue,
            ShadeDiffuseQueue,
            ShadeReflectiveQueue,
            GeneratePhase.PathStates,
            SceneBuffers.Materials,
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            ImageBuffer);
    }

    public int[] Execute()
    {
        // Performs one iteration of the Wavefront implementation
        // TODO: we could let the logic kernel call other kernels for less IO

        // NOTE: Setting warpSize to something like 32 ensures that only multiples of 32 are dequeued and processed,
        // ensuring high occupancy. However, it was difficult to not run the logic kernel over already enqueued pixels.
        // Thus we set it to 1 for now, so in some scenarios some rays are checked twice.
        const uint warpSize = 1u; // TODO: let OpenCL decide what's best based on GPU

        // Logic phase
        LogicPhase.EnqueueExecute(Manager, GlobalSize, LocalSize);

        // Manager.ReadBufferToHost(GeneratePhase.PathStates, out ClPathState[] pathStates0);
        // Manager.ReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue0);
        // Manager.ReadBufferToHost(NewRayQueue, out uint[] newRayQueue0);
        // Manager.ReadBufferToHost(ShadeDiffuseQueue, out uint[] shadeQueue0);
        // Manager.ReadBufferToHost(ShadowRayQueue, out uint[] shadowRayQueue0);
        // Manager.ReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug0);
        // Manager.ReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug0);
        // Manager.ReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug0);
        // Manager.ReadBufferToHost(ShadeDiffusePhase.DebugBuffer, out ClFloat3[] shadeDebug0);

        // Generate phase
        Manager.ReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesNewRay);
        // Based on states of queues, set kernel size (let no thread be idle)
        uint queuedNewRayCount = queueStatesNewRay[0].NewRayLength;
        if (queuedNewRayCount > 0)
        {
            uint workItems;
            uint localSize;
            if (queuedNewRayCount < warpSize)
            {
                workItems = queuedNewRayCount;
                localSize = queuedNewRayCount;
            }
            else // Find the biggest multiple of warpSize
            {
                workItems = queuedNewRayCount / warpSize * warpSize;
                localSize = warpSize;
            }
            GeneratePhase.EnqueueExecute(Manager, new nuint[] { workItems }, new nuint[] { localSize }, dimensions: 1);
        };

        // Manager.ReadBufferToHost(GeneratePhase.PathStates, out ClPathState[] pathStates1);
        // Manager.ReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue1);
        // Manager.ReadBufferToHost(NewRayQueue, out uint[] newRayQueue1);
        // Manager.ReadBufferToHost(ShadeDiffuseQueue, out uint[] shadeQueue1);
        // Manager.ReadBufferToHost(ShadowRayQueue, out uint[] shadowRayQueue1);
        // Manager.ReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug1);
        // Manager.ReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug1);
        // Manager.ReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug1);
        // Manager.ReadBufferToHost(ShadeDiffusePhase.DebugBuffer, out ClFloat3[] shadeDebug1);
        
        // Shade diffuse phase
        Manager.ReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesShadeDiffuse);
        // Based on states of queues, set kernel size (let no thread be idle)
        uint queuedShadeDiffuseCount = queueStatesShadeDiffuse[0].ShadeDiffuseLength;
        if (queuedShadeDiffuseCount > 0)
        {
            uint workItems;
            uint localSize;
            if (queuedShadeDiffuseCount < warpSize)
            {
                workItems = queuedShadeDiffuseCount;
                localSize = queuedShadeDiffuseCount;
            }
            else // Find the biggest multiple of warpSize
            {
                workItems = queuedShadeDiffuseCount / warpSize * warpSize;
                localSize = warpSize;
            }
            ShadeDiffusePhase.EnqueueExecute(Manager, new nuint[] { workItems }, new nuint[] { localSize }, dimensions: 1);
        }

        // Manager.ReadBufferToHost(GeneratePhase.PathStates, out ClPathState[] pathStates2);
        // Manager.ReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue2);
        // Manager.ReadBufferToHost(NewRayQueue, out uint[] newRayQueue2);
        // Manager.ReadBufferToHost(ShadeDiffuseQueue, out uint[] shadeQueue2);
        // Manager.ReadBufferToHost(ShadowRayQueue, out uint[] shadowRayQueue2);
        // Manager.ReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug2);
        // Manager.ReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug2);
        // Manager.ReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug2);
        // Manager.ReadBufferToHost(ShadeDiffusePhase.DebugBuffer, out ClFloat3[] shadeDebug2);

        // Shade reflective phase
        Manager.ReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesShadeReflective);
        // Based on states of queues, set kernel size (let no thread be idle)
        uint queuedShadeReflectiveCount = queueStatesShadeReflective[0].ShadeReflectiveLength;
        if (queuedShadeReflectiveCount > 0)
        {
            uint workItems;
            uint localSize;
            if (queuedShadeReflectiveCount < warpSize)
            {
                workItems = queuedShadeReflectiveCount;
                localSize = queuedShadeReflectiveCount;
            }
            else // Find the biggest multiple of warpSize
            {
                workItems = queuedShadeReflectiveCount / warpSize * warpSize;
                localSize = warpSize;
            }
            ShadeReflectivePhase.EnqueueExecute(Manager, new nuint[] { workItems }, new nuint[] { localSize }, dimensions: 1);
        }

        // Manager.ReadBufferToHost(GeneratePhase.PathStates, out ClPathState[] pathStates3);
        // Manager.ReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue3);
        // Manager.ReadBufferToHost(NewRayQueue, out uint[] newRayQueue3);
        // Manager.ReadBufferToHost(ShadeDiffuseQueue, out uint[] shadeQueue3);
        // Manager.ReadBufferToHost(ShadowRayQueue, out uint[] shadowRayQueue3);
        // Manager.ReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug3);
        // Manager.ReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug3);
        // Manager.ReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug3);
        // Manager.ReadBufferToHost(ShadeDiffusePhase.DebugBuffer, out ClFloat3[] shadeDebug3);

        // Extend phase
        Manager.ReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesExtendRay);
        // Based on states of queues, set kernel size (let no thread be idle)
        uint queuedExtendRayCount = queueStatesExtendRay[0].ExtendRayLength;
        if (queuedExtendRayCount > 0)
        {
            uint workItems;
            uint localSize;
            if (queuedExtendRayCount < warpSize)
            {
                workItems = queuedExtendRayCount;
                localSize = queuedExtendRayCount;
            }
            else // Find the biggest multiple of warpSize
            {
                workItems = queuedExtendRayCount / warpSize * warpSize;
                localSize = warpSize;
            }
            ExtendPhase.EnqueueExecute(Manager, new nuint[] { workItems }, new nuint[] { localSize }, dimensions: 1);
        }

        // Manager.ReadBufferToHost(GeneratePhase.PathStates, out ClPathState[] pathStates4);
        // Manager.ReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue4);
        // Manager.ReadBufferToHost(NewRayQueue, out uint[] newRayQueue4);
        // Manager.ReadBufferToHost(ShadeDiffuseQueue, out uint[] shadeQueue4);
        // Manager.ReadBufferToHost(ShadowRayQueue, out uint[] shadowRayQueue4);
        // Manager.ReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug3);
        // Manager.ReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug3);
        // Manager.ReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug3);
        // Manager.ReadBufferToHost(ShadeDiffusePhase.DebugBuffer, out ClFloat3[] shadeDebug3);


        // Shadow phase
        Manager.ReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesShadow);
        // Based on states of queues, set kernel size (let no thread be idle)
        uint queuedShadowRayCount = queueStatesShadow[0].ShadowRayLength;
        if (queuedShadowRayCount > 0)
        {
            uint workItems;
            uint localSize;
            if (queuedShadowRayCount < warpSize)
            {
                workItems = queuedShadowRayCount;
                localSize = queuedShadowRayCount;
            }
            else // Find the biggest multiple of warpSize
            {
                workItems = queuedShadowRayCount / warpSize * warpSize;
                localSize = warpSize;
            }
            ShadowPhase.EnqueueExecute(Manager, new nuint[] { workItems }, new nuint[] { localSize }, dimensions: 1);
        }

        // wait for all queued commands to finish for debugging purposes
        // var err = Manager.Cl.Finish(Manager.Queue.Id);
        //
        // if (err != (int)ErrorCodes.Success)
        // {
        //     throw new Exception($"Error {err}: finishing queue");
        // }

        // Manager.ReadBufferToHost(SceneBuffers.Materials, out ClMaterial[] materials);
        // Manager.ReadBufferToHost(SceneBuffers.SceneInfo, out ClSceneInfo[] sceneInfos);
        // Manager.ReadBufferToHost(SceneBuffers.Spheres, out ClSphere[] spheres);
        // Manager.ReadBufferToHost(GeneratePhase.PathStates, out ClPathState[] pathStates);
        // Manager.ReadBufferToHost(QueueStates, out ClQueueStates[] queueStatesAfterwards);
        // Manager.ReadBufferToHost(ExtendRayQueue, out uint[] extendRayQueue);
        // Manager.ReadBufferToHost(NewRayQueue, out uint[] newRayQueue);
        // Manager.ReadBufferToHost(ShadeDiffuseQueue, out uint[] shadeQueue);
        // Manager.ReadBufferToHost(ShadowRayQueue, out uint[] shadowRayQueue);
        // Manager.ReadBufferToHost(GeneratePhase.DebugBuffer, out ClFloat3[] generateDebug);
        // Manager.ReadBufferToHost(ExtendPhase.DebugBuffer, out ClFloat3[] extendDebug);
        // Manager.ReadBufferToHost(LogicPhase.DebugBuffer, out ClFloat3[] logicDebug);
        // Manager.ReadBufferToHost(ShadeDiffusePhase.DebugBuffer, out ClFloat3[] shadeDebug);

        // Display current state
        Manager.ReadBufferToHost(ImageBuffer, out int[] colors);

        // Thread.Sleep(1000);

        return colors; // TODO: return pointer or write directly into OpenGL memory
    }
}
