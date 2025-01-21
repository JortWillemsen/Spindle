//
// This is code to test a single GPU kernel
//

using Gpu.OpenCL;
using Gpu.Pipeline;
using Silk.NET.OpenCL;
using System.Numerics;

namespace Gpu;

public static partial class KernelTests
{
    public static void TestLogicPhase()
    {
        // Prepare input data
        const int numberOfRays = 16;

        ClMaterial[] materials = {
            new()
            {
                Type = MaterialType.Diffuse,
                Albedo = .78f,
                Color = new ClFloat3 { X = 20, Y = 30, Z = 40 },
            },
            new()
            {
                Type = MaterialType.Diffuse,
                Albedo = .69f,
                Color = new ClFloat3 { X = 25, Y = 35, Z = 45 },
            },
            new()
            {
                Type = MaterialType.Reflective,
                Albedo = 1f,
                Color = new ClFloat3 { X = 100, Y = 101, Z = 102 },
            }
        };

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

        ClPathState[] pathStates = Enumerable.Repeat(
            new ClPathState
            {
                Direction = new ClFloat3 { X = 0, Y = 0, Z = 1 },
                Origin = new ClFloat3 { X = 0, Y = 0, Z = 0.5f },
                T = 390,
                ObjectId = 1
            },
            numberOfRays).ToArray();


        // Prepare OpenCL
        OpenCLManager manager = new();

        ReadWriteBuffer<ClQueueStates> queueStates = new(manager, new[] { new ClQueueStates() }); // Set all lengths to 0
        ReadWriteBuffer<uint> newRayQueue = new(manager, new uint[4_000_000 / sizeof(uint)]);
        ReadWriteBuffer<ClPathState> pathStatesBuffer = new(manager, pathStates);
        ReadOnlyBuffer<ClMaterial> materialsBuffer = new(manager, materials);
        ReadOnlyBuffer<ClSceneInfo> sceneInfoBuffer = new(manager, sceneInfo);
        ReadOnlyBuffer<ClSphere> sphereBuffer = new(manager, spheres);
        ReadOnlyBuffer<ClTriangle> triangleBuffer = new(manager, triangles);
        ReadWriteBuffer<uint> imageBuffer = new(manager, new uint[numberOfRays]); // TODO: musn't this be added as a buffer as well (Manager.AddBuffer)?

        manager.AddBuffers(pathStatesBuffer, materialsBuffer, sceneInfoBuffer, sphereBuffer, triangleBuffer, imageBuffer);
        manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");
        LogicPhase phase = new(manager, "/../../../../Gpu/Programs/logic.cl", "logic",
            queueStates, newRayQueue, pathStatesBuffer, materialsBuffer, sceneInfoBuffer, sphereBuffer, triangleBuffer, imageBuffer);

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

        // manager.EnqueueReadBufferToHost(phase.DebugBuffer, out ClFloat3[] result);
        manager.EnqueueReadBufferToHost(imageBuffer, out uint[] result);
        for (int index = 0; index < result.Length; index++)
        {
            var item = result[index];
            Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
        }
    }
}
