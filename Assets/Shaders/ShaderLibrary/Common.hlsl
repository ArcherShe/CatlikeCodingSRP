#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED
    #include "UnityInput.hlsl"

    #define UNITY_MATRIX_M unity_ObjectToWorld
    #define UNITY_MATRIX_I_M unity_WorldToObject
    #define UNITY_MATRIX_V unity_MatrixV
    #define UNITY_MATRIX_I_V unity_MatrixInvV
    #define UNITY_MATRIX_VP unity_MatrixVP
    #define UNITY_PREV_MATRIX_M unity_prev_MatrixM
    #define UNITY_PREV_MATRIX_I_M unity_prev_MatrixIM
    #define UNITY_MATRIX_P glstate_matrix_projection

    // float3 TransformObjectToWorld(float3 positionOS)
    // {
    //     return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
    // }
    //     
    // float4 TransformWorldToHClip(float3 positionOS)
    // {
    //     return mul(unity_MatrixVP, float4(positionOS, 1.0));
    // }
#endif