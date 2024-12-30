using Silk.NET.OpenCL;

namespace Gpu;

public class ClProgram
{
    public nint Id { get; private set; }
    
    public unsafe ClProgram(OpenCLManager manager, String programName)
    {
        var fileName = Directory.GetCurrentDirectory() + programName;
        
        if (!File.Exists(fileName))
            throw new Exception($"File does not exist: {fileName}");
        
        using StreamReader sr = new StreamReader(fileName);
        string clStr = sr.ReadToEnd();
        
        Id = manager.Cl.CreateProgramWithSource(manager.Context.Id, 1, new string[] { clStr }, null, null);
        
        if (Id == IntPtr.Zero)
            throw new Exception("Failed to create CL program from source.");

        var errNum = manager.Cl.BuildProgram(Id, 0, null, (byte*) null, null, null);
        
        if (errNum != (int) ErrorCodes.Success)
        {
            _ = manager.Cl.GetProgramBuildInfo(Id, 0, ProgramBuildInfo.BuildLog, 0, null, out nuint buildLogSize);
            byte[] log = new byte[buildLogSize / (nuint) sizeof(byte)];
            fixed (void* pValue = log)
            {
                manager.Cl.GetProgramBuildInfo(Id, 0, ProgramBuildInfo.BuildLog, buildLogSize, pValue, null);
            }
            string? build_log = System.Text.Encoding.UTF8.GetString(log);

            Console.WriteLine("Error in kernel: ");
            Console.WriteLine("=============== OpenCL Program Build Info ================");
            Console.WriteLine(build_log);
            Console.WriteLine("==========================================================");

            manager.Cl.ReleaseProgram(Id);
        }
        
        manager.Programs.Add(this);
    }
    
}
