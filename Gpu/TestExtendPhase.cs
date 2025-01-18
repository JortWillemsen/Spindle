﻿//
// This is code to test a single GPU kernel
//

using Gpu.OpenCL;
using Gpu.Pipeline;
using Silk.NET.OpenCL;
using System.Numerics;

namespace Gpu;

public static partial class KernelTests
{
    public static void TestExtendPhase()
    {
        // Prepare input data
        const int numberOfRays = 16;

        // ClSphere[] spheres = Enumerable.Repeat(new ClSphere()
        // {
        //     Material = 3,
        //     Position = new ClFloat3 { X = 0, Y = 0, Z = 6 },
        //     Radius = 2
        // }, numberOfRays).ToArray();
        ClSphere[] spheres =
        {
            new() { Material = 3, Position = new ClFloat3 { X = 0, Y = 0, Z = 11 }, Radius = 2 },
            new() { Material = 2, Position = new ClFloat3 { X = 0, Y = 0, Z = 6 }, Radius = 2 },
            new() { Material = 1, Position = new ClFloat3 { X = 0, Y = 0, Z = -200 }, Radius = 100 },
        };

        ClTriangle[] triangles = Enumerable.Repeat(new ClTriangle()
        {
            Material = 4,
            V1 = new ClFloat3 { X = 0,  Y = 1, Z = 1 },
            V2 = new ClFloat3 { X = -1, Y = 0, Z = 0 },
            V3 = new ClFloat3 { X = 1,  Y = 0, Z = 0 },
        }, numberOfRays).ToArray();

        ClSceneInfo[] sceneInfo = { new() { NumSpheres = spheres.Length, NumTriangles = triangles.Length } };

        ClRay[] extensionRays = Enumerable.Repeat(
            new ClRay
            {
                Direction = new ClFloat3 { X = 0, Y = 0, Z = 1 },
                Origin = new ClFloat3 { X = 0, Y = 0, Z = -3f },
                // T = 390,
                // Object_id = 1
            },
            numberOfRays).ToArray();


        // Prepare OpenCL
        OpenCLManager manager = new();

        ReadOnlyBuffer<ClSceneInfo> sceneInfoBuffer = new(manager, sceneInfo);
        ReadOnlyBuffer<ClSphere> sphereBuffer = new(manager, spheres);
        ReadOnlyBuffer<ClTriangle> triangleBuffer = new(manager, triangles);
        ReadWriteBuffer<ClQueueStates> queueStates = new(manager, new[] { new ClQueueStates() }); // Set all lengths to 0
        ReadWriteBuffer<uint> extendRayQueue = new(manager, new uint[4_000_000 / sizeof(uint)]);
        ReadWriteBuffer<ClRay> extensionRaysBuffer = new(manager, extensionRays);

        manager.AddBuffers(sceneInfoBuffer, sphereBuffer, triangleBuffer, queueStates, extendRayQueue, extensionRaysBuffer);
        manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");
        ExtendPhase phase = new(manager, "/../../../../Gpu/Programs/extend.cl", "extend",
            sceneInfoBuffer, sphereBuffer, triangleBuffer, queueStates, extendRayQueue, extensionRaysBuffer);

        var globalSize = new nuint[2]
        {
            (nuint)MathF.Ceiling(MathF.Sqrt(numberOfRays)),
            (nuint)MathF.Ceiling(MathF.Sqrt(numberOfRays))
        };
        var localSize = new nuint[2] { 2, 2 };

        // Execute
        phase.EnqueueExecute(manager, globalSize, localSize);
        var err = manager.Cl.Finish(manager.Queue.Id);

        if (err != (int)ErrorCodes.Success)
        {
            throw new Exception($"Error {err}: finishing queue");
        }

        // manager.ReadBufferToHost(phase.DebugBuffer, out ClFloat3[] result);
        manager.ReadBufferToHost(extensionRaysBuffer, out ClRay[] result);
        for (int index = 0; index < result.Length; index++)
        {
            var item = result[index];
            Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
        }
    }
}
