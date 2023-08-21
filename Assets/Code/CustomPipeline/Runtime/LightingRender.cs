using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class Lighting
{
    private const int maxDirLightCount = 4;
    private const string bufferName = "Lighting";
    private static int dirLightCountId = Shader.PropertyToID( "_DirectionalLightCount" );
    private static int dirLightColorsId = Shader.PropertyToID( "_DirectionalLightColors" );
    private static int dirLightDirectionsId = Shader.PropertyToID( "_DirectionalLightDirections" );
    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightDirections  = new Vector4[maxDirLightCount];
    
    private CommandBuffer buffer = new CommandBuffer() { name = bufferName };
    private CullingResults cullingResults;
    private Shadows shadows = new Shadows();

    public void Steup( ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings )
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample( bufferName );
        shadows.Step(context, cullingResults, shadowSettings);
        SetupLights();
        shadows.Render();
        buffer.EndSample( bufferName );
        context.ExecuteCommandBuffer( buffer );
        buffer.Clear();
    }

    public void Cleanup()
    {
        shadows.Cleanup();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for( int i = 0; i < visibleLights.Length; i++ )
        {
            if( visibleLights[i].lightType == LightType.Directional )
            {
                VisibleLight visibleLight = visibleLights[i];
                SetupDirectionalLight( dirLightCount++, ref visibleLight );
                if(dirLightCount >= maxDirLightCount) break;
            }
        }
        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    void SetupDirectionalLight( int index, ref VisibleLight visibleLight )
    {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn( 2 );
        shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }
}