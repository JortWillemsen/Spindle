using Engine;
using Engine.Cameras;
using System.Drawing;
using System.Numerics;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Materials;
using Engine.MeshImporters;
using Engine.Renderers;
using Engine.Scenes;
using Engine.Strategies.BVH;
using Renderer.Display;

const int windowWidth = 600;
const int windowHeight = 400;
const int maxDepth = 20;
const int samples = 5;
const float fov = 65f;
const bool renderDragon = true;

Console.WriteLine("Starting render");

var cameraManager = new CameraManager(new Size(windowWidth, windowHeight), CameraLayout.Single);

if (renderDragon)
{
    // cameraManager.AddBasicCamera(new Vector3(0, 0, -100f), maxDepth, samples, fov);
    cameraManager.AddCamera(new IntersectionTestsCamera(new Vector3(0, 0, -100f), Vector3.UnitY, new Vector3(0, 0, 1), new Size(), fov, maxDepth, samples,
        displayedIntersectionsRange: new Interval(400, 1300)));
    cameraManager.AddCamera(new TraversalStepsCamera(new Vector3(0, 0, -100f), Vector3.UnitY, new Vector3(0, 0, 1), new Size(), fov, maxDepth, samples,
    displayedTraversalStepsRange: new Interval(300, 1200)));
}
else
{
    // cameraManager.AddBasicCamera(new Vector3(0, 3.5f, -15f), maxDepth, samples, fov);
    cameraManager.AddCamera(new TraversalStepsCamera(new Vector3(0, 3.5f, -15f), Vector3.UnitY, new Vector3(0, 0, 1), new Size(), fov, maxDepth, samples,
        displayedTraversalStepsRange: new Interval(100, 400)));
    cameraManager.AddCamera(new IntersectionTestsCamera(new Vector3(0, 3.5f, -15f), Vector3.UnitY, new Vector3(0, 0, 1), new Size(), fov, maxDepth, samples,
        displayedIntersectionsRange: new Interval(100, 270)));
}

var matGround = new Diffuse(0.5f, new Vector3(0.8f, 0.8f, 0f));
var matCenter = new Diffuse(0.5f, new Vector3(.1f, .2f, .5f));
var matTriangle = new Diffuse(0.8f, new Vector3(0.5f, 0.5f, 0.5f));
var matReflect = new Reflective(1f, new Vector3(.5f, .5f, .5f), 0f);

var groundOrb = new Sphere(new Vector3(0, -100.5f, 1f), matGround, 100f);
var orb = new Sphere(new Vector3(-.6f,  0f, 1.2f), matCenter, 0.5f);
var orb2 = new Sphere(new Vector3(.6f,  0f, 1.2f), matCenter, 0.5f);
var orb3 = new Sphere(new Vector3(0f,  1f, 1.2f), matReflect, 0.5f);
var tri = new Triangle(
    new Vector3(-.5f, 1f, 2f),
    new Vector3(0f, 3f, 2f),
    new Vector3(.5f, 1f, 2f),
    matTriangle);

var objects = new List<Geometry> { };
var lights = new List<LightSource> { new Spotlight(Vector3.One, Vector3.One) };

var dragonImporter = new ObjMeshImporter("Assets/dragon.obj", new Vector3(0, 10, 0), matCenter);
var teaPotImporter1 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(-7, -2, 0), matCenter);
var teaPotImporter2 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(7, -2, 0), matCenter);
var teaPotImporter3 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(0, 8, 20), matCenter);
var teaPotImporter4 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(-20, 40, 80), matCenter);

var teapots = new MeshImporter[] { teaPotImporter1, teaPotImporter2, teaPotImporter3, teaPotImporter4 };
var dragon = new MeshImporter[] { dragonImporter };

var scene = new BvhScene(new SplitDirectionStrategy(50), objects, lights, renderDragon ? dragon : teapots);

Console.WriteLine("Done creating acceleration structure");

var renderer = new PathTracingRenderer(scene);

// IDisplay display = new PhotoDisplay(renderer, camera);
IDisplay display = args.Length > 0
    ? new PhotoDisplay(renderer, cameraManager)
    : new OpenGLDisplay(renderer, cameraManager);

display.Show(args);
