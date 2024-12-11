using System.Drawing;
using System.Numerics;
using Engine.Cameras;
using Engine.Geometry;
using Engine.Lighting;
using Engine.Materials;
using Engine.MeshImporters;
using Engine.Renderers;
using Engine.Scenes;
using Engine.Strategies.BVH;
using Renderer.Display;

const float aspectRatio = 16f / 9f;
const int windowWidth = 300;
const int windowHeight = (int)(windowWidth / aspectRatio);
const int maxDepth = 20;
const int samples = 5;
const float fov = 65f;

Console.WriteLine("Starting render");

var cameraManager = new CameraManager(new Size(windowWidth, windowHeight), CameraLayout.Matrix);
cameraManager.AddBasicCamera(new Vector3(0, 3.5f, -15f), maxDepth, samples, fov);
// cameraManager.AddCamera(new BasicCamera(new Vector3(1, 1, 3), Vector3.UnitY, new Vector3(-1, 0, -3), new Size(), fov, maxDepth, samples));
// cameraManager.AddCamera(new IntersectionTestsCamera(new Vector3(1, 1, 3), Vector3.UnitY, new Vector3(-1, 0, -3), new Size(), fov, maxDepth, samples,
//     displayedIntersectionsRange: 100));
// cameraManager.AddCamera(new TraversalStepsCamera(new Vector3(1, 1, 3), Vector3.UnitY, new Vector3(-1, 0, -3), new Size(), fov, maxDepth, samples,
//     displayedTraversalStepsRange: 200));

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

var cuteDragonImporter = new ObjMeshImporter("Assets/cute_dragon.obj", new Vector3(0, 0, 0), matCenter);
var teaPotImporter1 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(-7, -2, 0), matCenter);
var teaPotImporter2 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(7, -2, 0), matCenter);
var teaPotImporter3 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(0, 8, 20), matCenter);
var teaPotImporter4 = new ObjMeshImporter("Assets/teapot.obj", new Vector3(-20, 40, 80), matCenter);
var scene = new BvhScene(new SplitDirectionStrategy(20), objects, lights, teaPotImporter1, teaPotImporter2, teaPotImporter3, teaPotImporter4);

Console.WriteLine("Done creating acceleration structure");

var renderer = new PathTracingRenderer(scene);

// IDisplay display = new PhotoDisplay(renderer, camera);
IDisplay display = args.Length > 0
    ? new PhotoDisplay(renderer, cameraManager)
    : new OpenGLDisplay(renderer, cameraManager);

display.Show(args);
