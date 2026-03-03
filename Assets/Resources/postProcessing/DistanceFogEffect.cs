using System;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess( typeof(DistanceFogEffectRenderer) , PostProcessEvent.BeforeStack , "Custom/DistanceFog" )]
public sealed class DistanceFogEffect : PostProcessEffectSettings
{
    [Header( "Distance Fog" )]
    public FloatParameter FogStart = new() { value = 1f };

    public FloatParameter FogEnd           = new() { value = 100f };
    public FloatParameter FogAmount        = new() { value = 1f };
    public ColorParameter FogStartColor    = new() { value = Color.black };
    public ColorParameter FogEndColor      = new() { value = Color.white };
    public FloatParameter SkyboxImportance = new() { value = 1f };

    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported( PostProcessRenderContext context )
    {
        return enabled.value
               && FogAmount.value > 0f;
    }
}

public sealed class DistanceFogEffectRenderer : PostProcessEffectRenderer<DistanceFogEffect>
{
    public Camera camera;

    public override void Render( PostProcessRenderContext context )
    {


        var sheet = context.propertySheets.Get( Shader.Find( "PostProcessing/DistanceFog" ) );
        var projectionInverse = GL.GetGPUProjectionMatrix( context.camera.projectionMatrix , false ).inverse;
        //Matrix4x4 projectionInverse = context.camera.projectionMatrix.inverse;
        var viewInverse = context.camera.cameraToWorldMatrix;

        var inverseViewProjection = projectionInverse * viewInverse;


        sheet.properties.SetMatrix( "_InverseProjection" , projectionInverse );
        sheet.properties.SetMatrix( "_InverseView" , viewInverse );
        sheet.properties.SetMatrix( "_InverseViewProjection" , inverseViewProjection );


        sheet.properties.SetFloat( "_FogStart" , settings.FogStart.value );
        sheet.properties.SetFloat( "_FogEnd" , settings.FogEnd.value );
        sheet.properties.SetFloat( "_FogAmount" , settings.FogAmount.value );
        sheet.properties.SetColor( "_FogStartColor" , settings.FogStartColor );
        sheet.properties.SetColor( "_FogEndColor" , settings.FogEndColor );
        sheet.properties.SetFloat( "_SkyboxImportance" , settings.SkyboxImportance );

        context.command.BlitFullscreenTriangle( context.source , context.destination , sheet , 0 );
    }
}