// This kernel calculates the extension ray and shadow ray, following
// an incoming ray on a simple diffuse material.

typedef struct
{
    float3 direction;
    float3 origin;
} Ray;

__kernel void scatter(
    __global float3 *hitPosition,
    __global float3 *normal,
    __global float3 *incomingRayDirection,
    __global Ray *extensionRays,
    __global Ray *shadowRays,
    __global int *debugOutput)
{
    uint x = get_global_id(0);
    uint y = get_global_id(1);
    uint outputIndex = x + y * get_local_size(0);

    // Ray res;
    // res.direction = 0;
    // res.origin = 1;

    // extensionRays[outputIndex] = res;
    // shadowRays[outputIndex] = res;
    debugOutput[outputIndex] = get_global_size(0);
}