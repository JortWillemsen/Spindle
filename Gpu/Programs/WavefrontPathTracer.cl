typedef struct
{
    int imageWidth;
    int imageHeight;
    int numSpheres;
    int numTriangles;
} SceneInfo;

typedef struct
{
    float3 v1;
    float3 v2;
    float3 v3;
    float3 normal;
} Triangle;

typedef struct
{
    float3 direction;
    float3 origin;
} Ray;

typedef struct
{
    float3  position;
    float   radius;
} Sphere;


bool intersect_sphere(const Ray* ray, const Sphere* sphere, float* t)
{
 float3 rayToCenter = sphere->position - ray->origin;

 /* calculate coefficients a, b, c from quadratic equation */

 /* float a = dot(ray->dir, ray->dir); // ray direction is normalised, dotproduct simplifies to 1 */ 
 float b = dot(rayToCenter, ray->direction);
 float c = dot(rayToCenter, rayToCenter) - sphere->radius*sphere->radius;
 float disc = b * b - c; /* discriminant of quadratic formula */

 /* solve for t (distance to hitpoint along ray) */

 if (disc < 0.0f) return false;
 else *t = b - sqrt(disc);

 if (*t < 0.0f){
  *t = b + sqrt(disc);
  if (*t < 0.0f) return false; 
 }

 else return true;
}

__kernel void trace(
    __global SceneInfo *info,
    __global const Ray *rays,
    __global const Sphere *spheres,
    __global const Triangle *triangles,
    __global int *result)
{
    uint x = get_global_id(0);
    uint y = get_global_id(1);

    Ray r = rays[y * info->imageWidth + x];
    bool hit = false;
    float t = FLT_MAX;

    for (int i = 0; i < info->numSpheres; ++i) {
        Sphere s = spheres[i];
        
        if (intersect_sphere(&r, &s, &t)) {
            hit = true;

            result[y * info->imageWidth + x] = 1;

            // Calculate the normal at the intersection point
            float3 hitPoint = r.origin + t * r.direction;
            float3 normal = normalize(hitPoint - spheres[i].position);
            
            // Convert normal to color (for simplicity, we use the normal as the color)
            int color = (int)((normal.x) * 255) << 16 |
                        (int)((normal.y) * 255) << 8 |
                        (int)((normal.z) * 255);
            
            result[y * info->imageWidth + x] = color;
            break;
        }
    }

    
    if (!hit) result[y * info->imageWidth + x] = 0xFFF;
}