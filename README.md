# Spindle
Weaving silk into textures.

## How to use

One can start the path tracer by starting the `Renderer` project from within his IDE.
Otherwise, go to the root folder of this project and run `dotnet build -c release`.
Then, go to `Renderer/bin/Release/net6.0` and start `./Renderer`.
The working directory is important in order to be able to load the program files.
The binary expects the working directory to contain the `Programs` folder copied from `Gpu/Programs`.

## Selecting your renderer

This project has two renderers: one simple path tracer which can optionally benefit from
a BVH acceleration structure, and one OpenCL renderer.

In `program.cs` in the `Renderer` project, you can control the renderer used and the scene.
If you use the `SampledCamera` camera, you use the CPU one to benefit from BVH speedups if enabled.
`OpenCLCamera` can be used instead to benefit from GPU speedups.

## Controls

When rendering with the CPU camera and not the OpenCL camera, the following
controls can be used (note: rendering multiple cameras is bugged since adding
the OpenCL camera):

```
Escape or Alt-F4 - Exit

WASD - Translate focussed camera along X and Z axis
QE - Translate focussed camera along Y axis
Arrow keys - Rotate focussed camera

IO - Zoom In and Out

BackSlash - Cycle trough camera layouts

HL - Increase/Decrease number of visible cameras
JK - Cycle focus through cameras
CX - Add and Delete focussed camera
```

