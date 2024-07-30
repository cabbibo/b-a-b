using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(FogEffectRenderer), PostProcessEvent.AfterStack, "Custom/FogEffect")]
public sealed class FogEffect : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("size")]
    public FloatParameter intensity = new FloatParameter { value = 0.5f };

    [Tooltip("The height map")]
    public TextureParameter heightMap = new TextureParameter { value = null };

    [Tooltip("The size of the map")]
    public Vector3Parameter mapSize = new Vector3Parameter { value = Vector3.zero };


    [Tooltip("The inverse projection matrix")]
    public Vector4Parameter inverseProjection1 = new Vector4Parameter { value = Vector4.zero };
    public Vector4Parameter inverseProjection2 = new Vector4Parameter { value = Vector4.zero };
    public Vector4Parameter inverseProjection3 = new Vector4Parameter { value = Vector4.zero };
    public Vector4Parameter inverseProjection4 = new Vector4Parameter { value = Vector4.zero };




    public FloatParameter _FogMultiplier = new FloatParameter { value = 1f };
    public FloatParameter _FogHeightMultiplier = new FloatParameter { value = 0.001f };
    public FloatParameter _FogHeightPower = new FloatParameter { value = 1f };
    public FloatParameter _FogDensityAtFar = new FloatParameter { value = 1f };
    public FloatParameter _FogDensityAtNear = new FloatParameter { value = 0f };
    public FloatParameter _FogStepSize = new FloatParameter { value = 80f };
    public FloatParameter _MaxFogTotal = new FloatParameter { value = 1f };
    public IntParameter _FogSamples = new IntParameter { value = 50 };

    public FloatParameter _OceanHeight = new FloatParameter { value = 240f };

    public ColorParameter _FogColorNear = new ColorParameter { value = Color.white };
    public ColorParameter _FogColorFar = new ColorParameter { value = Color.white };
    public ColorParameter _FogColorDistant = new ColorParameter { value = Color.white };

    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value
            && intensity.value > 0f;
    }

}

public sealed class FogEffectRenderer : PostProcessEffectRenderer<FogEffect>
{

    public Camera camera;
    public override void Render(PostProcessRenderContext context)
    {


        var sheet = context.propertySheets.Get(Shader.Find("PostProcessing/FogEffect"));
        sheet.properties.SetFloat("_Intensity", settings.intensity);


        // Debug.Log(context.camera.transform.position);
        //  Debug.Log(context.camera.projectionMatrix.inverse);
        //  Debug.Log(context.camera.cameraToWorldMatrix.inverse);

        Matrix4x4 projectionInverse = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false).inverse;
        //Matrix4x4 projectionInverse = context.camera.projectionMatrix.inverse;
        Matrix4x4 viewInverse = context.camera.cameraToWorldMatrix;





        Matrix4x4 inverseViewProjection = (projectionInverse * viewInverse);

        sheet.properties.SetTexture("_HeightMap", settings.heightMap);
        sheet.properties.SetVector("_MapSize", settings.mapSize);


        sheet.properties.SetMatrix("_InverseProjection", projectionInverse);
        sheet.properties.SetMatrix("_InverseView", viewInverse);
        sheet.properties.SetMatrix("_InverseViewProjection", inverseViewProjection);

        sheet.properties.SetFloat("_FogMultiplier", settings._FogMultiplier);
        sheet.properties.SetFloat("_FogHeightMultiplier", settings._FogHeightMultiplier);
        sheet.properties.SetFloat("_FogHeightPower", settings._FogHeightPower);
        sheet.properties.SetFloat("_FogDensityAtFar", settings._FogDensityAtFar);
        sheet.properties.SetFloat("_FogDensityAtNear", settings._FogDensityAtNear);
        sheet.properties.SetFloat("_FogStepSize", settings._FogStepSize);
        sheet.properties.SetFloat("_MaxFogTotal", settings._MaxFogTotal);
        sheet.properties.SetInt("_FogSamples", settings._FogSamples);
        sheet.properties.SetColor("_FogColorNear", settings._FogColorNear);
        sheet.properties.SetColor("_FogColorFar", settings._FogColorFar);
        sheet.properties.SetColor("_FogColorDistant", settings._FogColorDistant);
        sheet.properties.SetFloat("_OceanHeight", settings._OceanHeight);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}