typedef struct
{
    int imageWidth;
    int imageHeight;
    int numSpheres;
    int numTriangles;
} SceneInfo;

typedef struct {
    float x;
    float y;
    float z;
} Vector3;

typedef struct
{
    Vector3 v1;
    Vector3 v2;
    Vector3 v3;
    Vector3 normal;
} Triangle;

typedef struct
{
    Vector3  direction;
    Vector3  origin;
} Ray;

typedef struct
{
    Vector3  position;
    float   radius;
} Sphere;

// Takes Ray, Sphere, returns the t distance
// bool intersectSphere(Ray *ray, Sphere *sphere, float *t)
// {
//     float t0, t1;
//     float radius2 = sphere->radius * sphere->radius;

//     // Geometric solution
//     float3 L = sphere->position - ray->origin;
//     float tca = dot(L, ray->direction);
//     float d2 = dot(L, L) - tca * tca;

//     if (d2 > radius2) return false;

//     float thc = sqrt(radius2 - d2);
//     t0 = tca - thc;
//     t1 = tca + thc;

//     if (t0 > t1)
//     {
//         float tmp = t0;
//         t0 = t1;
//         t1 = tmp;
//     }

//     if (t0 < 0)
//     {
//         t0 = t1;
//         if (t0 < 0) return false;
//     }

//     *t = t0;

//     return true;
// }

__kernel void trace(
    __global SceneInfo *info,
    __global const Ray *rays,
    __global const Sphere *spheres,
    __global const Triangle *triangles,
    __global float *result)
{
    uint x = get_global_id(0);
    uint y = get_global_id(1);

    result[y * info->imageWidth + x] = spheres[0].radius;

    //result[y * info->imageWidth + x] = 0xFFFFFF;

    // Ray r = rays[x * info->imageWidth + y];
    // bool hit = false;
    // float t = FLT_MAX;

    // for (int i = 0; i < info->numSpheres; ++i) {
    //     if (intersectSphere(&r, &spheres[i], &t)) {
    //         hit = true;
            
    //         result[y * info->imageWidth + x] = t;
    //         return;

    //         // Calculate the normal at the intersection point
    //         float3 hitPoint = r.origin + t * r.direction;
    //         float3 normal = normalize(hitPoint - spheres[i].position);
            
    //         // Convert normal to color (for simplicity, we use the normal as the color)
    //         int color = (int)((normal.x + 1.0f) * 255) << 16 |
    //                     (int)((normal.y + 1.0f) * 255) << 8 |
    //                     (int)((normal.z + 1.0f) * 255);
            
    //         result[y * info->imageWidth + x] = color;
    //         break;
    //     }
    // }
}