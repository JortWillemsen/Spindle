//
// This is code to test a single GPU kernel
//

using Engine.Cameras;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Materials;
using Engine.Scenes;
using Gpu;
using System.Drawing;
using System.Numerics;

Console.WriteLine("OpenCL test application");

// Prepare scene
const float fov = 65f;
const int maxDepth = 20;
OpenCLCamera camera = new OpenCLCamera(new Vector3(0, 0, -3), Vector3.UnitY, new Vector3(0, 0, 3), new Size(), fov, maxDepth);

var matBrightYellow = new Diffuse(.9f, new Vector3(0.8f, 0.8f, 0.0f));
var orbCentre = new Sphere(new Vector3(0, 0, 0), matBrightYellow, 1f);
var objects = new List<Geometry> { orbCentre };
var lights = new List<LightSource> { new Spotlight(Vector3.One, Vector3.One) };

Scene scene = new Scene(objects, lights);

// Prepare input data
const int numberOfRays = 16;

nuint[] randomStates = new nuint[numberOfRays]
{
    // TODO generate this with random seed
    69,
    420,
    10,
    20,
    9000,
    90001,
    42069,
    804930,
    02380923809,
    032093,
    480239805,
    8934820,
    89023402,
    4092094389,
    043289040,
    03488035
};

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
    new ClFloat3 { X = 0, Y = 2, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 4, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 6, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
    new ClFloat3 { X = 0, Y = 1, Z = 0 },
};


ClFloat3[] incomingRayDirections = new ClFloat3[numberOfRays]
{
    new ClFloat3 { X = 0, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 1, Z = 1 },
    new ClFloat3 { X = 1, Y = 0, Z = 1 },
    new ClFloat3 { X = 0, Y = 2, Z = 1 },
    new ClFloat3 { X = 1, Y = 2, Z = 1 },
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

float[] mat_albedos = new float[numberOfRays]
{
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f,
    6.9f
};

ClFloat3[] mat_colors = new ClFloat3[numberOfRays]
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
manager.SetProgram("/../../../../Gpu/Programs/MaterialDiffuse.cl");
manager.SetBuffers(
    new OutputBuffer<nint>(manager, new nint[numberOfRays]), // debug output
    new InputBuffer<nuint>(manager, randomStates),
    new InputBuffer<ClFloat3>(manager, hitPositions),
    new InputBuffer<ClFloat3>(manager, normals),
    new InputBuffer<float>(manager, mat_albedos),
    new InputBuffer<ClFloat3>(manager, mat_colors),
    new OutputBuffer<ClRay>(manager, new ClRay[numberOfRays]),// extensionRays
    new OutputBuffer<ClRay>(manager, new ClRay[numberOfRays]), // shadowRays
    new OutputBuffer<ClFloat3>(manager, new ClFloat3[numberOfRays]) // pixelColors
);
manager.SetKernel("scatter_diffuse");
manager.SetWorkSize(new nuint[2] {
    (nuint) MathF.Ceiling(MathF.Sqrt(numberOfRays)),
    (nuint) MathF.Ceiling(MathF.Sqrt(numberOfRays))
}, new nuint[2] { 2, 2 });

// Execute
var result = manager.Execute();
for (int index = 0; index < result.Length; index++)
{
    var item = result[index];
    Console.WriteLine($"{(index + 1).ToString(),3}: {item}");
}
