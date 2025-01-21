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
    public static void TestShadePhase()
    {
        // Prepare input data
        const int numberOfRays = 16;

        ClMaterial[] materials = {
            new()
            {
                Type = MaterialType.Diffuse,
                Albedo = .78f,
                Color = new ClFloat3 { X = .20f, Y = .30f, Z = .40f },
            },
            new()
            {
                Type = MaterialType.Diffuse,
                Albedo = .69f,
                Color = new ClFloat3 { X = .25f, Y = .35f, Z = .45f },
            },
            new()
            {
                Type = MaterialType.Reflective,
                Albedo = .5f,
                Color = new ClFloat3 { X = .100f, Y = .101f, Z = .102f },
            }
        };

        ClSphere[] spheres =
        {
            new() { Material = 2, Position = new ClFloat3 { X = 0, Y = 0, Z = 11 }, Radius = 2 },
            new() { Material = 1, Position = new ClFloat3 { X = 0, Y = 0, Z = 6 }, Radius = 3 },
            new() { Material = 0, Position = new ClFloat3 { X = 0, Y = 0, Z = -200 }, Radius = 100 },
        };


        uint[] randomStates = new uint[numberOfRays]
        {
            69, 420, 10, 20, 9000, 90001, 42069, 804930, 02380923809, 032093, 480239805,
            8934820, 89023402, 4092094389, 043289040, 03488035
        };

        ClPathState[] pathStates = Enumerable.Repeat(
            new ClPathState
            {
                Direction = ClFloat3.FromVector3(Vector3.Normalize(new Vector3(0, 0, 1))),
                Origin = new ClFloat3 { X = 0, Y = 0, Z = -1 },
                T = 4, // Calculated
                ObjectId = 1,
                MaterialId = 1,
            },
            numberOfRays).ToArray();

        // Prepare OpenCL
        OpenCLManager manager = new();

        ReadOnlyBuffer<ClMaterial> materialsBuffer = new(manager, materials);
        ReadWriteBuffer<uint> shadeQueue = new(manager, Enumerable.Range(0, numberOfRays + 4).Select(i => (uint)i).ToArray());
        ReadWriteBuffer<uint> newRayQueue = new(manager, new uint[4_000_000 / sizeof(uint)]);
        ReadWriteBuffer<uint> shadowRayQueue = new(manager, new uint[4_000_000 / sizeof(uint)]);
        ReadWriteBuffer<uint> randomStatesBuffer = new(manager, randomStates);
        ReadWriteBuffer<ClQueueStates> queueStates = new(manager, new[] { new ClQueueStates() { ShadeLength = (uint)shadeQueue.GetLength() } }); // Set all lengths to 0
        ReadWriteBuffer<ClPathState> pathStatesBuffer = new(manager, pathStates);
        ReadOnlyBuffer<ClSphere> sphereBuffer = new(manager, spheres);

        manager.AddBuffers(materialsBuffer, queueStates, shadeQueue, newRayQueue, shadowRayQueue, randomStatesBuffer, pathStatesBuffer, sphereBuffer);
        manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");
        ShadePhase phase = new(manager, "/../../../../Gpu/Programs/shade.cl", "shade",
            materialsBuffer, queueStates, shadeQueue, newRayQueue, shadowRayQueue, randomStatesBuffer, pathStatesBuffer, sphereBuffer);

        var globalSize = new nuint[2]
        {
            (nuint)MathF.Ceiling(MathF.Sqrt(numberOfRays)),
            (nuint)MathF.Ceiling(MathF.Sqrt(numberOfRays))
        };
        var localSize = new nuint[2] { 1, 1 };

        // Execute
        phase.EnqueueExecute(manager, globalSize, localSize);
        var err = manager.Cl.Finish(manager.Queue.Id);

        if (err != (int)ErrorCodes.Success)
        {
            throw new Exception($"Error {err}: finishing queue");
        }

        manager.EnqueueReadBufferToHost(phase.DebugBuffer, out ClFloat3[] debugState);
        manager.EnqueueReadBufferToHost(queueStates, out ClQueueStates[] queueStatesState);
        manager.EnqueueReadBufferToHost(shadeQueue, out uint[] shadeQueueState);
        manager.EnqueueReadBufferToHost(newRayQueue, out uint[] newRayQueueState);
        manager.EnqueueReadBufferToHost(shadowRayQueue, out uint[] shadowRayQueueState);
        manager.EnqueueReadBufferToHost(randomStatesBuffer, out uint[] randomStatesBufferState);
        manager.EnqueueReadBufferToHost(pathStatesBuffer, out ClPathState[] pathStatesBufferState);
        var result = pathStatesBufferState;
        for (int index = 0; index < result.Length; index++)
        {
            var item = result[index];
            Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
        }
    }
}
