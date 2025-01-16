// Generates primary rays and writes them to the 'rays' buffer.

#include <structs.h>

__kernel void generate(
    __global const SceneInfo *scene_info,
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
}