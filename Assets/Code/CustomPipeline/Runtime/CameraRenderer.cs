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

    public void Render( ScriptableRenderContext context, Camera camera )
    {
        this.camera = camera;
        this.context = context;
        #if UNITY_EDITOR
        PrepareBuffer();
        PrepareForSceneWindow();
        #endif
        if (!Cull()) return;
        
        Steup();
        DrawVisibleGeometry();
        #if UNITY_EDITOR
        DrawUnsupportedShaders();
        DrawGizmos();
        #endif
        Submit();
    }

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

    void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings( camera )
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        context.DrawRenderers( cullingResults, ref drawingSettings, ref filteringSettings );
        context.DrawSkybox( camera );

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers( cullingResults, ref drawingSettings, ref filteringSettings );
    }
    
    void Submit()
    {
        buffer.EndSample( bufferName );
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer( buffer );
        buffer.Clear();
    }

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