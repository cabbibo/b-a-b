using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

using WrenUtils;
using Crest;


[CreateAssetMenu(fileName = "PostParameters", menuName = "PostParameters", order = 1)]
public class PostParameters : ScriptableObject
{



    public bool mainPost;
    public bool bloom;
    public bool colorGrading;
    public bool vignette;
    public bool lensDistortion;
    public bool chromaticAberration;
    public bool fogEffect;
    public bool depthOfField;
    public bool glitchEffect;
    public bool splatEffect;
    public bool ambientOcclusion;
    public bool spaterPost;
    public bool astigma;



    [Header("Main Post Settings")]

    public float mainHue;
    public float mainSaturation;
    public float mainLightness;
    public float mainBlend;
    public float mainFade;

    public Texture2D biomeMap;



    [Header("Bloom Settings")]
    public float bloomIntensity;
    public float bloomThreshold;
    public float bloomDirtIntensity;


    [Header("Color Grading Settings")]
    public Color colorFilter;





    [Header("Vignette Settings")]
    public float vignetteIntensity;
    public float vignetteSmoothness;
    public float vignetteRoundness;
    public Color vignetteColor;
    public bool vignetteRounded;






    [Header("Lens Distortion Settings")]
    public float lensDistortionIntensity;
    public float lensDistortionScale;

    [Header("Chromatic Aberration Settings")]
    public float chromaticAberrationIntensity;



    [Header("Fog Settings")]
    public float fogIntensity=1;
    public float fogHeightPower=1;
    public float fogHeightMultiplier=30;

    public int fogSteps = 50;
    public float fogStepSize= 10;
    public float maxFog = 1;
    public Color fogNear = Color.white;
    public Color fogFar = Color.white;
    public Color fogDistant = Color.white;
    public float fogOceanHeight = 1;

    public float fogDensityNear = 1;
    public float fogDensityFar = 1;

    public float fogLightColorImportance = 1;

    [Header("Depth Of Field Settings")]
    public float depthOfFieldAperture;
    public float depthOfFieldFocalLength;
    public float depthOfFieldFocusDistance;
    public bool focusOnWren;


    [Header("Glitch Settings")]
    public float glitchIntensity;
    public float glitchBlend;
    public float glitchSize;
    public float glitchAmount;
    public float glitchSpeed;
    public float glitchSplit;




    [Header("Splat Settings")]
    public bool renderBackground;
    public int splatAmount;
    public float splatSize;

    public float splatSpeed;
    public float splatMatchAmount;
    public float normalForce;
    public float curlForce;
    public float curlSize;
    public float normalOffset;



    [Header("Ambient Occlusion Settings")]
    public float ambientOcclusionIntensity;
    public Color ambientOcclusionColor;



    [Header("Spater Post Settings")]
    public float spaterPostFade;

    [Header("Astigma Settings")]
    public float astigmaIntensity;
    public float astigmaScale;
    public float astigmaCutoff;
    public float astimgaAngle;
    public float astigmaNumSamples;
    public float astigmaNumDirections;


