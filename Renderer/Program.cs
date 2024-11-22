// See https://aka.ms/new-console-template for more information

using System.Numerics;
using Engine.Cameras;
using Engine.Geometry;

Console.WriteLine("Welcome to Spindle!");

const float aspectRatio = 16f / 9f;

var imageWidth = 400;
var imageHeight = (int)(imageWidth / aspectRatio);
var maxDepth = 25;
var samples = 20;

// Camera
var focalLength = 1f;
var viewportHeight = 2f;

var camera = new BasicCamera(aspectRatio, focalLength, imageWidth, imageHeight, maxDepth, samples, Vector3.Zero, Vector3.Up, Vector3.Front);

var matGround = new LambertianDiffuse(new Vector3(0.8f, 0.8f, 0f), 0.5f);
var matCenter = new LambertianDiffuse(new Vector3(0.1f, 0.2f, 0.5f), 0.5f);
var matLeft = new Dielectric(1.5f);
var matBubble = new Dielectric(1f / 1.5f);
var matRight = new Reflective(new Vector3(0.8f, 0.6f, 0.2f), 1f);

var sphere1 = new Sphere(new Vector3(0f, -100.5f, -1f), 100f, matGround);
var sphere2 = new Sphere(new Vector3(0f, 0f, -1.2f), 0.5f, matCenter);
var sphere3 = new Sphere(new Vector3(-1f, 0f, -1f), 0.5f, matLeft);
var sphere5 = new Sphere(new Vector3(-1f, 0f, -1f), 0.4f, matBubble);
var sphere4 = new Sphere(new Vector3(1f, 0f, -1f), 0.5f, matRight);

var scene = new Scene(sphere1, sphere2, sphere3, sphere4, sphere5);

ImageRenderer.RenderToFile(camera, scene, "/output/" + args[0]);