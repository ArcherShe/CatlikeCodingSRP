#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
    #include "Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
    CBUFFER_END
    
    struct a2v
    {
        float3 positionOS : POSITION;
    };
    
    float4 vert(a2v input) : SV_POSITION
    {
        float3 worldPos = TransformObjectToWorld(input.positionOS);
        return TransformWorldToHClip(worldPos);
    }
        
    float4 frag() : SV_TARGET
    {
        return _BaseColor;
    }
#endif