using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor editor;
    private Object[] materials;
    private MaterialProperty[] properties;

    private bool showPresets;

    public override void OnGUI( MaterialEditor materialEditor, MaterialProperty[] properties )
    {
        base.OnGUI( materialEditor, properties );
        editor = materialEditor;
        this.properties = properties;
        materials = materialEditor.targets;

        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout( showPresets, "Presets", true );
        if( HasPremultiplyAlpha && showPresets )
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }

    bool HasProperty( string name ) => FindProperty( name, properties, false ) != null;

    private bool HasPremultiplyAlpha => HasProperty( "_PremultiplyAlpha" );

    bool SetProperty( string name, float value )
    {
        MaterialProperty property = FindProperty( name, properties );
        if( property != null )
        {
            FindProperty( name, properties ).floatValue = value;
            return true;
        }

        return false;
    }

    void SetProperty( string name, string keyword, bool value )
    {
        if( SetProperty( name, value ? 1f : 0f ) )
        {
            SetKeyword( keyword, value );
        }
    }

    void SetKeyword( string keyword, bool enable )
    {
        if( enable )
        {
            foreach( Material mat in materials )
            {
                mat.EnableKeyword( keyword );
            }
        }
        else
        {
            foreach( Material mat in materials )
            {
                mat.DisableKeyword( keyword );
            }
        }
    }

    bool Clipping
    {
        set => SetProperty( "_Clipping", "_CLIPPING", value );
    }

    bool PremultiplyAlpha
    {
        set => SetProperty( "_PremultiplyAlpha", "_PREMULTIPLY_ALPHA", value );
    }

    BlendMode SrcBlend
    {
        set => SetProperty( "_SrcBlend", (float)value );
    }

    BlendMode DstBlend
    {
        set => SetProperty( "_DstBlend", (float)value );
    }

    bool ZWrite
    {
        set => SetProperty( "_ZWrite", value ? 1f : 0f );
    }

    RenderQueue RenderQueue
    {
        set
        {
            foreach( Material mat in materials )
            {
                mat.renderQueue = (int)value;
            }
        }
    }

    bool PresetButton( string name )
    {
        if( GUILayout.Button( name ) )
        {
            editor.RegisterPropertyChangeUndo( name );
            return true;
        }

        return false;
    }

    void OpaquePreset()
    {
        if( PresetButton( "Opaque" ) )
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    void ClipPreset()
    {
        if( PresetButton( "Clip" ) )
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    void FadePreset()
    {
        if( PresetButton( "Fade" ) )
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    void TransparentPreset()
    {
        if( PresetButton( "Transparent" ) )
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
}