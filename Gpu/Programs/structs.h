typedef struct
{
    float3 direction;
    float3 origin;
    float3 accumulated_luminance;
    float3 latest_luminance_sample;
    float t;
    uint material_id;
    uint object_id;
} PathState;

typedef struct
{
    float3 camera_position;
    float3 frustum_top_left;
    float3 frustum_horizontal;
    float3 frustum_vertical;
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

typedef struct
{
    volatile uint new_ray_length;
    volatile uint extend_ray_length;
    volatile uint shade_length;
    volatile uint shadow_ray_length;
} QueueStates;
