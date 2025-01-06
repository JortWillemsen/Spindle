#include "structs.h"

__kernel void shade(
  __global Intersection *intersections,
  __global Ray *extensionRays,
  __global Ray *shadowRays,
  __global int *colors)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    
    // Calculates shading for each intersection.
    // Updates the colors buffer with the new color.
    // Traces shadow rays and writes them to the shadowRays buffer.
    // Traces extension rays and writes them to the extensionRays buffer.

    int index = (y * get_global_size(0) + x);
    colors[0] = 1;
}