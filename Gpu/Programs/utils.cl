#include "structs.h"

// Based on lecture 8: variance reduction pt. 2
float3 CosineSampleHemisphere(float3 normal, uint* randomState)
{
    float r0 = RandomFloat(randomState), r1 = RandomFloat(randomState);
    float r = sqrt(r0);
    float theta = 2 * M_PI * r1;
    float x = r * cos(theta);
    float y = r * sin(theta);
    float z = sqrt(1 - r0);
    float3 randomVector = (float3)(x, y, z);

    // Align the random vector with the given normal vector
    // (The part below has been generated by ChatGPT 4o mini, but discussed with teacher :))

    float3 tangent1, tangent2;

    // Create an orthogonal basis with the normal
    if (fabs(normal.x) > fabs(normal.y)) {
        tangent1 = normalize(cross((float3)(0, 1, 0), normal)); // use a world axis if normal is along x or z
        // TODO: use fast_normalize everywhere?
    }
    else {
        tangent1 = normalize(cross((float3)(1, 0, 0), normal));
    }
    tangent2 = cross(normal, tangent1);

    // Now apply the transformation so that the original sample is aligned with he normal
    return randomVector.x * tangent1 + randomVector.y * tangent2 + randomVector.z * normal;
}

float3 reflect(float3 incoming, float3 normal)
{
    return incoming - 2.0f * dot(normal, incoming) * normal;
}

// Returns closest intersection, preferably in positive direction (front)
float IntersectSphere(PathState ray, Sphere sphere)
{
    float3 oc = sphere.position - (ray.origin + ray.direction * 0.0005f); // Prevent shadow acne
    // Hours wasted on shadow acne: 4

    float h = dot(ray.direction, oc);
    float c = powr(length(oc), 2) - sphere.radius * sphere.radius;

    // Solve quadratic formula to determine hit
    float discriminant = h * h - c;
    if (discriminant < 0) return -1; // No hit

    float rootedDiscriminant = sqrt(discriminant);
    float t1 = (h - rootedDiscriminant);
    float t2 = (h + rootedDiscriminant);
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
