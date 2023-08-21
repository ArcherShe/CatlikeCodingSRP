using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private const string bufferName = "Shadow";
    private const int maxShadowedDirectionalLightCount = 1;

    private static int directionalShadowTexId = Shader.PropertyToID("_DirectionalShadowTex");

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

    public void Step(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        shadowedDirectionalLightCount = 0;
        this.context = context;
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;
        // buffer.BeginSample(bufferName);
        //
        // buffer.EndSample(bufferName);
    }

    public void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    //储存可见光阴影数据
    public void ReserveDirectionalShadows(Light light, int index)
    {

    }

    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            buffer.GetTemporaryRT(directionalShadowTexId, 1, 1, 32, FilterMode.Bilinear,
                RenderTextureFormat.Shadowmap);
        }
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(directionalShadowTexId);
        ExecuteBuffer();
    }
    void RenderDirectionalShadows()
    {
        int size = (int)shadowSettings.dirrectional.atalsSize;
        buffer.GetTemporaryRT(directionalShadowTexId, size, size, 32, FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap);
        //指定渲染数据储存在RT中
        buffer.SetRenderTarget(directionalShadowTexId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        //清除深度缓冲区
        buffer.ClearRenderTarget(true, false, Color.clear);
    }
}
