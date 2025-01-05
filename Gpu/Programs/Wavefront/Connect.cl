__kernel void connect(
  __global Ray *shadowRays,
  __global SceneInfo *sceneInfo,
  __global Sphere *spheres,
  __global Triangle *triangles,
  __global Intersection *intersections)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    
    // Calculates shadow ray intersections.
    // Returns the first intersection we find, not the nearest.
}