#include "structs.h"

// Accumulates radiance contributions and queues new ray generations.

__kernel void logic(
  __global Ray *shadowRays,
  __global SceneInfo *sceneInfo,
  __global Sphere *spheres,
  __global Triangle *triangles,
  __global Intersection *intersections)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    
}