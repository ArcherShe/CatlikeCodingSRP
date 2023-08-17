#ifndef CUSTOM_INSTANCING_PASS_INCLUDED
#define CUSTOM_INSTANCING_PASS_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

// GPU INSTANCE
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
struct a2v
{
    float3 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 pos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f vert(a2v input) //: SV_POSITION
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, o);
    float3 worldPos = TransformObjectToWorld(input.positionOS);
    o.pos = TransformWorldToHClip(worldPos);
    return o;
}

float4 frag(v2f i) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(i);
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
}
#endif