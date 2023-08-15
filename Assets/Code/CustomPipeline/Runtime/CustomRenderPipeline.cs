using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer renderer = new CameraRenderer();

    public CustomRenderPipeline()
    {
        // 启用批处理
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }
    protected override void Render( ScriptableRenderContext context, Camera[] cameras )
    {
        for( int i = 0; i < cameras.Length; i++ )
        {
            renderer.Render( context, cameras[i] );
        }
    }
}