#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
    #include "Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);
    
    // GPU INSTANCE
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
        UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
    struct a2v
    {
        float3 positionOS : POSITION;
        float2 baseUV : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : VAR_BASE_UV;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    v2f vert(a2v input) //: SV_POSITION
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, o);
        float3 worldPos = TransformObjectToWorld(input.positionOS);
        o.pos = TransformWorldToHClip(worldPos);
        float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
        o.uv = input.baseUV * baseST.xy + baseST.zw;
        return o;
    }
    
    float4 frag(v2f i) : SV_TARGET
    {
        UNITY_SETUP_INSTANCE_ID(i);
        float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
        float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
        float4 base = baseMap * baseColor;
        #if defined(_CLIPPING)
            clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
        #endif
        return base;
    }
#endif