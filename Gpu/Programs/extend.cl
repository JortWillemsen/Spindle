#include "structs.h"

__kernel void extend(
  __global const SceneInfo *sceneInfo,
  __global const Sphere *spheres,
  __global const Triangle * triangles,
  __global Ray *rays,
  __global float3 *debug)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    uint i = x + y * get_global_size(0);

    // Takes rays from the ray buffer and calculates intersections with the scene.
    // Writes for each ray an intersection object to the intersections buffer.

    debug[i] = 69.0;
}