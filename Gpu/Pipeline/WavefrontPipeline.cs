using Engine.Cameras;
using Engine.Scenes;

namespace Gpu.Pipeline;

public class WavefrontPipeline
{
    public OpenCLManager Manager { get; private set; }

    public ClSceneBuffers SceneBuffers { get; private set; }
    public Buffer ColorsBuffer { get; private set; }
    public GeneratePhase GeneratePhase { get; private set; }
    public ConnectPhase ConnectPhase { get; private set; }
    public ShadePhase ShadePhase { get; private set; }
    public ExtendPhase ExtendPhase { get; private set; }

    public WavefrontPipeline(
        Scene scene, 
        OpenCLCamera camera)
    {
        Manager = new OpenCLManager();
        SceneBuffers = BufferConverter.ConvertSceneToBuffers(Manager, scene);

        // Add structs that will be bound with every program
        Manager.AddUtilsProgram("/../../../../Gpu/Programs/structs.h", "structs.h");
        
        // Add buffers to the manager
        Manager.AddBuffers(SceneBuffers.SceneInfo, SceneBuffers.Spheres, SceneBuffers.Triangles);
        // Find number of rays, used for calculating buffer sizes
        var numOfRays = camera.ImageSize.Width * camera.ImageSize.Height;

        var colors = new int[numOfRays];
        ColorsBuffer = new ReadWriteBuffer<int>(Manager, colors);
        
        GeneratePhase = new GeneratePhase(
            Manager, 
            "/../../../../Gpu/Programs/generate.cl", 
            "generate", 
            numOfRays);

        ExtendPhase = new ExtendPhase(
            Manager,
            "/../../../../Gpu/Programs/extend.cl",
            "extend",
            GeneratePhase.RayBuffer,
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles);

        ShadePhase = new ShadePhase(
            Manager,
            "/../../../../Gpu/Programs/shade.cl",
            "shade",
            ExtendPhase.IntersectionsBuffer,
            GeneratePhase.RayBuffer,
            ColorsBuffer);

        ConnectPhase = new ConnectPhase(
            Manager,
            "/../../../../Gpu/Programs/connect.cl",
            "connect",
            ShadePhase.ShadowRaysBuffer,
            SceneBuffers.SceneInfo,
            SceneBuffers.Spheres,
            SceneBuffers.Triangles,
            ExtendPhase.IntersectionsBuffer);
    }

    public int[] Execute()
    {
        // TODO: Loop through phases until we don't need to anymore
        return Array.Empty<int>();
    }
}
