// Calculates intersection of primary / extension rays.
// Updates ray buffer with intersection info.

#include "structs.h"

// Returns closest intersection, preferably in positive direction (front)
float IntersectSphere(PathState ray, Sphere sphere)
{
    float3 oc = sphere.position - (ray.origin + ray.direction * 0.01f); // Prevent shadow acne
    // Hours wasted on shadow acne: 4

    // TODO: simplify a
    float a = 1; // powr(length(ray.direction), 2); (direction should always be normalized)
    float h = dot(ray.direction, oc);
    float c = powr(length(oc), 2) - sphere.radius * sphere.radius;

    // Solve quadratic formula to determine hit
    float discriminant = h * h - a * c;
    if (discriminant < 0) return -1; // No hit

    float rootedDiscriminant = sqrt(discriminant);
    float t1 = (h - rootedDiscriminant) / a;
    float t2 = (h + rootedDiscriminant) / a; // TODO: inline this value
    // Note: t1 < t2

    if (t1 < 0)
    {
        // t2 > t1 and as such:
        // - if t2 < 0, t2 is closer
        // - if t2 > 0, t2 is the only positive intersection
        // We always want to return t2 and not t1
        // The result is that the camera can be inside of an object (culling range=0)
        return t2;
    }
    return t1; // Will be closer than t2
}

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
    uint intersected_object = 0; // TODO: how to differentiate between triangle and sphere index?

    // For every sphere, test intersection
    uint num_spheres = scene_info->num_spheres;
    for (int x = 0; x < num_spheres; x++)
    {
        float new_t = IntersectSphere(rays[ray_index], spheres[x]); // TODO: for now we only support spheres
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