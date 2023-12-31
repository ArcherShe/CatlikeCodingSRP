﻿using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    bool useDynamicBatching, useGPUInstancing; 
    private CameraRenderer renderer = new CameraRenderer();
    private ShadowSettings shadowSettings;
    
    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;
        // 启用批处理(启用动态批处理需关闭此项)
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        
        //设置颜色空间为线性
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    protected override void Render( ScriptableRenderContext context, Camera[] cameras )
    {
        for( int i = 0; i < cameras.Length; i++ )
        {
            renderer.Render( context, cameras[i], useDynamicBatching, useGPUInstancing, shadowSettings);
        }
    }
}