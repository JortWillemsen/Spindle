#include "structs.h"

// Accumulates radiance contributions and queues new ray generations.

uint convert_color(float3 rgb_floats)
{
    uint3 rgb = convert_uint3(rgb_floats * 255); // TODO Assumes range of 0..1
    return (rgb[0] << 16) | (rgb[1] << 8) | rgb[2];
}

__kernel void logic(
  __global Ray *shadowRays,
  __global Ray *extensionRays,
  __global Material *materials,
  __global SceneInfo *sceneInfo,
  __global Sphere *spheres,
  __global Triangle *triangles,
  __global uint *image,
  __global float3 *debug)
{
    uint i = get_global_id(0) + get_global_id(1) * get_global_size(0);

    // TODO: currently has different purpose: display color from last intersection

    Ray extension_ray = extensionRays[i];
    if (extension_ray.t < 0)
    {
        image[i] = 0; // No intersection, black for now
        return;
    }

    // Intersection!
    Sphere sphere = spheres[extension_ray.object_id];
    Material mat = materials[sphere.material];
    image[i] = convert_color(mat.color * mat.albedo);
}