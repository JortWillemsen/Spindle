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
    public static void TestExtendPhase()
    {
        // Prepare input data
        const int numberOfRays = 16;

        ClSceneInfo[] sceneInfo = { new() { NumSpheres = 1, NumTriangles = 1} };

        ClSphere[] spheres = Enumerable.Repeat(new ClSphere()
        {
            Material = 3,
            Position = new ClFloat3 { X = 13, Y = 12, Z = 14 },
            Radius = 5
        }, numberOfRays).ToArray();

        ClTriangle[] triangles = Enumerable.Repeat(new ClTriangle()
        {
            Material = 4,
            V1 = new ClFloat3 { X = 21, Y = 21, Z = 21 },
            V2 = new ClFloat3 { X = 22, Y = 22, Z = 22 },
            V3 = new ClFloat3 { X = 23, Y = 23, Z = 23 },
        }, numberOfRays).ToArray();

        ClRay[] extensionRays = Enumerable.Repeat(
            new ClRay
            {
                Direction = new ClFloat3 { X = 10, Y = 20, Z = 30 },
                Origin = new ClFloat3 { X = -5, Y = -5, Z = -5 },
                T = 420.69f,
                Object_id = 1
            },
            numberOfRays).ToArray();


        // Prepare OpenCL
        OpenCLManager manager = new();

        ReadOnlyBuffer<ClSceneInfo> sceneInfoBuffer = new(manager, sceneInfo);
        ReadOnlyBuffer<ClSphere> sphereBuffer = new(manager, spheres);
        ReadOnlyBuffer<ClTriangle> triangleBuffer = new(manager, triangles);
        ReadWriteBuffer<ClRay> extensionRaysBuffer = new(manager, extensionRays);

        manager.AddBuffers(sceneInfoBuffer, sphereBuffer, triangleBuffer, extensionRaysBuffer);
        manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");
        ExtendPhase phase = new(manager, "/../../../../Gpu/Programs/extend.cl", "extend",
            sceneInfoBuffer, sphereBuffer, triangleBuffer, extensionRaysBuffer);

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

        manager.ReadBufferToHost(extensionRaysBuffer, out ClRay[] result);
        for (int index = 0; index < result.Length; index++)
        {
            var item = result[index];
            Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
        }
    }
}
