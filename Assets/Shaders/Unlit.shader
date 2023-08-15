Shader "CustomRP/Unlit"{
    Properties{
        _BaseColor("Color", Color) = (1.0,1.0,1.0,1.0)
    }
    SubShader{
        Pass{
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "ShaderLibrary/UnlitPass.hlsl"
            ENDHLSL
        }
    }
    
}
