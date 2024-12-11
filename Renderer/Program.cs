using Engine;
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
using System.Diagnostics;

const float aspectRatio = 16f / 9f;
const int windowWidth = 300;
const int windowHeight = (int)(windowWidth / aspectRatio);
const int maxDepth = 20;
const int samples = 20;
const float fov = 65f;

const bool renderDragon = true;

var sw = new Stopwatch();
sw.Start();
Console.WriteLine("Starting render");

var cameraManager = new CameraManager(new Size(windowWidth, windowHeight), CameraLayout.Matrix);
cameraManager.AddBasicCamera(new Vector3(0, 3.5f, -15f), maxDepth, samples, fov);

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
var dragon = new MeshImporter[] { dragonImporter };
var teapots = new MeshImporter[] { teaPotImporter1, teaPotImporter2, teaPotImporter3, teaPotImporter4 };
var scene = new BvhScene(new KDTreeStrategy(20), objects, lights, sw, teapots );
// var scene = new Scene(objects, lights, sw, teaPotImporter1 );

var renderer = new PathTracingRenderer(scene);

// IDisplay display = new PhotoDisplay(renderer, camera);
IDisplay display = args.Length > 0
    ? new PhotoDisplay(renderer, cameraManager, sw)
    : new OpenGLDisplay(renderer, cameraManager);

display.Show(args);
