#include "structs.h"

// Accumulates radiance contributions and queues new ray generations.

uint convert_color(float3 rgb_floats)
{
    uint3 rgb = convert_uint3(rgb_floats * 255); // TODO Assumes range of 0..1
    return (rgb[0] << 16) | (rgb[1] << 8) | rgb[2];
}

__kernel void logic(
  __global QueueStates *queue_states,
  __global uint *new_ray_queue,
  __global Ray *shadowRays,
  __global Ray *path_states,
  __global Material *materials,
  __global SceneInfo *sceneInfo,
  __global Sphere *spheres,
  __global Triangle *triangles,
  __global uint *image,
  __global float3 *debug)
{
    // const float3 ambient_color = (float3)(0.5, 0.7, 1); TODO: isn't every object slightly lit with ambient light?
    uint i = get_global_linear_id();

    // TODO: currently has different purpose: display color from last intersection

    Ray path_state = path_states[i];
    if (path_state.t == 0) // State currently is empty - no ray has ever been shot for this pixel
    {
        // Enqueue a new ray to be generated for this pixel
        uint queue_index = atomic_inc(&queue_states->new_ray_length);
        new_ray_queue[queue_index] = i; // Point to this ray

        // Communicate screen information (assumes this kernel is run in 2D over all pixels)
        float x = get_global_id(0);
        float y = get_global_id(1);
        path_states[i].origin = (float3)(x, y, 0); // Origin will be overwritten
        uint width = get_global_size(0); // assumes image size == screen size
        uint height = get_global_size(1);
        path_states[i].direction = (float3)(width, height, 0);

        return;
        // TODO: atomical increments of queue length can be done in a coaliscing way, see paper
    }

    // Ray has been extended, test for intersection and draw

    if (path_state.t < 0) // No intersection
    {
        // Draw a fancy sky box
        float a = .5 * path_state.direction.y + 1.0;
        float3 ambient_color = (1 - a) * (float3)(1,1,1) + a * (float3)(0.5, 0.7, 1);
        image[i] = convert_color(ambient_color);
        return;
    }

    // Intersection! Determine color
    Sphere sphere = spheres[path_state.object_id];
    Material mat = materials[sphere.material];
    image[i] = convert_color(mat.color * mat.albedo);
}