    public void SetValues(
        MainPost mainPost_Reference,
        Bloom bloom_Reference,
        ColorGrading colorGrading_Reference,
        Vignette vignette_Reference,
        LensDistortion lensDistortion_Reference,
        ChromaticAberration chromaticAberration_Reference,
        FogEffect fogEffect_Reference,
        DepthOfField depthOfField_Reference,
        GlitchEffect glitchEffect_Reference,
        AmbientOcclusion ambientOcclusion_Reference,
        SpaterPostSettings spaterPost_Reference,
        Astigma astigma_Reference,
        PlaceParticlesOnDepthMap placeParticlesOnDepthMap
    )
    {

//Debug.Log("Setting Post Parameters");

        /*if (God.wren != null)
                {
                    CartToPolar(God.wren.transform.position);
                }*/



        mainPost_Reference.enabled.Override(mainPost);
        bloom_Reference.enabled.Override(bloom);
        colorGrading_Reference.enabled.Override(colorGrading);
        vignette_Reference.enabled.Override(vignette);
        lensDistortion_Reference.enabled.Override(lensDistortion);
        chromaticAberration_Reference.enabled.Override(chromaticAberration);
        fogEffect_Reference.enabled.Override(fogEffect);
        depthOfField_Reference.enabled.Override(depthOfField);
        glitchEffect_Reference.enabled.Override(glitchEffect);
        ambientOcclusion_Reference.enabled.Override(ambientOcclusion);
        spaterPost_Reference.enabled.Override(spaterPost);
        astigma_Reference.enabled.Override(astigma);




        ambientOcclusion_Reference.intensity.value = ambientOcclusionIntensity;
        ambientOcclusion_Reference.color.value = ambientOcclusionColor;


        //        print(post);
        mainPost_Reference._Hue.value = mainHue;
        mainPost_Reference._Saturation.value = mainSaturation;
        mainPost_Reference._Lightness.value = mainLightness;
        mainPost_Reference._Blend.value = mainBlend;
        mainPost_Reference._Fade.value = mainFade;

        if (God.wren != null && focusOnWren)
        {
            depthOfFieldFocusDistance = Vector3.Distance(God.wren.transform.position, God.camera.transform.position);
        }

        // todo if we are focusing on something else make that be the focus object ( even better make them both be in focus)
        depthOfField_Reference.focusDistance.value = depthOfFieldFocusDistance;
        depthOfField_Reference.aperture.value = depthOfFieldAperture;
        depthOfField_Reference.focalLength.value = depthOfFieldFocalLength;


        astigma_Reference.intensity.value = astigmaIntensity;
        astigma_Reference.scale.value = astigmaScale;
        astigma_Reference.cutoff.value = astigmaCutoff;
        astigma_Reference.angle.value = astimgaAngle;
        astigma_Reference.numSamples.value = astigmaNumSamples;
        astigma_Reference.numDirections.value = astigmaNumDirections;


        bloom_Reference.intensity.value = bloomIntensity;
        bloom_Reference.threshold.value = bloomThreshold;


        bloom_Reference.dirtIntensity.value = bloomDirtIntensity;


        colorGrading_Reference.colorFilter.value = colorFilter;


        vignette_Reference.intensity.value = vignetteIntensity;
        vignette_Reference.smoothness.value = vignetteSmoothness;
        vignette_Reference.roundness.value = vignetteRoundness;
        vignette_Reference.color.value = vignetteColor;
        vignette_Reference.rounded.value = vignetteRounded;

        fogEffect_Reference.intensity.value = fogIntensity;
        fogEffect_Reference._FogMultiplier.value = fogIntensity;
        fogEffect_Reference._FogHeightMultiplier.value = fogHeightMultiplier;
        fogEffect_Reference._FogHeightPower.value = fogHeightPower;

        fogEffect_Reference._FogDensityAtFar.value = fogDensityFar;
        fogEffect_Reference._FogDensityAtNear.value = fogDensityNear;
        fogEffect_Reference._FogStepSize.value = fogStepSize;
        fogEffect_Reference._MaxFogTotal.value = maxFog;
        fogEffect_Reference._FogSamples.value = fogSteps;
        fogEffect_Reference._FogColorNear.value = fogNear;
        fogEffect_Reference._FogColorFar.value = fogFar;
        fogEffect_Reference._FogColorDistant.value = fogDistant;
        fogEffect_Reference._OceanHeight.value = fogOceanHeight;

        fogEffect_Reference._LightColorImportance.value = fogLightColorImportance;


        lensDistortion_Reference.intensity.value = lensDistortionIntensity;
        lensDistortion_Reference.scale.value = lensDistortionScale;

        chromaticAberration_Reference.intensity.value = chromaticAberrationIntensity;

        glitchEffect_Reference.blend.value = glitchBlend;
        glitchEffect_Reference.size.value = glitchSize;
        glitchEffect_Reference.amount.value = glitchAmount;
        glitchEffect_Reference.speed.value = glitchSpeed;
        glitchEffect_Reference.split.value = glitchSplit;

        spaterPost_Reference.fade.value = spaterPostFade;

        if( splatAmount<1 )
        {
            splatAmount = 1;
        }
        if( splatAmount != placeParticlesOnDepthMap.splatAmount )
        {
            placeParticlesOnDepthMap.Reset();
        }

        placeParticlesOnDepthMap.renderBackground = renderBackground;
        placeParticlesOnDepthMap.splatAmount = splatAmount;
        placeParticlesOnDepthMap.splatSize = splatSize;
        placeParticlesOnDepthMap.splatSpeed = splatSpeed;
        placeParticlesOnDepthMap.splatMatchAmount = splatMatchAmount;
        placeParticlesOnDepthMap.normalForce = normalForce;
        placeParticlesOnDepthMap.curlForce = curlForce;
        placeParticlesOnDepthMap.curlSize = curlSize;
        placeParticlesOnDepthMap.normalOffset = normalOffset;



    }




