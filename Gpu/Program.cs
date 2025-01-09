//
// This is code to test a single GPU kernel
//

using Engine.Cameras;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Materials;
using Engine.Scenes;
using Gpu;
using Gpu.Pipeline;
using Silk.NET.OpenCL;
using System.Drawing;
using System.Numerics;

Console.WriteLine("OpenCL test application");

// Prepare input data
const int numberOfRays = 16;

uint[] randomStates = new uint[numberOfRays]
{
    // TODO generate this with random seed
    1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1
    //69,
    //420,
    //10,
    //20,
    //9000,
    //90001,
    //42069,
    //804930,
    //02380923809,
    //032093,
    //480239805,
    //8934820,
    //89023402,
    //4092094389,
    //043289040,
    //03488035
};

ClIntersection[] intersections = new ClIntersection[numberOfRays]
{
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
    new()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = new ClFloat3 { X = 1, Y = 2, Z = 3 },
        Hit = true,
        T = 69f,
        Material = 420
    },
};

ClRay[] extensionRays = new ClRay[numberOfRays]
{
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
};

// Prepare OpenCL
OpenCLManager manager = new();

ReadOnlyBuffer<uint> randomStatesBuffer = new(manager, randomStates);
ReadOnlyBuffer<ClIntersection> intersectionResultsBuffer = new(manager, intersections);
ReadWriteBuffer<ClRay> extensionRaysBuffer = new(manager, extensionRays);
ReadWriteBuffer<ClFloat3> pixelColorsBuffer = new(manager, new ClFloat3[numberOfRays]);

manager.AddBuffers(randomStatesBuffer, intersectionResultsBuffer, extensionRaysBuffer, pixelColorsBuffer);
manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");
ShadePhase phase = new(manager, "/../../../../Gpu/Programs/shade.cl", "shade",
    randomStatesBuffer, intersectionResultsBuffer, extensionRaysBuffer, pixelColorsBuffer);

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

manager.ReadBufferToHost(phase.DebugBuffer, out ClFloat3[] result);
for (int index = 0; index < result.Length; index++)
{
    var item = result[index];
    Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
}
