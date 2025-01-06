//
// This is code to test a single GPU kernel
//

using Engine.Cameras;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Materials;
using Engine.MeshImporters;
using Engine.Scenes;
using Gpu;
using Gpu.Pipeline;
using System.Drawing;
using System.Numerics;

Console.WriteLine("OpenCL test application");

// Prepare scene
const float fov = 65f;
const int maxDepth = 20;
OpenCLCamera camera = new OpenCLCamera(new Vector3(0, 0, -3), Vector3.UnitY, new Vector3(0, 0, 3), new Size(600, 400), fov, maxDepth);

var matBrightYellow = new Diffuse(.9f, new Vector3(0.8f, 0.8f, 0.0f));
var orbCentre = new Sphere(new Vector3(0, 0, 0), matBrightYellow, 1f);
var teaPotImporter1 = new ObjMeshImporter("../../../../Renderer/Assets/teapot.obj", new Vector3(-7, -2, 0), matBrightYellow);
var objects = new List<Geometry> { orbCentre };
objects.AddRange(teaPotImporter1.Import());
var lights = new List<LightSource> { new Spotlight(Vector3.One, Vector3.One) };

Scene scene = new Scene(objects, lights);

var pipeline = new WavefrontPipeline(scene, camera);

/*// Prepare input data
const int numberOfRays = 16;

ClFloat3[] hitPositions = new ClFloat3[numberOfRays]
{
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
    new ClFloat3 { X = 1, Y = 2, Z = 3 },
};

ClFloat3[] normals = new ClFloat3[numberOfRays]
{
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
};


ClFloat3[] incomingRayDirections = new ClFloat3[numberOfRays]
{
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
};

ClFloat3[] albedos = new ClFloat3[numberOfRays]
{
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
    new ClFloat3 { X = 80, Y = 69, Z = 111 },
};

// Prepare OpenCL
OpenCLManager manager = new OpenCLManager();
manager.AddProgram("/../../../../Gpu/Programs/MaterialDiffuse.cl");
manager.AddBuffers(
    new ReadWriteBuffer<nint>(manager, new nint[numberOfRays]),  // debug output
    new ReadOnlyBuffer<ClFloat3>(manager, hitPositions),
    new ReadOnlyBuffer<ClFloat3>(manager, normals),
    new ReadOnlyBuffer<ClFloat3>(manager, incomingRayDirections),
    new ReadOnlyBuffer<ClFloat3>(manager, albedos),
    new ReadWriteBuffer<ClRay>(manager, new ClRay[numberOfRays]), // extensionRays
    new ReadWriteBuffer<ClRay>(manager, new ClRay[numberOfRays])  // shadowRays
);
manager.AddKernel("scatter");
manager.SetWorkSize(new nuint[2] { numberOfRays / 4, numberOfRays / 4 }, new nuint[2] { 2, 2 });

// Execute
var result = manager.Execute();
for (int index = 0; index < result.Length; index++)
{
    var item = result[index];
    Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
}*/
