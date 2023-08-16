using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private const string bufferName = "Render Camera";
    private static ShaderTagId unlitShaderTagId = new ShaderTagId( "SRPDefaultUnlit" );
    
    private Camera camera;
    private ScriptableRenderContext context;
    private CullingResults cullingResults;
    private CommandBuffer buffer = new CommandBuffer() { name = bufferName };

    public void Render( ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
    {
        this.camera = camera;
        this.context = context;
        #if UNITY_EDITOR
        PrepareBuffer();
        PrepareForSceneWindow();
        #endif
        if (!Cull()) return;
        
        Steup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        #if UNITY_EDITOR
        DrawUnsupportedShaders();
        DrawGizmos();
        #endif
        Submit();
    }

    /// <summary>
    /// 启动渲染管线
    /// </summary>
    void Steup()
    {
        context.SetupCameraProperties( camera );
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
                                 flags <= CameraClearFlags.Depth,
                                 flags == CameraClearFlags.Color,
                                 flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear );
        buffer.BeginSample( bufferName );
        ExecuteBuffer();
    }

    /// <summary>
    /// 绘制多边形
    /// </summary>
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings( camera )
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            // 动态批处理开启需要关闭GPUIntancing
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        context.DrawRenderers( cullingResults, ref drawingSettings, ref filteringSettings );
        context.DrawSkybox( camera );

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers( cullingResults, ref drawingSettings, ref filteringSettings );
    }
    
    /// <summary>
    /// 提交渲染
    /// </summary>
    void Submit()
    {
        buffer.EndSample( bufferName );
        ExecuteBuffer();
        context.Submit();
    }

    /// <summary>
    /// buffer 执行
    /// </summary>
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer( buffer );
        buffer.Clear();
    }

    /// <summary>
    /// 剔除
    /// </summary>
    /// <returns></returns>
    bool Cull()
    {
        if( camera.TryGetCullingParameters( out ScriptableCullingParameters p ) )
        {
            cullingResults = context.Cull( ref p );
            return true;
        }
        return false;
    }
}