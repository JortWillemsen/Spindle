namespace Gpu.Pipeline;

public abstract class Phase
{
    public nint KernelId { get; protected set; }

    public void Execute(OpenCLManager manager, nuint[] globalSize, nuint[] localSize)
    {
        manager.Queue.EnqueueKernel(
            manager,
            KernelId,
            globalSize,
            localSize
        );
    }
}
