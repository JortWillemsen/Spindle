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
    public Memory<float> Memory;
    public nuint[] GlobalSize { get; private set; }
    public nuint[] LocalSize { get; private set; }


    public unsafe OpenCLManager()
    {
        Cl = CL.GetApi();
        Context = new Context(this);
        Programs = new List<ClProgram>();
        Kernels = new List<Kernel>();
        Queue = new CommandQueue(this);
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

        int[] output = new int[Memory.OutputBuffer.GetLength()];
        
        fixed (void* pValue = output)
        {
            // Read the output buffer back to the Host
            Cl.EnqueueReadBuffer(Queue.Id, Memory.OutputBuffer.Id, true, 0, Memory.OutputBuffer.GetSize(), pValue, 0, null, null);
        }
        
        Console.Write("Kernel executed.");

        return output;
    }
    
    public OpenCLManager SetProgram(string path)
    {
        Programs.Add(new ClProgram(this, path));
        return this;
    } 

    public OpenCLManager SetKernel(string name)
    {
        var kernel = new Kernel(this, Programs[0], name);
        
        kernel.SetArguments(this, Memory.InputBuffers, Memory.OutputBuffer);
        Kernels.Add(kernel);
        
        return this;
    }
    
    
    
    public OpenCLManager SetBuffers(Scene scene, OpenCLCamera camera)
    {
        var buffers = BufferConverter.ConvertToBuffers(this, scene, camera);
        
        Memory = new Memory<float>(this, buffers.Output, buffers.SceneInfo, buffers.Rays, buffers.Spheres, buffers.Triangles);
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
            Memory.InputBuffers.ForEach(b => Cl.ReleaseMemObject(b.Id));
            Cl.ReleaseMemObject(Memory.OutputBuffer.Id);
        }
        
        
        Cl.ReleaseCommandQueue(Queue.Id);
        Kernels.ForEach(k => Cl.ReleaseKernel(k.Id));
        Programs.ForEach(p => Cl.ReleaseProgram(p.Id));
        Cl.ReleaseContext(Context.Id);
    }
}
