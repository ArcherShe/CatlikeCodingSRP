Shader "CustomRP/Lit"{
    
    Properties{
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _Metallic("Metallic", range(0.0, 1.0)) = 0
        _Smoothness("Smoothness", range(0.0, 1.0)) = 0.5
        _Cutoff("Alpha Cutoff", range(0.0, 1.0)) = 1
        [Toggle(_CLIPPING)]_Clipping("Alpha Clipping", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)]_ZWrite("Z Write", Float) = 1
        [Toggle(_PREMULTIPLY_ALPHA)]_PREMULTIPLY_ALPHA("Premultiply Alpah", Float)= 1
    }
    
    CustomEditor "CustomShaderGUI"
    SubShader{
        Pass{
            Tags { "LightMode" = "CustomLit" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma shader_feature _PREMULTIPLY_ALPHA
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "Pass//LitPass.hlsl"
            ENDHLSL
        }
    }
    
}
