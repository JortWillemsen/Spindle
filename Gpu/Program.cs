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
const int numberOfRays = 8;

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
};

// Prepare OpenCL
OpenCLManager manager = new OpenCLManager();
manager.SetProgram("/../../../../Gpu/Programs/MaterialDiffuse.cl");
manager.SetBuffers(
    new OutputBuffer<ClRay>(manager, new ClRay[numberOfRays]), // debug output
    new InputBuffer<ClFloat3>(manager, hitPositions),
    new InputBuffer<ClFloat3>(manager, normals),
    new InputBuffer<ClFloat3>(manager, incomingRayDirections),
    new InputBuffer<ClRay>(manager, new ClRay[numberOfRays]), // extensionRays
    new InputBuffer<ClRay>(manager, new ClRay[numberOfRays])  // shadowRays
);
manager.SetKernel("scatter");
manager.SetWorkSize(new nuint[numberOfRays], new nuint[numberOfRays]);

// Execute
int[] result = manager.Execute();
for (int index = 0; index < result.Length; index++)
{
    int item = result[index];
    Console.WriteLine($"item at index {index}: {item}");
}
