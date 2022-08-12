using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(SunShaftsHDRPRenderer), PostProcessEvent.AfterStack, "Custom/SunShaftsHDRP")]
public sealed class SunShaftsHDRP : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("SunShafts effect intensity.")]
    public FloatParameter blend = new FloatParameter { value = 0.5f };

    public enum SunShaftsResolution
    {
        Low = 0,
        Normal = 1,
        High = 2,
    }

    public enum ShaftsScreenBlendMode
    {
        Screen = 0,
        Add = 1,
    }

    public SunShaftsResolution resolution = SunShaftsResolution.Normal;
    public ShaftsScreenBlendMode screenBlendMode = ShaftsScreenBlendMode.Screen;

    //public Transform sunTransform;
    //public int radialBlurIterations = 2;
    //public Color sunColor = Color.white;
    //public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
    //public float sunShaftBlurRadius = 2.5f;
    //public float sunShaftIntensity = 1.15f;

    //public float maxRadius = 0.75f;

    //public bool useDepthTexture = true;

    //public Shader sunShaftsShader;
    //private Material sunShaftsMaterial;

    //public Shader simpleClearShader;
    //private Material simpleClearMaterial;
    //public Transform sun;

    public Vector3Parameter sunTransform = new Vector3Parameter { value = new Vector3(0f, 0f, 0f) }; // Transform sunTransform;
    public IntParameter radialBlurIterations = new IntParameter { value = 2 };
    public ColorParameter sunColor = new ColorParameter { value = Color.white };
    public ColorParameter sunThreshold = new ColorParameter { value = new Color(0.87f, 0.74f, 0.65f) };
    public FloatParameter sunShaftBlurRadius = new FloatParameter { value = 2.5f };
    public FloatParameter sunShaftIntensity = new FloatParameter { value = 1.15f };

    public FloatParameter maxRadius = new FloatParameter { value = 0.75f };

    public BoolParameter useDepthTexture = new BoolParameter { value = true };

    //public Shader sunShaftsShader;
    // private Material sunShaftsMaterial;

    //public Shader simpleClearShader;
    //private Material simpleClearMaterial;

}

public sealed class SunShaftsHDRPRenderer : PostProcessEffectRenderer<SunShaftsHDRP>
{



    //v5.0
    void onEnable()
    {
        //SceneRenderPipeline.
        //RenderPipeline.
        // HDRenderPipeline.  
    }


    //public override bool CheckResources()
    //{
    //    CheckSupport(useDepthTexture);

    //    sunShaftsMaterial = CheckShaderAndCreateMaterial(sunShaftsShader, sunShaftsMaterial);
    //    simpleClearMaterial = CheckShaderAndCreateMaterial(simpleClearShader, simpleClearMaterial);

    //    if (!isSupported)
    //        ReportAutoDisable();
    //    return isSupported;
    //}


    RenderTexture lrColorB;
    RenderTexture lrDepthBuffer;


    public override void Render(PostProcessRenderContext context)
    {
        //var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/GrayscaleShafts"));
        var sheetSHAFTS = context.propertySheets.Get(Shader.Find("Hidden/Custom/GrayscaleShafts"));
        //sheet.properties.SetFloat("_Blend", settings.blend);
        sheetSHAFTS.properties.SetFloat("_Blend", settings.blend);

        //scontext.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

        //if (CheckResources() == false)
        //{
        //    Graphics.Blit(source, destination);
        //    return;
        //}
        Camera camera = Camera.main;
        // we actually need to check this every frame
        if (settings.useDepthTexture)
        {
            // GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
            camera.depthTextureMode |= DepthTextureMode.Depth;
        }
        //int divider = 4;
        //if (settings.resolution == SunShaftsHDRP.SunShaftsResolution.Normal)
        //    divider = 2;
        // else if (settings.resolution == SunShaftsHDRP.SunShaftsResolution.High)
        //    divider = 1;

        Vector3 v = Vector3.one * 0.5f;
        if (settings.sunTransform != Vector3.zero)
            v = camera.WorldToViewportPoint(settings.sunTransform);
        else
            v = new Vector3(0.5f, 0.5f, 0.0f);

        int rtW = context.width; //source.width / divider;
        int rtH = context.width; //source.height / divider;

        //lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);
        lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);

