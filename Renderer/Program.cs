using System.Drawing;
using System.Numerics;
using Engine.Cameras;
using Engine.Geometry;
using Engine.Materials;
using Engine.Renderers;
using Renderer.Display;
using Plane = Engine.Geometry.Plane;

Console.WriteLine("Welcome to Spindle!");

const float aspectRatio = 16f / 9f;

const int imageWidth = 400;
const int imageHeight = (int)(imageWidth / aspectRatio);
const int maxDepth = 20;
const int samples = 10;

// Camera
const float fov = 65f;

var up = new Vector3(0, 1, 0);
var front = new Vector3(0, 0, 1);
var camPos = Vector3.Zero;

var camera = new BasicCamera(camPos, up, front, new Rectangle(0, 0, imageWidth, imageHeight), fov, maxDepth, samples);

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
	? new PhotoDisplay(renderer, camera)
	: new OpenGLDisplay(renderer, camera, imageWidth, imageHeight);

display.Show(args);
