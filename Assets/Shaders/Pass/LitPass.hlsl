#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"
#include "../ShaderLibrary/UnityInput.hlsl"
#include "../ShaderLibrary/Shadow.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// 最小反射系数
#define MIN_REFLECTIVITY 0.04

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

// GPU INSTANCE
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
    // UNITY_DEFINE_INSTANCED_PROP(float, _PremultiplyAlpha)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct a2v
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV  : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : VAR_POSITION;
    float3 normalWS : VAR_NORMAL;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

DirectionalShadowData GetDirectionShadowData(int lightIndex, ShadowData shadowData)
{
    DirectionalShadowData d;
    d.strength = _DirectionalLightShadowData[lightIndex].x;
    d.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
    return d;
}

Light GetDirectionalLight(int index, Surface surfaceWS, ShadowData shadowData)
{
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;
    DirectionalShadowData dirShadowData  = GetDirectionShadowData(index, shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(dirShadowData , surfaceWS);
    light.attenuation = shadowData.cascadeIndex * 0.25;
    return light;
}

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

float3 GetLighting(Surface surfaceWS, BRDF brdf)
{
    ShadowData shadowData = GetShadowData(surfaceWS);
    float3 color = 0.0;
    for(int i = 0; i < GetDirectionalLightCount(); i++)
    {
        color += GetLighting(surfaceWS, brdf, GetDirectionalLight(i, surfaceWS, shadowData));
    }
    return color;
}

float OneMinusReflectivity(float metallic)
{
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

BRDF GetBRDF(Surface surface, bool applyAlphaToDiffuse = false)
{
    BRDF brdf;
    float oneMinuseReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinuseReflectivity;
    if( applyAlphaToDiffuse )
    {
        brdf.diffuse *= surface.alpha;
    }
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}

v2f vert(a2v input)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(input)
    UNITY_TRANSFER_INSTANCE_ID(input, o);
    o.positionWS = TransformObjectToWorld(input.positionOS);
    o.positionCS = TransformWorldToHClip(o.positionWS);
    o.normalWS = TransformObjectToWorldNormal(input.normalOS);
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    o.baseUV = input.baseUV.xy * baseST.xy + baseST.zw;
    return o;
}
    
float4 frag(v2f input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input)
    Surface surface;
    surface.position = input.positionWS;
    surface.normal = normalize(input.normalWS);
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base = baseMap * baseColor;
    #if defined(_CLIPPING)
    clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif
    surface.vieDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Metallic);
    surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Smoothness);
    #if defined(_PREMULTIPLY_ALPHA)
        BRDF brdf = GetBRDF(surface, true);
    #else
        BRDF brdf = GetBRDF(surface);
    #endif
    float3 color = GetLighting(surface, brdf);
    return float4(color, surface.alpha);
}
#endif