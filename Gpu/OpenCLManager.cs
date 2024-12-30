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


    public unsafe OpenCLManager()
    {
        Cl = CL.GetApi();
        Context = new Context(this);
        Programs = new List<ClProgram>();
        Kernels = new List<Kernel>();
        Queue = new CommandQueue(this);

        float[] a = new float[1000];
        float[] b = new float[1000];
        float[] o = new float[1000];

        for (int i = 0; i < 1000; i++)
        {
            a[i] = i;
            b[i] = i * 2;
        }

        var input1 = new InputBuffer<float>(this, a);
        var input2 = new InputBuffer<float>(this, b);
        var outputB = new OutputBuffer<float>(this, o);

        Memory = new Memory<float>(this, outputB, input1, input2);
        
        var commandQueue = new CommandQueue(this);
        var program = new ClProgram(this, "/../../../Programs/HelloWorld.cl");
        var kernel = new Kernel(this, program, "hello_kernel");
        kernel.SetArguments(this, Memory.InputBuffers, Memory.OutputBuffer);
        
        nuint[] globalWorkSize = new nuint[1] { 1000 };
        nuint[] localWorkSize = new nuint[1] { 1 };

        Cl.EnqueueNdrangeKernel(
            Queue.Id, 
            kernel.Id, 
            1, 
            (nuint*) null, 
            globalWorkSize, 
            localWorkSize, 
            0, 
            (nint*) null, 
            (nint*) null);
        
        float[] output = new float[1000];
        
        fixed (void* pValue = output)
        {
            // Read the output buffer back to the Host
            Cl.EnqueueReadBuffer(Queue.Id, Memory.OutputBuffer.Id, true, 0, 1000 * sizeof(float), pValue, 0, null, null);
        }
        
        for (int i = 0; i < output.Length; i++)
        {
            Console.WriteLine(output[i]);
        }
        Console.WriteLine("Executed program succesfully.");
        Cleanup();
    }

    public void Cleanup()
    {
        Memory.InputBuffers.ForEach(b => Cl.ReleaseMemObject(b.Id));
        Cl.ReleaseMemObject(Memory.OutputBuffer.Id);
        Cl.ReleaseCommandQueue(Queue.Id);
        Kernels.ForEach(k => Cl.ReleaseKernel(k.Id));
        Programs.ForEach(p => Cl.ReleaseProgram(p.Id));
        Cl.ReleaseContext(Context.Id);
    }
}
