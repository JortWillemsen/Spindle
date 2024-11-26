using System.Drawing;
using System.Numerics;
using Engine.Cameras;
using Engine.Geometry;
using Engine.Materials;
using Engine.Renderers;
using Renderer.Display;

Console.WriteLine("Welcome to Spindle!");

const float aspectRatio = 16f / 9f;

const int imageWidth = 400;
const int imageHeight = (int)(imageWidth / aspectRatio);
const int maxDepth = 50;
const int samples = 100;

// Camera
const float fov = 65f;

var up = new Vector3(0, 1, 0);
var front = new Vector3(0, 0, 1);

var camera = new BasicCamera(Vector3.Zero, up, front, new Rectangle(0, 0, imageWidth, imageHeight), fov, maxDepth, samples);

var matGround = new Diffuse(new Vector3(0.8f, 0.8f, 0f), 0.5f);
var matCenter = new Diffuse(new Vector3(0.1f, 0.2f, 0.5f), 0.5f);

var sphere1 = new Sphere(new Vector3(0f, -100.5f, 1f), matGround, 100f);
var sphere2 = new Sphere(new Vector3(0f, 0f, 1.2f), matCenter, 0.5f);

var scene = new Scene(sphere1, sphere2);

var renderer = new PathTracingRenderer(scene);

var display = new SimpleDisplay(renderer, camera);

display.RenderToFile("/output/" + args[0]);
