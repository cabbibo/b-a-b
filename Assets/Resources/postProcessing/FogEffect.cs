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
        Debug.Log(context.camera.projectionMatrix.inverse);
        Debug.Log(context.camera.cameraToWorldMatrix.inverse);

        Matrix4x4 projectionInverse = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false).inverse;
        //Matrix4x4 projectionInverse = context.camera.projectionMatrix.inverse;
        Matrix4x4 viewInverse = context.camera.cameraToWorldMatrix;





        Matrix4x4 inverseViewProjection = (projectionInverse * viewInverse);

        sheet.properties.SetTexture("_HeightMap", settings.heightMap);
        sheet.properties.SetVector("_MapSize", settings.mapSize);


        sheet.properties.SetMatrix("_InverseProjection", projectionInverse);
        sheet.properties.SetMatrix("_InverseView", viewInverse);
        sheet.properties.SetMatrix("_InverseViewProjection", inverseViewProjection);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}