        // mask out everything except the skybox
        // we have 2 methods, one of which requires depth buffer support, the other one is just comparing images

        //    sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);
        //    sunShaftsMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));
        //    sunShaftsMaterial.SetVector("_SunThreshold", sunThreshold);
        sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * settings.sunShaftBlurRadius);
        sheetSHAFTS.properties.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, settings.maxRadius));
        sheetSHAFTS.properties.SetVector("_SunThreshold", settings.sunThreshold);

        if (!settings.useDepthTexture)
        {
            //var format= GetComponent<Camera>().hdr ? RenderTextureFormat.DefaultHDR: RenderTextureFormat.Default;
            var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
            RenderTexture tmpBuffer = RenderTexture.GetTemporary(context.width, context.height, 0, format);
            RenderTexture.active = tmpBuffer;
            GL.ClearWithSkybox(false, camera);

            //sunShaftsMaterial.SetTexture("_Skybox", tmpBuffer);
            sheetSHAFTS.properties.SetTexture("_Skybox", tmpBuffer);
            //        Graphics.Blit(source, lrDepthBuffer, sunShaftsMaterial, 3);
            context.command.BlitFullscreenTriangle(context.source, lrDepthBuffer, sheetSHAFTS, 3);
            RenderTexture.ReleaseTemporary(tmpBuffer);
        }
        else
        {
            //          Graphics.Blit(source, lrDepthBuffer, sunShaftsMaterial, 2);
            context.command.BlitFullscreenTriangle(context.source, lrDepthBuffer, sheetSHAFTS, 2);

        }
        //  context.command.BlitFullscreenTriangle(lrDepthBuffer, context.destination, sheet, 5);
        // return;
        // paint a small black small border to get rid of clamping problems
        //      DrawBorder(lrDepthBuffer, simpleClearMaterial);

        // radial blur:

        //settings.radialBlurIterations =  Mathf.Clamp((int)settings.radialBlurIterations, 1, 4);
        int radialBlurIterations = Mathf.Clamp(settings.radialBlurIterations, 1, 4);

        float ofs = settings.sunShaftBlurRadius * (1.0f / 768.0f);

        //sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        //sunShaftsMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));
        sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        sheetSHAFTS.properties.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, settings.maxRadius));

        for (int it2 = 0; it2 < radialBlurIterations; it2++)
        {
            // each iteration takes 2 * 6 samples
            // we update _BlurRadius each time to cheaply get a very smooth look

            lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);
            // Graphics.Blit(lrDepthBuffer, lrColorB, sunShaftsMaterial, 1);
            context.command.BlitFullscreenTriangle(lrDepthBuffer, lrColorB, sheetSHAFTS, 1);
            RenderTexture.ReleaseTemporary(lrDepthBuffer);
            ofs = settings.sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
            //sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

            lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);
            // Graphics.Blit(lrColorB, lrDepthBuffer, sunShaftsMaterial, 1);
            context.command.BlitFullscreenTriangle(lrColorB, lrDepthBuffer, sheetSHAFTS, 1);
            RenderTexture.ReleaseTemporary(lrColorB);
            ofs = settings.sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
            // sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        }

        // put together:

        if (v.z >= 0.0f)
        {
            //sunShaftsMaterial.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
            sheetSHAFTS.properties.SetVector("_SunColor", new Vector4(settings.sunColor.value.r, settings.sunColor.value.g, settings.sunColor.value.b, settings.sunColor.value.a) * settings.sunShaftIntensity);
        }
        else
        {
            // sunShaftsMaterial.SetVector("_SunColor", Vector4.zero); // no backprojection !
            sheetSHAFTS.properties.SetVector("_SunColor", Vector4.zero); // no backprojection !
        }
        //sunShaftsMaterial.SetTexture("_ColorBuffer", lrDepthBuffer);
        sheetSHAFTS.properties.SetTexture("_ColorBuffer", lrDepthBuffer);
        //    Graphics.Blit(context.source, context.destination, sunShaftsMaterial, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheetSHAFTS, (settings.screenBlendMode == SunShaftsHDRP.ShaftsScreenBlendMode.Screen) ? 0 : 4);

        //RenderTexture.ReleaseTemporary(lrColorB);
        RenderTexture.ReleaseTemporary(lrDepthBuffer);

    }
}