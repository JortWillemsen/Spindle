// This kernel calculates the extension ray and shadow ray, following
// an incoming ray on a simple diffuse material. Also updates pixel color.

#include "structs.h"
#include "random.cl"
#include "utils.cl"

__kernel void shade( // TODO: currently just renders diffuse materials
    __global const Material *materials, // TODO: we could declare this as a __constant buffer, potentially optimizing caching
    __global uint *randomStates,
    __global Intersection *intersections,
    __global Ray *extensionRays,
    __global Ray *shadowRays,
    __global float3 *pixelColors,
    __global float3 *debug)
{
    uint x = get_global_id(0);
    uint y = get_global_id(1);
    uint i = x + y * get_global_size(0);

    Intersection intersection = intersections[i];
    Material mat = materials[intersection.material]; // Is always diffuse in this kernel
    if (mat.type != mat_diffuse) return; // TODO this is temporary

    // ==> Calculate extension ray

    float3 bounceDirection = CosineSampleHemisphere(intersection.normal, &randomStates[i]);
    extensionRays[i].origin = intersection.hitPoint;
    extensionRays[i].direction = bounceDirection;

    // ==> Calculate shadow ray

    // TODO we have never implemented this before

    // ==> Calculate sampled color

    pixelColors[i] += mat.albedo * mat.color; // Every sample the new color is weighed in
    // TODO assumes that if the ray hits nothing (skybox), the following is applied (ambient lighting):
    // float a = .5f * (dirNormalized.Y + 1f);
    // pixel = (1f - a) * new Vector3(1f, 1f, 1f) + a * new Vector3(.5f, .7f, 1f );
}
