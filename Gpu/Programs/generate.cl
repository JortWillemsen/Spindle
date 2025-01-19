﻿// Generates primary rays and writes them to the 'rays' buffer.

#include <structs.h>

__kernel void generate(
    __global const SceneInfo *scene_info,
    __global QueueStates *queue_states,
    __global uint *new_ray_queue,
    __global uint *extend_ray_queue,
    __global Ray *rays, // TODO: sometimes we call this path_states, sometimes rays, lets pick one once we are finished
    __global float3 *debug)
{
    uint i = get_global_linear_id(); // new_ray_queue index
    uint ray_index = new_ray_queue[i];

    // Retrieve screen information, communicated from logic kernel
    float x = rays[ray_index].origin.x;
    float y = rays[ray_index].origin.y;
    uint width = rays[ray_index].direction[0];
    uint height = rays[ray_index].direction[1];

    // Draw line to pixel from camera
    float x_percentage = x / (width - 1);
    float y_percentage = y / (height - 1);
    float3 cam_to_pixel = scene_info->frustum_top_left
      + x_percentage * scene_info->frustum_horizontal
      - y_percentage * scene_info->frustum_vertical;

    // Create new ray
    // Note how this overwrites original screen information in buffer
    rays[ray_index].origin = scene_info->camera_position;
    rays[ray_index].direction = normalize(cam_to_pixel);

    // Enqueue extension of this primary ray in extend_ray_queue
    // TODO assumes there always is space in the queue
    uint queue_length = atomic_inc(&queue_states->extend_ray_length); // TODO uses atomic, but we know that queue_length afterwards is increased with global_size(0). I tried to remove the atomic call, prehaps give it another go?
    extend_ray_queue[queue_length] = ray_index;

    // Update state of new_ray_queue
    atomic_dec(&queue_states->new_ray_length);

    // Move unprocessed part of queue back to begin of buffer (always less than 1 local_size amount of items)
    uint local_id = get_local_id(0);
    new_ray_queue[local_id] = new_ray_queue[get_global_size(0) + local_id];
}