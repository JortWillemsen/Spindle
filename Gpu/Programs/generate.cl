#include <structs.h>

__kernel void generate(__global Ray *rays)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    
    // Generate rays and writes them to the 'rays' buffer.
}