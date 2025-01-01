__constant int IMAGE_WIDTH = 600;
__constant int IMAGE_HEGHT = 400;

#include "sphere.cl"

__kernel void trace(
    __global struct Ray *origins,
    __global struct Sphere *spheres,
    __global float *result)
{
    int x = get_global_id(0);
    int y = get_global_id(1);


    result[x * IMAGE_WIDTH + y] = 1;
}