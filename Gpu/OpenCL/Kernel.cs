using Silk.NET.OpenCL;

namespace Gpu.OpenCL;

public class Kernel
{
    public nint Id { get; private set; }
    public String Name { get; private set; }

    public unsafe Kernel(OpenCLManager manager, ClProgram program, String name)
    {
        int error;
        Id = manager.Cl.CreateKernel(program.Id, name, &error);
        Name = name;
        
        if (Id == IntPtr.Zero)
        {
            manager.Cleanup();
            throw new Exception($"Failed to create kernel: {error}");
        }
    }

    public unsafe void SetArguments(OpenCLManager manager, Buffer[] args)
    {
        int errNum = 0;

        for (int i = 0; i < args.Length; i++)
        {
            errNum |= manager.Cl.SetKernelArg(Id, (uint) i, (nuint) sizeof(nuint), args[i].Id);
        }

        if (errNum != (int)ErrorCodes.Success)
        {
            manager.Cleanup();
            throw new Exception($"Error {errNum}: setting kernel arguments.");
        }
    }
}
