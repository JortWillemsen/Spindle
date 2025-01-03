// This kernel calculates the extension ray and shadow ray, following
// an incoming ray on a simple diffuse material.

typedef struct
{
    float3 direction;
    float3 origin;
} Ray;

__kernel void scatter(
    __local float3 *hitPosition,
    __local float3 *normal,
    __local float3 *incomingRayDirection,
    __global Ray *extensionRays,
    __global Ray *shadowRays,
    __global Ray *debugOutput)
{
    uint x = get_global_id(0);
    uint y = get_global_id(1);
    uint outputIndex = x * y;

    Ray res;
    res.direction = 0;
    res.origin = 1;

    // extensionRays[outputIndex] = res;
    // shadowRays[outputIndex] = res;
    debugOutput[outputIndex] = res;
}