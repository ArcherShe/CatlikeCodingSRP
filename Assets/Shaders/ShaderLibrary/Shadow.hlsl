#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "Surface.hlsl"

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
CBUFFER_END

struct DirectionalShadowData
{
    float strength;
    int tileIndex;
};

struct ShadowData
{
    int cascadeIndex;
};

ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData data;
    int i;
    for (i = 0; i < _CascadeCount; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
        if (distanceSqr < sphere.w) {
            break;
        }
    }
    data.cascadeIndex = i;
    return data;
}

//阴影贴图采样
float SampleDirectionalShadowTex(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

//阴影衰减
float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surfaceWS)
{
    if(data.strength <= 0) return 0;
    float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex],
        float4(surfaceWS.position, 1.0)).xyz;
    float shadow = SampleDirectionalShadowTex(positionSTS);
    return lerp(1.0, shadow, data.strength);
}

#endif