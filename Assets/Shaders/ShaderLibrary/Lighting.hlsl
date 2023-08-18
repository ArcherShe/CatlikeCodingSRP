#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
#include "Surface.hlsl"

struct Light
{
    float3 color;
    float3 direction;
};

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
};

float Square(float v)
{
    return  v * v;
}

float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    float3 halfDir = SafeNormalize(light.direction + surface.vieDirection);
    float nh2 = Square(saturate(dot(surface.normal, halfDir)));
    float lh2 = Square(saturate(dot(light.direction, halfDir)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2/(d2 * max(1.0, lh2) * normalization);
}

float3 DirectBDRF(Surface surface, BRDF brdf, Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction))* light.color;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light)
{
    return IncomingLight(surface, light) * DirectBDRF(surface, brdf, light);
}
#endif