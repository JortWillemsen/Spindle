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
    public Memory Memory;
    public nuint[] GlobalSize { get; private set; }
    public nuint[] LocalSize { get; private set; }


    public unsafe OpenCLManager()
    {
        Cl = CL.GetApi();
        Context = new Context(this);
        Programs = new List<ClProgram>();
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
    
    public OpenCLManager AddProgram(string path)
    {
        Programs.Add(new ClProgram(this, path));
        return this;
    } 

    public OpenCLManager AddKernel(string name, params Buffer[] arguments)
    {
        var kernel = new Kernel(this, Programs[0], name);
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
        Cl.ReleaseContext(Context.Id);
    }

    public String GetKernelId(string kernel)
    {
        return this.Kernels.Select(k => k.Name).First();
    }
}
