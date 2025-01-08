namespace Gpu;

public class Memory
{
    public List<Buffer> Buffers { get; private set; }

    public Memory()
    {
        Buffers = new List<Buffer>();
    }
    
    public Memory(params Buffer[] buffers)
    {
        Buffers = buffers.ToList();
    }

    public void AddBuffer(Buffer buffer)
    {
        Buffers.Add(buffer);
    }

    public void AddBuffers(params Buffer[] buffers)
    {
        Buffers.AddRange(buffers);
    }
}
