#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#include "Common.hlsl"
#include "Surface.hlsl"

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowTex);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct DirectionalShadowData
{
    float strength;
    int texIndex;
};

//阴影贴图采样
float SampleDirectionalShadowTex(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowTex, SHADOW_SAMPLER, positionSTS);
}

//阴影衰减
float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surfaceWS)
{
    if(data.strength <= 0) return 0;
    float3 positionSTS = mul(_DirectionalShadowMatrices[data.texIndex], float4(surfaceWS.position, 1.0)).xyz;
    float shadow = SampleDirectionalShadowTex(positionSTS);
    return lerp(0.0, shadow, data.strength);
}

#endif