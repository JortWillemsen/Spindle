namespace Gpu.Pipeline;

public abstract class Phase
{
    public nint KernelId { get; protected set; }

    public void EnqueueExecute(OpenCLManager manager, nuint[] globalSize, nuint[] localSize, uint dimensions = 2)
    {
        manager.Queue.EnqueueKernel(
            manager,
            KernelId,
            globalSize,
            localSize,
            dimensions
        );
    }
}
