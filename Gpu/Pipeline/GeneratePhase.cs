using Engine.Cameras;
using Gpu.OpenCL;
using System.Numerics;

namespace Gpu.Pipeline;

public class GeneratePhase : Phase
{ 
    public Buffer RayBuffer { get; private set; }

    /// <summary>
    /// Generates primary rays on the GPU.
    /// Writes to a ray buffer that is filled on the GPU.
    /// </summary>
    /// <param name="manager">OpenCL manager for buffer creation</param>
    /// <param name="path">Path to program file</param>
    /// <param name="kernel">Name of kernel to execute</param>
    /// <param name="sceneInfoBuffer">Contains info about camera and such</param>
    /// <param name="numOfRays">Used to find the length of the ray buffer</param>
    public GeneratePhase(
        OpenCLManager manager,
        string path,
        string kernel,
        Camera camera,
        Buffer sceneInfoBuffer,
        int numOfRays)
    {
        // ClRay[] rays = new ClRay[numOfRays];
        var rays = BufferConverter.GenerateRayBuffers(manager, camera);
        // ClRay[] rays = Enumerable.Repeat( new ClRay
        // {
        //     Origin = new ClFloat3 { X = 0, Y = 0, Z = -9 },
        //     Direction = ClFloat3.FromVector3(Vector3.Normalize(new Vector3(0, 1, 1)))
        // }, numOfRays).ToArray();
        RayBuffer = new ReadWriteBuffer<ClRay>(manager, rays);

        manager.AddProgram(path, "generate.cl")
            .AddBuffers(RayBuffer)
            .AddKernel("generate.cl",kernel, sceneInfoBuffer, RayBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
