#include "sphere.cl"
#include "triangle.cl"
#include "sceneInfo.cl"

__kernel void trace(
    __global const SceneInfo *info,
    __global const Ray *rays,
    __global const Sphere *spheres,
    __global const Triangle *triangles,
    __global int *result)
{
    // int x = get_global_id(0);
    // int y = get_global_id(1);
    
    //result[x * info->imageHeight + y] = 0xFFFFFF;

    // Ray r = rays[x * info->imageHeight + y];
    // bool hit = false;
    // float t = FLT_MAX;

    // for (int i = 0; i < info->numSpheres; ++i) {
    //     if (intersectSphere(&r, &spheres[i], &t)) {
    //         hit = true;
            
    //         // Calculate the normal at the intersection point
    //         float3 hitPoint = r.origin + t * r.direction;
    //         float3 normal = normalize(hitPoint - spheres[i].position);
            
    //         // Convert normal to color (for simplicity, we use the normal as the color)
    //         int color = (int)((normal.x + 1.0f) * 0.5f * 255) << 16 |
    //                     (int)((normal.y + 1.0f) * 0.5f * 255) << 8 |
    //                     (int)((normal.z + 1.0f) * 0.5f * 255);
            
    //         result[x * info->imageHeight + y] = color;
    //         break;
    //     }
    // }

    // if (!hit) {
    //     // Set pixel to white if no intersection
    //     result[x * info->imageHeight + y] = 0xFFFFFF;
} 