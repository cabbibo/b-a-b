using System;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess( typeof(LensFlareEffectRenderer) , PostProcessEvent.BeforeStack , "Custom/LensFlare Effect" )]
public sealed class LensFlareEffect : PostProcessEffectSettings
{
    public FloatParameter LensFlareIntensity = new() { value = 1f };

    public FloatParameter LensFlareSpacing = new() { value = 100f };
    public FloatParameter SunDogIntensity  = new() { value = 100f };
    public FloatParameter SunDogSpacing    = new() { value = 100f };

    public FloatParameter GhostMultiplier = new() { value = 1f };
    public FloatParameter FlareMultiplier = new() { value = 1f };
    public FloatParameter HexMultiplier   = new() { value = 1f };
    public FloatParameter HaloMultiplier  = new() { value = 1f };

    public TextureParameter HexTexture = new();

    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported( PostProcessRenderContext context )
    {
        return enabled.value
               && LensFlareIntensity.value > 0f;
    }
}

public sealed class LensFlareEffectRenderer : PostProcessEffectRenderer<LensFlareEffect>
{
    public Camera camera;

    public override void Render( PostProcessRenderContext context )
    {


        var sheet = context.propertySheets.Get( Shader.Find( "PostProcessing/LensFlareEffect" ) );
        var projectionInverse = GL.GetGPUProjectionMatrix( context.camera.projectionMatrix , false ).inverse;
        //Matrix4x4 projectionInverse = context.camera.projectionMatrix.inverse;
        var viewInverse = context.camera.cameraToWorldMatrix;

        var inverseViewProjection = projectionInverse * viewInverse;


        sheet.properties.SetMatrix( "_InverseProjection" , projectionInverse );
        sheet.properties.SetMatrix( "_InverseView" , viewInverse );
        sheet.properties.SetMatrix( "_InverseViewProjection" , inverseViewProjection );


        sheet.properties.SetFloat( "_Intensity" , settings.LensFlareIntensity.value );
        sheet.properties.SetFloat( "_Spacing" , settings.LensFlareSpacing.value );
        sheet.properties.SetFloat( "_SunDogIntensity" , settings.SunDogIntensity.value );
        sheet.properties.SetFloat( "_SunDogSpacing" , settings.SunDogIntensity.value );
        sheet.properties.SetTexture( "_HexTexture" , settings.HexTexture );
        sheet.properties.SetFloat( "_GhostMultiplier" , settings.GhostMultiplier.value );
        sheet.properties.SetFloat( "_FlareMultiplier" , settings.FlareMultiplier.value );
        sheet.properties.SetFloat( "_HexMultiplier" , settings.HexMultiplier.value );
        sheet.properties.SetFloat( "_HaloMultiplier" , settings.HaloMultiplier.value );


        context.command.BlitFullscreenTriangle( context.source , context.destination , sheet , 0 );
    }
}