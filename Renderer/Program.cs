using System.Drawing;
using System.Numerics;
using Engine.Cameras;
using Engine.Geometry;
using Engine.Materials;
using Engine.Renderers;
using Renderer.Display;

Console.WriteLine("Welcome to Spindle!");

const float aspectRatio = 16f / 9f;

const int windowWidth = 300 * 2;
const int windowHeight = (int)(windowWidth / aspectRatio);
const int maxDepth = 20;
const int samples = 2;

const float fov = 65f;

var cameraManager = new CameraManager(new Size(windowWidth, windowHeight), CameraLayout.Matrix);
cameraManager.AddBasicCamera(Vector3.Zero, maxDepth, samples, fov);
cameraManager.AddCamera(new BasicCamera(new Vector3(1, 1, 3), Vector3.UnitY, new Vector3(-1, 0, -3), new Size(), 70, maxDepth, samples));

var matGround = new Diffuse(new Vector3(0.8f, 0.8f, 0f), 0.8f);
var matCenter = new Diffuse(new Vector3(0.1f, 0.2f, 0.5f), 0.5f);
var matReflect = new Reflective(new Vector3(.5f, .5f, .5f), 0f);

var groundOrb = new Sphere(new Vector3(0, -100.5f, 1f), matGround, 100f);
var orb = new Sphere(new Vector3(-.6f,  0f, 1.2f), matCenter, 0.5f);
var orb2 = new Sphere(new Vector3(.6f,  0f, 1.2f), matCenter, 0.5f);
var orb3 = new Sphere(new Vector3(0f,  1f, 1.2f), matReflect, 0.5f);


var scene = new Scene(groundOrb, orb, orb2, orb3);

var renderer = new PathTracingRenderer(scene);

// IDisplay display = new PhotoDisplay(renderer, camera);
IDisplay display = args.Length > 0
	? new PhotoDisplay(renderer, cameraManager)
	: new OpenGLDisplay(renderer, cameraManager);

display.Show(args);
