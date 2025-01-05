namespace Gpu.Pipeline;

public class ConnectPhase : Phase
{
    /// <summary>
    /// Traces shadow rays on the GPU and writes the intersections
    /// </summary>
    /// <param name="manager">OpenCLManager used for buffer creation</param>
    /// <param name="path">Path to OpenCL program</param>
    /// <param name="kernel">Kernel to execute</param>
    /// <param name="shadowRaysBuffer">Buffer of shadow rays to trace</param>
    /// <param name="sceneInfobuffer">Buffer that contains scene info</param>
    /// <param name="spheresBuffer">Buffer that contains spheres used for intersection calculations</param>
    /// <param name="trianglesBuffer">Buffer that contains triangles used for intersection calculations</param>
    /// <param name="intersectionsBuffer">Buffer that contains intersection information</param>
    public ConnectPhase(
        OpenCLManager manager, 
        String path, 
        String kernel,
        Buffer shadowRaysBuffer,
        Buffer sceneInfobuffer,
        Buffer spheresBuffer,
        Buffer trianglesBuffer,
        Buffer intersectionsBuffer)
    {
        manager.AddProgram(path)
            .AddKernel(kernel, shadowRaysBuffer, sceneInfobuffer, spheresBuffer, trianglesBuffer, intersectionsBuffer);

        KernelId = manager.GetKernelId(kernel);
    }
}
