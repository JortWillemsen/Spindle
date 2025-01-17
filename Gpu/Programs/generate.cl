// Generates primary rays and writes them to the 'rays' buffer.

#include <structs.h>

__kernel void generate(
    __global const SceneInfo *scene_info,
    __global QueueStates *queue_states,
    __global uint *extend_ray_queue,
    __global Ray *rays,
    __global float3 *debug)
{
    // Determine screen coordinates and size
    float x = get_global_id(0);
    float y = get_global_id(1);
    uint width = get_global_size(0); // assumes image size == screen size
    uint height = get_global_size(1);

    float x_percentage = x / (width - 1);
    float y_percentage = y / (height - 1);

    // Draw line to pixel from camera
    float3 cam_to_pixel = scene_info->frustum_top_left
      + x_percentage * scene_info->frustum_horizontal
      - y_percentage * scene_info->frustum_vertical;

    // Create new ray
    uint i = x + y * width;
    rays[i].origin = scene_info->camera_position;
    rays[i].direction = normalize(cam_to_pixel);

    // Enqueue extension of this primary ray
    // Notice how we do not need atomic increments here! TODO
    // TODO assumes there always is space in the queue
    uint queue_length = queue_states->extend_ray_length;
    extend_ray_queue[queue_length + i] = i; // i is the index of a ray here TODO in final pipeline this might not be the case
    uint processed_items = queue_length / 32u * 32u; // TODO: enfore in compile time that this is same as in WavefrontPipeline?
    queue_states->extend_ray_length = queue_length + processed_items;
    debug[i] = queue_states->extend_ray_length; // TODO test
}