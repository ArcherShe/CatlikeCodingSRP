#ifndef CUSTOM_SURFACE_INCLUDED
#define CUSTOM_SURFACE_INCLUDED

struct Surface
{
    float3 normal;
    float3 vieDirection;
    half3 color;
    half alpha;
    float metallic;
    float smoothness;
};

#endif