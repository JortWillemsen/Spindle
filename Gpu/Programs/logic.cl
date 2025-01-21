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
  __global uint *shade_queue,
  __global PathState *path_states,
  __global Material *materials,
  __global SceneInfo *sceneInfo,
  __global Sphere *spheres,
  __global Triangle *triangles,
  __global uint *image,
  __global float3 *debug)
{
    uint i = get_global_linear_id();
    if (path_states[i].t == -66) return; // Execute order 66

    // =====> If this state has never been initialized, initialize it now by creating a new primary ray

    //  TODO: do not process rays still waiting in the queue of other kernels

    // TODO use a new variable for this. We have bytes unused anyway
    // TODO: We could also just not execute this stage as first, but this solution gives more flexibility
    if (path_states[i].t == 0) // State currently is empty - no ray has ever been shot for this pixel
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

    // =====> Process material evaluation
    // Note that even if this is the first iteration and no evaluation has been performed in a previous set,
    // we still get the correct result

    // Using NEE, we accumulate the indirect light from the previous bounce.
    // If it was occluded, then accumulated_luminance has been set to 0 in the shadow ray kernel.
    // TODO actually do shadow rays
    // TODO: what about distance attenuation?


    // =====> Process extension ray intersection

    if (any(path_states[i].latest_luminance_sample != (float3)(-1, -1, -1)))
    {
        // path_states[i].accumulated_luminance = dot(path_states[i].accumulated_luminance, path_states[i].latest_luminance_sample);
        path_states[i].accumulated_luminance *= path_states[i].latest_luminance_sample;
    }

    // TODO: implement russian roulette or max depth
    if (path_states[i].t < 0) // No intersection, terminate
    {
        float a = .5 * path_states[i].direction.y + 1.0;
        float3 ambient_color = (1 - a) * (float3)(1,1,1) + a * (float3)(0.5, 0.7, 1);

        image[i] = convert_color(path_states[i].accumulated_luminance * ambient_color);
        path_states[i].t = -66; // Execute order 66
        return;
    }


    // If there is an intersection, queue the evaluation of its contribution towards indirect light:
    // TODO: for now we only support spheres because polymorphism is kinda hard with OpenCL C
    Sphere sphere = spheres[path_states[i].object_id];
    Material mat = materials[sphere.material];
    uint shade_queue_index = atomic_inc(&queue_states->shade_length); // TODO: assumes there always is space
    shade_queue[shade_queue_index] = i; // Point to this path state

    // Set arguments for the shade phase
    path_states[i].material_id = sphere.material;


    // TODO: if a ray is finished, send new rays...
}