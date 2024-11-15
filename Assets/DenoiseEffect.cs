using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DenoiseEffectRenderer), PostProcessEvent.AfterStack, "Custom/DenoiseEffect")]
public sealed class DenoiseEffect : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("size")]
    public FloatParameter intensity = new FloatParameter { value = 0.5f };

    public FloatParameter _Radius = new FloatParameter { value = 1f };
    public FloatParameter _Sigma = new FloatParameter { value = 1f };
    public FloatParameter _kSigma = new FloatParameter { value = 1f };
    public FloatParameter _Threshold = new FloatParameter { value = 0.5f };

// float sigma  >  0 - sigma Standard Deviation
//  float kSigma >= 0 - sigma coefficient 
//      kSigma * sigma  -->  radius of the circular kernel
//  float threshold   - edge sharpening threshold 


    // Only use if the blend value is greater than 1
    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return enabled.value
            && intensity.value > 0f;
    }

}

public sealed class DenoiseEffectRenderer : PostProcessEffectRenderer<DenoiseEffect>
{

    public Camera camera;
    public override void Render(PostProcessRenderContext context)
    {


        var sheet = context.propertySheets.Get(Shader.Find("PostProcessing/DenoiseEffect"));
        sheet.properties.SetFloat("_Intensity", settings.intensity);
        sheet.properties.SetFloat("_Radius", settings._Radius);
        sheet.properties.SetFloat("_Sigma", settings._Sigma);
        sheet.properties.SetFloat("_kSigma", settings._kSigma);
        sheet.properties.SetFloat("_Threshold", settings._Threshold);
        

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}