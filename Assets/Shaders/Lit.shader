Shader "CustomRP/Lit"{
    Properties{
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
    }
    SubShader{
        Pass{
            Tags { "LightMode" = "CustomLit" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Pass//LitPass.hlsl"
            ENDHLSL
        }
    }
    
}
