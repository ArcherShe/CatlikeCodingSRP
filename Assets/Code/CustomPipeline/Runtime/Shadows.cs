using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;
using Color = UnityEngine.Color;

public class Shadows
{
    private const string bufferName = "Shadow";
    private const int maxShadowedDirectionalLightCount = 4, maxCascades = 4;

    private static int dirShadowAtlasId = Shader.PropertyToID( "_DirectionalShadowAtlas" );
    private static int directionalShadowMatricesId = Shader.PropertyToID( "_DirectionalShadowMatrices" );
    private static int cascadeCountId  = Shader.PropertyToID( "_CascadeCount" );
    private static int cascadeCullingSpheresId  = Shader.PropertyToID( "_CascadeCullingSpheres" );
    private static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
    private static Matrix4x4[] directionalShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];

    private CommandBuffer buffer = new CommandBuffer() { name = bufferName };
    private ScriptableRenderContext context;
    private CullingResults cullingResults;
    private ShadowSettings shadowSettings;
    private int shadowedDirectionalLightCount;

    private ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    public void Step( ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings )
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;
        shadowedDirectionalLightCount = 0;
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
            {
                visibleLightIndex = visibleLightIndex
            };
            return new Vector2( light.shadowStrength,
                                shadowSettings.dirrectional.cascadeCount * shadowedDirectionalLightCount++
                              );
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
            buffer.GetTemporaryRT( dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear,
                                   RenderTextureFormat.Shadowmap );
        }
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT( dirShadowAtlasId );
        ExecuteBuffer();
    }

    //渲染所有平行光阴影
    void RenderDirectionalShadows()
    {
        int atlasSize = (int)shadowSettings.dirrectional.atalsSize;
        buffer.GetTemporaryRT( dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear,
                               RenderTextureFormat.Shadowmap );
        //指定渲染数据储存在RT中
        buffer.SetRenderTarget(dirShadowAtlasId,
                               RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
                              );
        //清除深度缓冲区
        buffer.ClearRenderTarget( true, false, Color.clear );
        buffer.BeginSample( bufferName );
        ExecuteBuffer();

        int tiles = shadowedDirectionalLightCount * shadowSettings.dirrectional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int size = atlasSize / split;
        for( int i = 0; i < shadowedDirectionalLightCount; i++ )
        {
            RenderDirectionalShadows( i, size, split );
        }
        buffer.SetGlobalInt( cascadeCountId, shadowSettings.dirrectional.cascadeCount );
        buffer.SetGlobalVectorArray( cascadeCullingSpheresId, cascadeCullingSpheres );
        buffer.SetGlobalMatrixArray( directionalShadowMatricesId, directionalShadowMatrices );
        buffer.EndSample( bufferName );
        ExecuteBuffer();
    }

    //渲染平行光阴影
    void RenderDirectionalShadows( int index, int tileSize, int split )
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings( cullingResults, light.visibleLightIndex );
        var cascadeCount = this.shadowSettings.dirrectional.cascadeCount;
        var tileOffset = index * cascadeCount;
        var ratios = this.shadowSettings.dirrectional.CascadeRatios;
        for( int i = 0; i < cascadeCount; i++ )
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives( light.visibleLightIndex, i, cascadeCount,
                                                                                    ratios, tileSize, 0f,
                                                                                    out Matrix4x4 viewMatrix,
                                                                                    out Matrix4x4 projectionMatrix,
                                                                                    out ShadowSplitData splitData );
        
            shadowSettings.splitData = splitData;
            if( index == 0 )
            {
                var cullingSphere = splitData.cullingSphere;
                cullingSphere.w *= cullingSphere.w;
                cascadeCullingSpheres[i] = cullingSphere;
            }
            var tileIndex = tileOffset + i;
            directionalShadowMatrices[tileIndex] = ConvertToShadowTexMatrix( projectionMatrix * viewMatrix, 
                                                                             SetTileVieport( tileIndex, tileSize, split ), split );
            buffer.SetViewProjectionMatrices( viewMatrix, projectionMatrix );
            ExecuteBuffer();
            //绘制阴影
            context.DrawShadows( ref shadowSettings );
        }
    }

    Vector2 SetTileVieport( int index, float tileSize, int split )
    {
        Vector2 offset = new Vector2( index % split, index / split );
        buffer.SetViewport( new Rect( offset.x * tileSize, offset.y * tileSize, tileSize, tileSize ) );
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
