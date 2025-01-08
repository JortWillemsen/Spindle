#include "structs.h"

__kernel void extend(
  __global Ray *rays,
  __global SceneInfo *sceneInfo,
  __global Sphere *spheres,
  __global Triangle * triangles,
  __global Intersection *intersections)
{
    int x = get_global_id(0);
    int y = get_global_id(1);

    // Takes rays from the ray buffer and calculates intersections with the scene.
    // Writes for each ray an intersection object to the intersections buffer.
}