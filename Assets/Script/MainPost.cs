using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(MainPostRenderer), PostProcessEvent.AfterStack, "Custom/MainPost")]
public sealed class MainPost : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("_Hue")]
    public FloatParameter _Hue = new FloatParameter { value = 0.0f };


    [Range(0f, 1f), Tooltip("_Saturation")]
    public FloatParameter _Saturation = new FloatParameter { value = 1.0f };

    
    [Range(0f, 1f), Tooltip("_Lightness")]
    public FloatParameter _Lightness = new FloatParameter { value = 1.0f };



    [Range(0f, 1f), Tooltip("_Blend")]
    public FloatParameter _Blend = new FloatParameter { value = 0.5f };


    
    [Range(0f, 1f), Tooltip("_Fade")]
    public FloatParameter _Fade = new FloatParameter { value = 0.0f };
    

    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value
            && ( _Blend.value > 0f || _Fade.value >0f);
    }

}

public sealed class MainPostRenderer : PostProcessEffectRenderer<MainPost>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("PostProcessing/MainPost"));
        sheet.properties.SetFloat("_Hue", settings._Hue);
        sheet.properties.SetFloat("_Saturation", settings._Saturation);
        sheet.properties.SetFloat("_Lightness", settings._Lightness);
        sheet.properties.SetFloat("_Blend", settings._Blend);
        sheet.properties.SetFloat("_Fade", settings._Fade);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}