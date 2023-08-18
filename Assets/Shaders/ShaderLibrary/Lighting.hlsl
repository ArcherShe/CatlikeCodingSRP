#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
#include "Surface.hlsl"

struct Light
{
    float3 color;
    float3 direction;
};

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction))* light.color;
}

float3 GetLighting(Surface surface, Light light)
{
    return IncomingLight(surface, light);
}

#endif