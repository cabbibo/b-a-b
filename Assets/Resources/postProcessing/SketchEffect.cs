using System;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess( typeof(SketchPostRenderer) , PostProcessEvent.AfterStack , "Custom/SketchEffect" )]
public sealed class SketchEffect : PostProcessEffectSettings
{
    [Range( 0f , 1f )]
    [Tooltip( "size" )]
    public FloatParameter intensity = new() { value = 0.5f };

    [Range( 0f , 1f )]
    public FloatParameter scale = new() { value = 0.5f };

    [Range( 0f , 1f )]
    public FloatParameter changeSpeed = new() { value = 0.1f };

    [Tooltip( "The height map" )]
    public TextureParameter heightMap = new() { value = null };


    [Tooltip( "The height map" )]
    public TextureParameter paintMap = new() { value = null };

    [Tooltip( "The size of the map" )]
    public Vector3Parameter mapSize = new() { value = Vector3.zero };


    [Tooltip( "The offset of the map" )]
    public Vector3Parameter mapOffset = new() { value = Vector3.zero };

    [Tooltip( "The inverse projection matrix" )]
    /* public Vector4Parameter inverseProjection1 = new() { value = Vector4.zero };

     public Vector4Parameter inverseProjection2 = new() { value = Vector4.zero };
     public Vector4Parameter inverseProjection3 = new() { value = Vector4.zero };
     public Vector4Parameter inverseProjection4 = new() { value = Vector4.zero };*/
    public FloatParameter _NoiseSpeed = new() { value = 10f };

    public FloatParameter _NoiseScale                = new() { value = 10f };
    public FloatParameter _NoiseSampleRotation       = new() { value = .5f };
    public FloatParameter _NoiseSampleRotationSize   = new() { value = .1f };
    public FloatParameter _NoiseSampleChromaticSplit = new() { value = .001f };
    public FloatParameter _NoiseSampleOffset         = new() { value = .001f };


    public FloatParameter _BorderSubtractor = new() { value = .9f };
    public FloatParameter _BorderMultiplier = new() { value = 10f };
    public FloatParameter _BorderNoiseAdder = new() { value = 1 };

    public ColorParameter _BorderColor = new() { value = Color.white };


    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported( PostProcessRenderContext context )
    {
        return enabled.value
               && intensity.value > 0f;
    }
}

public sealed class SketchPostRenderer : PostProcessEffectRenderer<SketchEffect>
{
    public Camera camera;

    public override void Render( PostProcessRenderContext context )
    {


        var sheet = context.propertySheets.Get( Shader.Find( "PostProcessing/SketchEffect" ) );

        var projectionInverse = GL.GetGPUProjectionMatrix( context.camera.projectionMatrix , false ).inverse;
        //Matrix4x4 projectionInverse = context.camera.projectionMatrix.inverse;
        var viewInverse = context.camera.cameraToWorldMatrix;

        var inverseViewProjection = projectionInverse * viewInverse;
        
        

        sheet.properties.SetTexture( "_HeightMap" , settings.heightMap );
        sheet.properties.SetTexture( "_PaintMap" , settings.paintMap );
        sheet.properties.SetVector( "_MapSize" , settings.mapSize );

        sheet.properties.SetFloat( "_Intensity" , settings.intensity.value );
        sheet.properties.SetFloat( "_Scale" , settings.scale.value );
        sheet.properties.SetFloat( "_ChangeSpeed" , settings.changeSpeed.value );


        sheet.properties.SetMatrix( "_InverseProjection" , projectionInverse );
        sheet.properties.SetMatrix( "_InverseView" , viewInverse );
        sheet.properties.SetMatrix( "_InverseViewProjection" , inverseViewProjection );

        sheet.properties.SetFloat( "_NoiseSpeed" , settings._NoiseSpeed.value );
        sheet.properties.SetFloat( "_NoiseScale" , settings._NoiseScale.value );
        sheet.properties.SetFloat( "_NoiseSampleRotation" , settings._NoiseSampleRotation.value );
        sheet.properties.SetFloat( "_NoiseSampleRotationSize" , settings._NoiseSampleRotationSize.value );
        sheet.properties.SetFloat( "_NoiseSampleChromaticSplit" , settings._NoiseSampleChromaticSplit.value );
        sheet.properties.SetFloat( "_NoiseSampleOffset" , settings._NoiseSampleOffset.value );
        sheet.properties.SetFloat( "_BorderSubtractor" , settings._BorderSubtractor.value );
        sheet.properties.SetFloat( "_BorderMultiplier" , settings._BorderMultiplier.value );
        sheet.properties.SetFloat( "_BorderNoiseAdder" , settings._BorderNoiseAdder.value );
        sheet.properties.SetColor( "_BorderColor" , settings._BorderColor.value );


        context.command.BlitFullscreenTriangle( context.source , context.destination , sheet , 0 );
    }
}