using Silk.NET.OpenCL;

namespace Gpu;

public class Kernel
{
    public nint Id { get; private set; }

    public unsafe Kernel(OpenCLManager manager, ClProgram program, String name)
    {
        int error;
        Id = manager.Cl.CreateKernel(program.Id, name, &error);

        if (Id == IntPtr.Zero)
        {
            manager.Cleanup();
            throw new Exception("Failed to create kernel");
        }
        
        manager.Kernels.Add(this);
    }

    public unsafe void SetArguments(OpenCLManager manager, List<Buffer> args, Buffer output)
    {
        int errNum = manager.Cl.SetKernelArg(Id, (uint) args.Count, (nuint) sizeof(nuint), output.Id);

        for (int i = 0; i < args.Count; i++)
        {
            errNum |= manager.Cl.SetKernelArg(Id, (uint) i, (nuint) sizeof(nuint), args[i].Id);
        }

        if (errNum != (int)ErrorCodes.Success)
        {
            manager.Cleanup();
            throw new Exception("Error setting kernel arguments.");
        }
    }
}
