using Silk.NET.OpenCL;

namespace Gpu;

public class Context
{
    public nint Id { get; private set; }
    
    public Device Device { get; private set; }
    
    public unsafe Context(OpenCLManager manager)
    {
        // Find the platforms
        var errNum = manager.Cl.GetPlatformIDs(1, out nint firstPlatformId, out uint numPlatforms);
        
        if (errNum != (int) ErrorCodes.Success || numPlatforms <= 0)
        {
            throw new Exception("Failed to find any OpenCL platforms");
        }
        
        nint[] contextProperties = new nint[]
        {
            (nint)ContextProperties.Platform,
            firstPlatformId,
            0
        };

        fixed (nint* p = contextProperties)
        {
            nint context = manager.Cl.CreateContextFromType(p, DeviceType.Gpu, null, null, out errNum);
            if (errNum != (int)ErrorCodes.Success)
            {
                Console.WriteLine("Could not create GPU context, trying CPU...");

                context = manager.Cl.CreateContextFromType(p, DeviceType.Cpu, null, null, out errNum);

                if (errNum != (int)ErrorCodes.Success)
                {
                    throw new Exception("Failed to create an OpenCL GPU or CPU context.");

                }

                // Returning CPU context
                Id = context;
            }

            // Returning GPU context
            Id = context;
            
            errNum = manager.Cl.GetContextInfo(context, ContextInfo.Devices, 0, null, out nuint deviceBufferSize);
            if (errNum != (int) ErrorCodes.Success)
            {
                throw new Exception("Failed call to clGetContextInfo(...,GL_CONTEXT_DEVICES,...)");
            }

            if (deviceBufferSize <= 0)
            {
                throw new Exception("No devices available.");
            }

            nint[] devices = new nint[deviceBufferSize / (nuint) sizeof(nuint)];
            fixed (void* pValue = devices)
            {
                int er = manager.Cl.GetContextInfo(context, ContextInfo.Devices, deviceBufferSize, pValue, null);

            }
            if (errNum != (int) ErrorCodes.Success)
            {
                devices = null;
                throw new Exception("Failed to get device IDs");
            }

            Device = new Device(manager, devices[0]);
        }
    }
}
