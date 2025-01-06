using Engine.Cameras;
using Engine.Scenes;
using Silk.NET.OpenCL;

namespace Gpu;

public class OpenCLManager
{
    public CL Cl { get; private set; }
    public nint Platform { get; private set; }
    public Context Context { get; private set; }
    public CommandQueue Queue { get; private set; }
    public List<Kernel> Kernels;
    public List<ClProgram> Programs;
    public List<ClProgram> UtilsPrograms;
    public Memory Memory;
    public nuint[] GlobalSize { get; private set; }
    public nuint[] LocalSize { get; private set; }


    public unsafe OpenCLManager()
    {
        Cl = CL.GetApi();
        Context = new Context(this);
        Programs = new List<ClProgram>();
        UtilsPrograms = new List<ClProgram>();
        Kernels = new List<Kernel>();
        Queue = new CommandQueue(this);
        Memory = new Memory();
    }

    public unsafe int[] Execute()
    {
        Cl.EnqueueNdrangeKernel(
            Queue.Id, 
            Kernels[0].Id, 
            2,
            (nuint*) null, 
            GlobalSize, 
            LocalSize,
            0, 
            (nint*) null, 
            (nint*) null);

        /*int[] output = new int[Memory.OutputBuffer.GetLength()];
        
        fixed (void* pValue = output)
        {
            // Read the output buffer back to the Host
            Cl.EnqueueReadBuffer(Queue.Id, Memory.OutputBuffer.Id, true, 0, Memory.OutputBuffer.GetSize(), pValue, 0, null, null);
        }
        */
        
        Console.WriteLine("Kernel executed.");

        return new int[] {};
    }

    public unsafe OpenCLManager AddUtilsProgram(string path, string name)
    {
        var utilsProgram = new ClProgram(this, path, name);

        Cl.CompileProgram(
            utilsProgram.Id,
            1,
            Context.Device.Id,
            "",
            0,
            null,
            new string[] {},
            null,
            null);
        
        UtilsPrograms.Add(new ClProgram(this, path, name));
        return this;
    }
    
    public unsafe OpenCLManager AddProgram(string path, string name)
    {
        var program = new ClProgram(this, path, name);
        var headerIds = UtilsPrograms.Select(p => p.Id).ToArray();
        var headerNames = UtilsPrograms.Select(p => p.Name).ToArray();
        fixed (
            nint* utilsPointer = headerIds)
        {
            var errNum = Cl.CompileProgram(
                program.Id,
                1,
                Context.Device.Id,
                "",
                (uint)UtilsPrograms.Count,
                utilsPointer,
                headerNames,
                null,
                null);

            if (errNum != (int) ErrorCodes.Success)
            {
                Console.WriteLine("Error code: " + errNum);
                _ = Cl.GetProgramBuildInfo(program.Id, Context.Device.Id, ProgramBuildInfo.BuildLog, 0, null, out nuint buildLogSize);
                byte[] log = new byte[buildLogSize / sizeof(byte)];
                fixed (void* pValue = log)
                {
                    Cl.GetProgramBuildInfo(program.Id,  Context.Device.Id, ProgramBuildInfo.BuildLog, buildLogSize, pValue, null);
                }
                string? build_log = System.Text.Encoding.UTF8.GetString(log);

                Console.WriteLine("Error in kernel: ");
                Console.WriteLine("=============== OpenCL Program Build Info ================");
                Console.WriteLine(build_log);
                Console.WriteLine("==========================================================");

                Cl.ReleaseProgram(program.Id);
                throw new Exception("Error COMPILING program");
            }

            var final = Cl.LinkProgram(
                Context.Id,
                1,
                Context.Device.Id,
                "",
                1,
                program.Id,
                null,
                null,
                null);

            if (final == IntPtr.Zero)
            {
                Cleanup();
                throw new Exception("Failed to link program");
            }
            
            Programs.Add(new ClProgram(final, name));
            
        }
        
            /*var id = Cl.LinkProgram(
                this.Context.Id,
                1,
                this.Context.Device.Id,
                "",
                (uint)UtilsPrograms.Count + 1,
                pointer,
                null,
                null,
                &err);*/

        return this;
    }

    public OpenCLManager AddKernel(string programName, string name, params Buffer[] arguments)
    {
        var program = Programs.Find(p => p.Name == programName);

        if (program == null)
        {
            throw new Exception("Could not find program with name: " + programName);
        }
        var kernel = new Kernel(this, program, name);
        Kernels.Add(kernel);
        kernel.SetArguments(this, arguments);
        
        return this;
    }
    
    public OpenCLManager AddBuffers(params Buffer[] buffers)
    {
        Memory.AddBuffers(buffers);
        return this;
    }

    public OpenCLManager AddBuffer(Buffer buffer)
    {
        Memory.AddBuffer(buffer);
        return this;
    }

    public OpenCLManager SetWorkSize(nuint[] global, nuint[] local)
    {
        GlobalSize = global;
        LocalSize = local;
        return this;
    }
    
    public void Cleanup()
    {
        if (Memory != null)
        {
            Memory.Buffers.ForEach(b => Cl.ReleaseMemObject(b.Id));
        }
        
        
        Cl.ReleaseCommandQueue(Queue.Id);
        Kernels.ForEach(k => Cl.ReleaseKernel(k.Id));
        Programs.ForEach(p => Cl.ReleaseProgram(p.Id));
        UtilsPrograms.ForEach(p => Cl.ReleaseProgram(p.Id));

        Cl.ReleaseContext(Context.Id);
    }

    public String GetKernelId(string kernel)
    {
        return this.Kernels.Select(k => k.Name).First();
    }
}
