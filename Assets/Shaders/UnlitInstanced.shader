Shader "CustomRP/UnlitInstances"{
    Properties{
        _BaseColor("Color", Color) = (1.0,1.0,1.0,1.0)
    }
    SubShader{
        Pass{
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "Pass//UnlitInstancedPass.hlsl"
            ENDHLSL
        }
    }
    
}
