using Silk.NET.OpenCL;

namespace Gpu;

public class ClProgram
{
    public nint Id { get; private set; }
    public string Name { get; private set; }

    public ClProgram(nint id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public unsafe ClProgram(OpenCLManager manager, string path, string name)
    {
        var fileName = Directory.GetCurrentDirectory() + $"/Programs/{path}";
        Name = name;
        
        if (!File.Exists(fileName))
            throw new Exception($"File does not exist: {fileName}");
        
        using StreamReader sr = new StreamReader(fileName);
        string clStr = sr.ReadToEnd();
        
        Id = manager.Cl.CreateProgramWithSource(manager.Context.Id, 1, new string[] { clStr }, null, null);
        if (Id == IntPtr.Zero)
            throw new Exception("Failed to create CL program from source: " + name);
    }
    
}
