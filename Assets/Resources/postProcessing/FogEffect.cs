using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(FogEffectRenderer), PostProcessEvent.AfterStack, "Custom/FogEffect")]
public sealed class FogEffect : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("size")]
    public FloatParameter intensity = new FloatParameter { value = 0.5f };






    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value
            && intensity.value > 0f;
    }

}

public sealed class FogEffectRenderer : PostProcessEffectRenderer<FogEffect>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("PostProcessing/FogEffect"));
        sheet.properties.SetFloat("_Intensity", settings.intensity);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}