//
// This is code to test a single GPU kernel
//

using Gpu.Cameras;
using Gpu.OpenCL;
using Gpu.Pipeline;
using Silk.NET.OpenCL;
using System.Drawing;
using System.Numerics;

namespace Gpu;

public static partial class KernelTests
{
    public static void TestGeneratePhase()
    {
        // Prepare input data
        const int windowWidth = 4;
        const int windowHeight = 4;
        const int numberOfRays = windowWidth * windowHeight;

        OpenCLCamera camera = new(
            new Vector3(0, 0, -4),
            Vector3.UnitY,
            new Vector3(0, 0, 1),
            new Size(windowWidth, windowHeight),
            60,
            20);

        ClSceneInfo[] sceneInfo = {
            new()
            {
                CameraPosition = ClFloat3.FromVector3(camera.Position),
                FrustumTopLeft = ClFloat3.FromVector3(camera.FrustumTopLeft),
                FrustumHorizontal = ClFloat3.FromVector3(camera.FrustumHorizontal),
                FrustumVertical = ClFloat3.FromVector3(camera.FrustumVertical),
                NumSpheres = 40,
                NumTriangles = 20
            }
        };

        // Prepare OpenCL
        OpenCLManager manager = new();

        ReadOnlyBuffer<ClSceneInfo> sceneInfoBuffer = new(manager, sceneInfo);

        manager.AddBuffers(sceneInfoBuffer);
        manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/random.cl", "random.cl");
        manager.AddUtilsProgram("/../../../../Gpu/Programs/utils.cl", "utils.cl");
        GeneratePhase phase = new(manager, "/../../../../Gpu/Programs/generate.cl", "generate",
            sceneInfoBuffer, numberOfRays);

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
        manager.ReadBufferToHost(phase.RayBuffer, out ClRay[] result);
        for (int index = 0; index < result.Length; index++)
        {
            var item = result[index];
            Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
        }
    }
}
