using System;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess( typeof(HeatWaveEffectRenderer) , PostProcessEvent.AfterStack , "Custom/HeatWave Effect" )]
public sealed class HeatWaveEffect : PostProcessEffectSettings
{
    public FloatParameter HeatWaveStart = new() { value = 1f };

    public FloatParameter HeatWaveEnd = new() { value = 100f };

    //   public FloatParameter FogAmount = new() { value = 1f };

    //public ColorParameter HeatWaveStartColor = new() { value = Color.black };
    //public ColorParameter HeatWaveEndColor   = new() { value = Color.white };
    public FloatParameter WaveSize      = new() { value = 1f };
    public FloatParameter WaveSpeed     = new() { value = 1f };
    public FloatParameter WaveIntensity = new() { value = 1f };

    public FloatParameter HeatWaveAberration = new() { value = .3f };

    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported( PostProcessRenderContext context )
    {
        return enabled.value
               && WaveIntensity.value > 0f;
    }
}

public sealed class HeatWaveEffectRenderer : PostProcessEffectRenderer<HeatWaveEffect>
{
    public Camera camera;

    public override void Render( PostProcessRenderContext context )
    {


        var sheet = context.propertySheets.Get( Shader.Find( "PostProcessing/HeatWaveEffect" ) );
        var projectionInverse = GL.GetGPUProjectionMatrix( context.camera.projectionMatrix , false ).inverse;
        //Matrix4x4 projectionInverse = context.camera.projectionMatrix.inverse;
        var viewInverse = context.camera.cameraToWorldMatrix;

        var inverseViewProjection = projectionInverse * viewInverse;


        sheet.properties.SetMatrix( "_InverseProjection" , projectionInverse );
        sheet.properties.SetMatrix( "_InverseView" , viewInverse );
        sheet.properties.SetMatrix( "_InverseViewProjection" , inverseViewProjection );


        sheet.properties.SetFloat( "_HeatWaveStart" , settings.HeatWaveStart.value );
        sheet.properties.SetFloat( "_HeatWaveEnd" , settings.HeatWaveEnd.value );
        sheet.properties.SetFloat( "_WaveSize" , settings.WaveSize.value );
        sheet.properties.SetFloat( "_WaveSpeed" , settings.WaveSpeed.value );
        sheet.properties.SetFloat( "_WaveIntensity" , settings.WaveIntensity.value );
        sheet.properties.SetFloat( "_HeatWaveAberration" , settings.HeatWaveAberration );
        

        context.command.BlitFullscreenTriangle( context.source , context.destination , sheet , 0 );
    }
}