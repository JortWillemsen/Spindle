using Silk.NET.OpenCL;

namespace Gpu;

public class CommandQueue
{
    public nint Id { get; private set; }

    public unsafe CommandQueue(OpenCLManager manager)
    {
        Id = manager.Cl.CreateCommandQueue(manager.Context.Id, manager.Context.Device.Id, CommandQueueProperties.None, null);

        if (Id == IntPtr.Zero)
        {
            manager.Cleanup();
            throw new Exception("Failed to create Command Queue");
        }
    }

    public unsafe void EnqueueKernel(
        OpenCLManager manager, 
        nint kernelId, 
        nuint[] globalSize,
        nuint[] localSize)
    {
        manager.Cl.EnqueueNdrangeKernel(
            Id, 
            kernelId, 
            2,
            (nuint*) null, 
            globalSize, 
            localSize,
            0, 
            (nint*) null, 
            (nint*) null);
        
        Console.WriteLine("Kernel queued.");
    }
}
