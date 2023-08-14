using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Profiling;
using UnityEngine.Profiling;

partial class CameraRenderer
{
    #if UNITY_EDITOR
    private static Material errorMaterial;
    private static ShaderTagId[] legcyShaderIds =
    {
        new ShaderTagId( "Always" ),
        new ShaderTagId( "ForwardBase" ),
        new ShaderTagId( "PrepassBase" ),
        new ShaderTagId( "Vertex" ),
        new ShaderTagId( "VertexLMRGBM" ),
        new ShaderTagId( "VertexLM" ),
    };

    private string SampleName{ get; set; }

    void DrawUnsupportedShaders ()
    {
        if( errorMaterial == null )
        {
            errorMaterial = new Material( Shader.Find( "Hidden/InternalErrorShader" ) );
        }
        var drawingSettings = new DrawingSettings( legcyShaderIds[0], new SortingSettings( camera ) )
        {
            overrideMaterial = errorMaterial,
        };
        for( int i = 1; i < legcyShaderIds.Length; i++ )
        {
            drawingSettings.SetShaderPassName( i, legcyShaderIds[i] );
        }
        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers( cullingResults, ref drawingSettings, ref filteringSettings );
    }

    void DrawGizmos()
    {
        if( Handles.ShouldRenderGizmos() )
        {
            context.DrawGizmos( camera, GizmoSubset.PreImageEffects );
            context.DrawGizmos( camera, GizmoSubset.PostImageEffects );
        }
    }

    void PrepareForSceneWindow()
    {
        if( camera.cameraType == CameraType.SceneView )
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView( camera );
        }
    }

    void PrepareBuffer()
    {
        // Profiler.BeginSample( "Editor Only" );
        buffer.name = SampleName = camera.name;
        // Profiler.EndSample();
    }
    #else
        const string SampleName = bufferName;
    #endif
}