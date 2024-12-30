using Silk.NET.OpenCL;
using System.Drawing;

namespace Gpu;

public abstract class Buffer
{
    public nint Id { get; protected set; }
}

public class InputBuffer<T> : Buffer where T : unmanaged
{
    public unsafe InputBuffer(OpenCLManager manager, T[] arr)
    {
        fixed (void* pointer = arr)
            Id = manager.Cl.CreateBuffer(manager.Context.Id, MemFlags.ReadOnly | MemFlags.CopyHostPtr,
                (nuint) (sizeof(T) * arr.Length), pointer, null);

        if (Id == IntPtr.Zero)
        {
            manager.Cleanup();
            throw new Exception("Failed to create Input Buffer");

        }
    }
}

public class OutputBuffer<T> : Buffer where T : unmanaged
{
    public unsafe OutputBuffer(OpenCLManager manager, T[] arr)
    {
        fixed (void* pointer = arr)
            Id = manager.Cl.CreateBuffer(manager.Context.Id, MemFlags.ReadWrite,
                (nuint) (sizeof(T) * arr.Length), null, null);

        if (Id == IntPtr.Zero)
        {
            manager.Cleanup();
            throw new Exception("Failed to create Output Buffer");
        }
    }
}
