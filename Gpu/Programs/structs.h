typedef struct
{
    float3 direction;
    float3 origin;
} Ray;

typedef struct
{
    int numSpheres;
    int numTriangles;
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

typedef struct 
{
    bool hit;
    float3 hitPoint;
    float3 normal;
    float t;
    uint material;
} Intersection;

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
