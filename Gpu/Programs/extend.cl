// Calculates intersection of primary / extension rays.
// Updates ray buffer with intersection info.

#include "structs.h"
#include "utils.cl"

__kernel void extend(
  __global const SceneInfo *scene_info,
  __global const Sphere *spheres,
  __global const Triangle *triangles,
  __global QueueStates *queue_states,
  __global uint *extend_ray_queue,
  __global PathState *rays,
  __global float3 *debug)
{
    uint i = get_global_linear_id(); // extend_ray_queue index
    uint ray_index = extend_ray_queue[i];

    float t = -1;
    uint intersected_object = 0;

    // For every sphere, test intersection
    uint num_spheres = scene_info->num_spheres;
    for (int x = 0; x < num_spheres; x++)
    {
        float new_t = IntersectSphere(rays[ray_index], spheres[x]);
        if (new_t > 0 && (t < 0 || new_t < t))
        {
            t = new_t;
            intersected_object = x;
        }
    }

    rays[ray_index].t = t;
    rays[ray_index].object_id = intersected_object;

    // Update queue length
    atomic_dec(&queue_states->extend_ray_length);

    // Move unprocessed part of queue back to begin of buffer (always less than 1 local_size amount of items)
    uint local_id = get_local_id(0);
    extend_ray_queue[local_id] = extend_ray_queue[get_global_size(0) + local_id];
}