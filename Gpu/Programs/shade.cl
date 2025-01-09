// This kernel calculates the extension ray and shadow ray, following
// an incoming ray on a simple diffuse material. Also updates pixel color.

#include "structs.h"
#include "random.cl"
#include "utils.cl"

__kernel void shade(
    __global uint *randomStates,
    __global Intersection *intersections,
    __global const float *mat_albedos,
    __global const float3 *mat_colors,
    __global Ray *extensionRays,
    __global Ray *shadowRays,
    __global float3 *pixelColors,
    __global float3 *debug)
{
    uint x = get_global_id(0);
    uint y = get_global_id(1);
    uint i = x + y * get_global_size(0);

    // ==> Calculate extension ray

    float3 bounceDirection = CosineSampleHemisphere(intersections[i].normal, &randomStates[i]);
    extensionRays[i].origin = intersections[i].hitPoint;
    extensionRays[i].direction = bounceDirection;

    // ==> Calculate shadow ray

    // TODO we have never implemented this before

    // ==> Calculate sampled color

    pixelColors[i] *= mat_albedos[i] * mat_colors[i]; // Every sample the new color is weighed in
    // TODO assumes that if the ray hits nothing (skybox), the following is applied (ambient lighting):
    // float a = .5f * (dirNormalized.Y + 1f);
    // pixel = (1f - a) * new Vector3(1f, 1f, 1f) + a * new Vector3(.5f, .7f, 1f );
}
