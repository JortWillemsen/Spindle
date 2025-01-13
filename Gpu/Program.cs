//
// This is code to test a single GPU kernel
//

using Gpu;
using Gpu.OpenCL;
using Gpu.Pipeline;
using Silk.NET.OpenCL;
using System.Numerics;

// Prepare input data
const int numberOfRays = 16;

ClMaterial[] materials = new ClMaterial[] {
    new ClMaterial
    {
        Type = MaterialType.Diffuse,
        Albedo = .78f,
        Color = new ClFloat3 { X = 20, Y = 30, Z = 40 },
    },
    new ClMaterial
    {
        Type = MaterialType.Diffuse,
        Albedo = .69f,
        Color = new ClFloat3 { X = 25, Y = 35, Z = 45 },
    },
    new ClMaterial
    {
        Type = MaterialType.Reflective,
        Albedo = .5f,
        Color = new ClFloat3 { X = 100, Y = 101, Z = 102 },
    }
};


uint[] randomStates = new uint[numberOfRays]
{
    69, 420, 10, 20, 9000, 90001, 42069, 804930, 02380923809, 032093, 480239805,
    8934820, 89023402, 4092094389, 043289040, 03488035
};

ClIntersection[] intersections = Enumerable.Repeat(
    new ClIntersection()
    {
        HitPoint = new ClFloat3 { X = 20, Y = 30, Z = 40 },
        Normal = ClFloat3.FromVector3(Vector3.Normalize(new Vector3(0, 1, 1))),
        Hit = true,
        T = 69f,
        Material = 1
    },
    numberOfRays).ToArray();

ClRay[] extensionRays = Enumerable.Repeat(
    new ClRay
    {
        Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
        Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 }
    },
    numberOfRays).ToArray();

// Prepare OpenCL
OpenCLManager manager = new();

ReadOnlyBuffer<ClMaterial> materialsBuffer = new(manager, materials);
ReadOnlyBuffer<uint> randomStatesBuffer = new(manager, randomStates);
ReadOnlyBuffer<ClIntersection> intersectionResultsBuffer = new(manager, intersections);
ReadWriteBuffer<ClRay> extensionRaysBuffer = new(manager, extensionRays);
ReadWriteBuffer<ClFloat3> pixelColorsBuffer = new(manager, new ClFloat3[numberOfRays]);

manager.AddBuffers(materialsBuffer, randomStatesBuffer, intersectionResultsBuffer, extensionRaysBuffer, pixelColorsBuffer);
manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");
ShadePhase phase = new(manager, "/../../../../Gpu/Programs/shade.cl", "shade",
    materialsBuffer, randomStatesBuffer, intersectionResultsBuffer, extensionRaysBuffer, pixelColorsBuffer);

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

manager.ReadBufferToHost(pixelColorsBuffer, out ClFloat3[] result);
for (int index = 0; index < result.Length; index++)
{
    var item = result[index];
    Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
}
