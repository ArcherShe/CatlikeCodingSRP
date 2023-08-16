Shader "CustomRP/UnlitTransparent"{
    Properties{
        _BaseMap("Texture", 2D) = "White" {}
        _BaseColor("Color", Color) = (1.0,1.0,1.0,1.0)
        _Cutoff("Alpha Cutoff", range(0, 1)) = 0.5
        [Toggle(_CLIPPING)]_Clipping("Alpah Clipping", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)]_ZWrite("Z Write", Float) = 1
    }
    SubShader{
        Pass{
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "ShaderLibrary/UnlitTransparentPass.hlsl"
            ENDHLSL
        }
    }
    
}
