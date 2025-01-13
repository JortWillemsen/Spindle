typedef struct
{
    float3 direction;
    float3 origin;
    float t;
    uint object_id;
} Ray;

typedef struct
{
    int num_spheres;
    int num_triangles;
} SceneInfo;

typedef struct
{
    float3 position;
    float radius;
    uint material;
} Sphere;

typedef struct
{
    float3 v1;
    float3 v2;
    float3 v3;
    uint material;
} Triangle;

// typedef struct
// {
//     bool hit; // TODO: obsolete, if material != 0 it is a hit
//     float3 hitPoint; // TODO: can be determined by t
//     float3 normal; // TODO: when desired, can be calcualted by object anyway
//     float t;
//     uint material;
// } Intersection;

enum MaterialType {
    mat_diffuse = 1u,
    mat_reflective = 2u,
};

typedef struct
{
    float3 color;
    float albedo;
    enum MaterialType type;
} Material;
