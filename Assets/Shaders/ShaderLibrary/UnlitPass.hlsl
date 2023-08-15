#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
    #include "Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
    CBUFFER_END
            
    float4 vert(float3 positionOS : POSITION) : SV_POSITION
    {
        float3 worldPos = TransformObjectToWorld(positionOS);
        return TransformWorldToHClip(worldPos);
    }
    
    float4 frag() : SV_TARGET
    {
        return _BaseColor;
    }
#endif