using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;
using Color = UnityEngine.Color;

public class Shadows
{
    private const string bufferName = "Shadow";
    private const int maxShadowedDirectionalLightCount = 4;

    private static int directionalShadowTexId = Shader.PropertyToID( "_DirectionalShadowTex" );
    private static int directionalShadowMatricesId = Shader.PropertyToID( "_DirectionalShadowMatrices" );

    private static Matrix4x4[] directionalShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];

    private CommandBuffer buffer = new CommandBuffer() { name = bufferName };
    private ScriptableRenderContext context;
    private CullingResults cullingResults;
    private ShadowSettings shadowSettings;
    private int shadowedDirectionalLightCount;

    private ShadowedDirectionalLight[] shadowedDirectionalLights =
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    public void Step( ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings )
    {
        shadowedDirectionalLightCount = 0;
        this.context = context;
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;
    }

    public void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer( buffer );
        buffer.Clear();
    }

    //储存可见光阴影数据
    public Vector2 ReserveDirectionalShadows( Light light, int visibleLightIndex )
    {
        if( shadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None &&
            light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds( visibleLightIndex, out Bounds b ) )
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount] = new ShadowedDirectionalLight()
                { visibleLightIndex = visibleLightIndex };
            var ret = new Vector2( light.shadowStrength, shadowedDirectionalLightCount );
            shadowedDirectionalLightCount++;
            return ret;
        }

        return Vector2.zero;
    }

    public void Render()
    {
        if( shadowedDirectionalLightCount > 0 )
        {
            RenderDirectionalShadows();
        }
        else
        {
            //不需要阴影时获得 1×1 虚拟纹理，避免额外的着色器变体
            buffer.GetTemporaryRT( directionalShadowTexId, 1, 1, 32, FilterMode.Bilinear,
                                   RenderTextureFormat.Shadowmap );
        }
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT( directionalShadowTexId );
        ExecuteBuffer();
    }

    //渲染所有平行光阴影
    void RenderDirectionalShadows()
    {
        int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int size = (int)shadowSettings.dirrectional.atalsSize / split;
        buffer.GetTemporaryRT( directionalShadowTexId, size, size, 32, FilterMode.Bilinear,
                               RenderTextureFormat.Shadowmap );
        //指定渲染数据储存在RT中
        buffer.SetRenderTarget( directionalShadowTexId, RenderBufferLoadAction.DontCare,
                                RenderBufferStoreAction.Store );
        //清除深度缓冲区
        buffer.ClearRenderTarget( true, false, Color.clear );
        buffer.BeginSample( bufferName );
        ExecuteBuffer();
        for( int i = 0; i < shadowedDirectionalLightCount; i++ )
        {
            RenderDirectionalShadows( i, size, split );
        }
        buffer.SetGlobalMatrixArray( directionalShadowMatricesId, directionalShadowMatrices );
        buffer.EndSample( bufferName );
        ExecuteBuffer();
    }

    //渲染平行光阴影
    void RenderDirectionalShadows( int index, int texSize, int split )
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings( cullingResults, light.visibleLightIndex );
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives( light.visibleLightIndex, 0, 1,
                                                                             Vector3.zero, texSize, 0f,
                                                                             out Matrix4x4 viewMatrix,
                                                                             out Matrix4x4 projectionMatrix,
                                                                             out ShadowSplitData splitData );
        shadowSettings.splitData = splitData;
        var offset = SetTileVieport( index, texSize, split );
        directionalShadowMatrices[index] = ConvertToShadowTexMatrix( projectionMatrix * viewMatrix, offset, split );
        buffer.SetViewProjectionMatrices( viewMatrix, projectionMatrix );
        ExecuteBuffer();
        //绘制阴影
        context.DrawShadows( ref shadowSettings );
    }

    Vector2 SetTileVieport( int index, float texSize, int split )
    {
        Vector2 offset = new Vector2( index % split, index / split );
        buffer.SetViewport( new Rect( offset.x * texSize, offset.y * texSize, texSize, texSize ) );
        return offset;
    }

    //世界空间转换为阴影贴图的矩阵
    Matrix4x4 ConvertToShadowTexMatrix( Matrix4x4 m, Vector2 offset, int split )
    {
        if( SystemInfo.usesReversedZBuffer )
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }
}
