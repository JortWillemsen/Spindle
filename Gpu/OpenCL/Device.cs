using Silk.NET.OpenCL;
using System.Text;

namespace Gpu;

// OpenCL Device
public class Device
{
    public nint Id { get; private set; }
    public String Name { get; private set; }

    public Device(OpenCLManager manager, nint id)
    {
        Id = id;
        Name = GetDeviceInfoString(manager.Cl, id, DeviceInfo.Name);
        
        Console.WriteLine($"Using Device: {Name}");
    }
    
    public unsafe string GetDeviceInfoString(CL cl, nint device, DeviceInfo paramName)
    {
        // Query size of the property
        cl.GetDeviceInfo(device, paramName, 0, null, out nuint size);
        
        // Allocate unmanaged memory for the property
        byte[] buffer = new byte[size];
        unsafe
        {
            fixed (byte* bufferPtr = buffer)
            {
                cl.GetDeviceInfo(device, paramName, size, bufferPtr, out _);
            }
        }

        // Convert to string and return
        return Encoding.ASCII.GetString(buffer);
    }
}
