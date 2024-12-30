namespace Gpu;

public class Memory<T> where T : unmanaged
{
    public List<Buffer> InputBuffers { get; private set; }
    public Buffer OutputBuffer;

    public Memory(OpenCLManager manager, Buffer outputBuffer, params Buffer[] buffers)
    {
        InputBuffers = buffers.ToList();
        OutputBuffer = outputBuffer;
    }
}