    Vector2 CartToPolar(Vector3 position)
    {

        float angle = Mathf.Atan2(position.x, position.z);
        float radius = (new Vector2(position.x, position.z)).magnitude;


        float x = (position.x + 2048) / 4096;
        float y = (position.z + 2048) / 4096;

        Color c = biomeMap.GetPixelBilinear(x, y, 0);

        //        print( c.a);


        float h, s, v;

        Color.RGBToHSV(c, out h, out s, out v);

        mainHue = h;
        mainBlend = c.a;



        /* angle = (angle > 0 ? angle : (2*Mathf.PI + angle));
         angle /= 2 * Mathf.PI;

         _Hue = angle;
         _Hue += angleOffset;
         _Hue %= 1;


         print( _Hue );*/

        return new Vector2(angle, radius);

    }

    public void CopyTo(PostParameters p)
    {

        //print(p);
        p.mainPost = mainPost;
        p.bloom = bloom;
        p.colorGrading = colorGrading;
        p.vignette = vignette;
        p.lensDistortion = lensDistortion;
        p.chromaticAberration = chromaticAberration;
        p.fogEffect = fogEffect;
        p.depthOfField = depthOfField;
        p.glitchEffect = glitchEffect;
        p.splatEffect = splatEffect;
        p.ambientOcclusion = ambientOcclusion;
        p.spaterPost = spaterPost;
        p.astigma = astigma;

        p.mainHue = mainHue;
        p.mainSaturation = mainSaturation;
        p.mainLightness = mainLightness;
        p.mainBlend = mainBlend;
        p.mainFade = mainFade;

        p.biomeMap = biomeMap;

        p.bloomIntensity = bloomIntensity;
        p.bloomThreshold = bloomThreshold;
        p.bloomDirtIntensity = bloomDirtIntensity;

        p.colorFilter = colorFilter;

        p.vignetteIntensity = vignetteIntensity;
        p.vignetteSmoothness = vignetteSmoothness;
        p.vignetteRoundness = vignetteRoundness;
        p.vignetteColor = vignetteColor;
        p.vignetteRounded = vignetteRounded;

        p.lensDistortionIntensity = lensDistortionIntensity;
        p.lensDistortionScale = lensDistortionScale;

        p.chromaticAberrationIntensity = chromaticAberrationIntensity;

        p.fogIntensity = fogIntensity;
        p.fogHeightPower = fogHeightPower;
        p.fogHeightMultiplier = fogHeightMultiplier;
        p.fogSteps = fogSteps;
        p.fogStepSize = fogStepSize;
        p.maxFog = maxFog;
        p.fogNear = fogNear;
        p.fogFar = fogFar;
        p.fogDistant = fogDistant;
        p.fogOceanHeight = fogOceanHeight;
        p.fogDensityNear = fogDensityNear;
        p.fogDensityFar = fogDensityFar;
        p.fogLightColorImportance = fogLightColorImportance;




        p.depthOfFieldAperture = depthOfFieldAperture;
        p.depthOfFieldFocalLength = depthOfFieldFocalLength;
        p.depthOfFieldFocusDistance = depthOfFieldFocusDistance;
        p.focusOnWren = focusOnWren;

        p.glitchIntensity = glitchIntensity;
        p.glitchBlend = glitchBlend;
        p.glitchSize = glitchSize;
        p.glitchAmount = glitchAmount;
        p.glitchSpeed = glitchSpeed;
        p.glitchSplit = glitchSplit;

        p.renderBackground = renderBackground;
        p.splatAmount = splatAmount;
        p.splatSize = splatSize;
        p.splatSpeed = splatSpeed;
        p.normalForce = normalForce;
        p.curlForce = curlForce;
        p.curlSize = curlSize;
        p.splatMatchAmount = splatMatchAmount;
        p.normalOffset = normalOffset;

        p.ambientOcclusionIntensity = ambientOcclusionIntensity;
        p.ambientOcclusionColor = ambientOcclusionColor;

        p.spaterPostFade = spaterPostFade;

        p.astigmaIntensity = astigmaIntensity;
        p.astigmaScale = astigmaScale;
        p.astigmaCutoff = astigmaCutoff;
        p.astimgaAngle = astimgaAngle;
        p.astigmaNumSamples = astigmaNumSamples;
        p.astigmaNumDirections = astigmaNumDirections;



    }


    public void OnValidate()
    {
//        Debug.Log("HIII");

        God.postController.OnPostParametersValidate(this);
    }


}