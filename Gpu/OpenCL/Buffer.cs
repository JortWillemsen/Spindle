using Silk.NET.OpenCL;
using System.Drawing;

namespace Gpu;

public abstract class Buffer
{
    public nint Id { get; protected set; }

    public abstract nuint GetSize();
    public abstract nuint GetLength();
}

public class ReadOnlyBuffer<T> : Buffer where T : unmanaged
{
    public T[] Array { get; private set; }
    
    public unsafe ReadOnlyBuffer(OpenCLManager manager, T[] arr)
    {
        Array = arr;
        fixed (void* pointer = arr)
            Id = manager.Cl.CreateBuffer(manager.Context.Id, MemFlags.ReadOnly | MemFlags.CopyHostPtr,
                (nuint) (sizeof(T) * arr.Length), pointer, null);

        if (Id == IntPtr.Zero)
        {
            manager.Cleanup();
            throw new Exception("Failed to create Input Buffer");
        }
    }

    public override unsafe nuint GetSize() => (nuint) (Array.Length * sizeof(T));
    public override nuint GetLength() => (nuint) Array.Length;
}

public class ReadWriteBuffer<T> : Buffer where T : unmanaged
{
    public T[] Array { get; private set; }
    
    public unsafe ReadWriteBuffer(OpenCLManager manager, T[] arr)
    {
        Array = arr;
        
        fixed (void* pointer = arr)
            Id = manager.Cl.CreateBuffer(manager.Context.Id, MemFlags.ReadWrite,
                (nuint) (sizeof(T) * arr.Length), null, null);

        if (Id == IntPtr.Zero)
        {
            manager.Cleanup();
            throw new Exception("Failed to create Output Buffer");
        }
    }
    
    public override unsafe nuint GetSize() => (nuint) (Array.Length * sizeof(T));
    public override nuint GetLength() => (nuint) Array.Length;

}
