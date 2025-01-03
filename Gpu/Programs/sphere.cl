#include "ray.cl"

typedef struct
{
    float3  position;
    float   radius;
} Sphere;

// Takes Ray, Sphere, returns the t distance
inline bool intersectSphere(Ray *ray, Sphere *sphere, float *t)
{
    float t0, t1;
    float radius2 = sphere->radius * sphere->radius;

    // Geometric solution
    float3 L = sphere->position - ray->origin;
    float tca = dot(L, ray->direction);
    float d2 = dot(L, L) - tca * tca;
    if (d2 > radius2) return false;
    float thc = sqrt(radius2 - d2);
    t0 = tca - thc;
    t1 = tca + thc;

    if (t0 > t1)
    {
        float tmp = t0;
        t0 = t1;
        t1 = tmp;
    }

    if (t0 < 0)
    {
        t0 = t1;
        if (t0 < 0) return false;
    }

    *t = t0;

    return true;
}