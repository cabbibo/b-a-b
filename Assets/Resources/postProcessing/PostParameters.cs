using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using WrenUtils;
using Crest;
using FloatParameter = UnityEngine.Rendering.PostProcessing.FloatParameter;


[CreateAssetMenu( fileName = "PostParameters" , menuName = "PostParameters" , order = 1 )]
public class PostParameters : ScriptableObject
{
    public bool mainPost            = false;
    public bool bloom               = false;
    public bool colorGrading        = false;
    public bool vignette            = false;
    public bool lensDistortion      = false;
    public bool chromaticAberration = false;
    public bool fogEffect           = false;
    public bool depthOfField        = false;
    public bool glitchEffect        = false;
    public bool splatEffect         = false;
    public bool ambientOcclusion    = false;
    public bool spaterPost          = false;
    public bool astigma             = false;
    public bool sketchEffect        = false;
    public bool dithered            = false;
    public bool distanceFog         = false;
    public bool heatWaveEffect      = false;
    public bool lensFlareEffect     = false;


    [Header( "Main Post Settings" )]
    public float mainHue = 0;

    public float mainSaturation = 1;
    public float mainLightness  = 1;
    public float mainBlend      = 1;
    public float mainFade       = 1;

    public Texture2D biomeMap = null; //Texture2D.blackTexture;


    [Header( "Bloom Settings" )]
    public float bloomIntensity = 1;

    public float bloomThreshold     = 1;
    public float bloomDirtIntensity = 1;


    [Header( "Color Grading Settings" )]
    public Color colorFilter = Color.white;

    public float   hueShift     = 0;
    public float   saturation   = 1;
    public float   brightness   = 1;
    public float   contrast     = 1;
    public float   postExposure = 0;
    public Vector4 lift         = new(1f , 1f , 1f , 0f);
    public Vector4 gamma        = new(1f , 1f , 1f , 0f);
    public Vector4 gain         = new(1f , 1f , 1f , 0f);


    [Header( "Vignette Settings" )]
    public float vignetteIntensity = 1;

    public float vignetteSmoothness = 1;
    public float vignetteRoundness  = 1;
    public Color vignetteColor      = Color.black;
    public bool  vignetteRounded    = false;


    [Header( "Lens Distortion Settings" )]
    public float lensDistortionIntensity = 1;

    public float lensDistortionScale = 1;

    [Header( "Chromatic Aberration Settings" )]
    public float chromaticAberrationIntensity = 1;


    [Header( "Fog Settings" )]
    public float fogIntensity = 1;

    public float fogHeightPower           = 1;
    public float fogHeightMultiplier      = 30;
    public float fogLightToDarkMultiplier = 2;

    public int   fogSteps       = 50;
    public float fogStepSize    = 10;
    public float maxFog         = 1;
    public Color fogNear        = Color.white;
    public Color fogFar         = Color.white;
    public Color fogDistant     = Color.white;
    public float fogOceanHeight = 1;

    public float fogDensityNear = 1;
    public float fogDensityFar  = 1;

    public float fogLightColorImportance = 1;

    [Header( "Depth Of Field Settings" )]
    public float depthOfFieldAperture = 1;

    public float depthOfFieldFocalLength   = 1;
    public float depthOfFieldFocusDistance = 10;
    public bool  focusOnWren               = true;


    [Header( "Glitch Settings" )]
    public float glitchIntensity = 1;

    public float glitchBlend  = 1;
    public float glitchSize   = 1;
    public float glitchAmount = 1;
    public float glitchSpeed  = 1;
    public float glitchSplit  = 1;


    [Header( "Splat Settings" )]
    public bool renderBackground = true;

    public Color splatsBackgroundColor = Color.black;

    public int   splatAmount = 1111;
    public float splatSize   = 1;

    public Texture2D splatTexture     = null;
    public int       splatTextureSize = 1;

    public Color splatDiscardColor         = Color.black;
    public float splatDiscardCutoff        = .1f;
    public float splatSpeed                = 1;
    public float splatMatchAmount          = 0;
    public float normalForce               = 0;
    public float curlForce                 = 0;
    public float curlSize                  = 0;
    public float normalOffset              = 0;
    public float splatHueRandomness        = 0;
    public float splatSaturationRandomness = 0;
    public float splatLightnessRandomness  = 0;
    public float splatColorMultiplier      = 2;


    [Header( "Ambient Occlusion Settings" )]
    public float ambientOcclusionIntensity = 1;

    public Color ambientOcclusionColor = Color.black;


    [Header( "Spater Post Settings" )]
    public float spaterPostFade = 1;

    public Texture2D spaterFrameTexture          = null; //Texture2D.blackTexture;
    public Texture2D spaterFrameNoiseTexture     = null; //Texture2D.blackTexture;
    public Color     spaterFrameColor            = Color.white;
    public float     spaterFrameNoiseScale       = 1;
    public float     spaterFrameNoiseChangeSpeed = 1;
    public float     spaterFrameNoiseWeight      = 1;
    public float     spaterOverallMultiplier     = 1;
    public float     spaterAudioPower            = 1;
    public float     spaterAudioBase             = 1;
    public float     spaterAudioDistort          = 1;
    public float     spaterAudioLookupSize       = 1;
    public float     spaterFrameNoiseRotation    = 1;


    [Header( "Astigma Settings" )]
    public float astigmaIntensity = 1;

    public float astigmaScale         = 1;
    public float astigmaCutoff        = 1;
    public float astimgaAngle         = 1;
    public float astigmaNumSamples    = 10;
    public float astigmaNumDirections = 6;

    public bool      astigmaUseTexture   = false;
    public Texture2D astigmaBokehTexture = null; //Texture2D.whiteTexture;
    public Color     astigmaColor        = Color.white;


    [Header( "Sketch Settings" )]
    public float sketchIntensity = 1;

    public float     sketchScale                     = 1;
    public float     sketchChangeSpeed               = 1;
    public Texture2D sketchPaintMap                  = null; // Texture2D.blackTexture;
    public float     sketchNoiseSpeed                = 6;
    public float     sketchNoiseScale                = 4;
    public float     sketchNoiseSampleRotation       = .5f;
    public float     sketchNoiseSampleRotationSize   = .2f;
    public float     sketchNoiseSampleChromaticSplit = .002f;
    public float     sketchNoiseSampleOffset         = .00f;
    public float     sketchBorderSubtractor          = .95f;
    public float     sketchBorderMultiplier          = 20f;
    public float     sketchBorderNoiseAdder          = .6f;
    public Color     sketchBorderColor               = Color.white;

    [Header( "Dither Settings" )]
    public float ditherIntensity = 1;

    public int       ditherPixelScale     = 1;
    public Texture2D ditherPattern        = null; //Texture2D.whiteTexture;
    public Texture3D ditherPrimary        = null;
    public Texture3D ditherSecondary      = null;
    public float     ditherNoiseIntensity = 0;

    [Header( "Distance Fog" )]
    public float DistanceFogStart = 100;

    public float DistanceFogEnd              = 1000;
    public float DistanceFogAmount           = .9f;
    public Color DistanceFogStartColor       = Color.black;
    public Color DistanceFogEndColor         = Color.white;
    public float DistanceFogSkyboxImportance = 0;

    [Header( "HeatWave Effect" )]
    public float HeatWaveIntensity = 1;

    public float HeatWaveSize  = 1;
    public float HeatWaveSpeed = 1;

    public float HeatWaveStart      = 0;
    public float HeatWaveEnd        = 1000;
    public float HeatWaveAberration = .2f;


    [Header( "Lens Flare Effect" )]
    public float LensFlareIntensity = 1f;

    public float LensFlareSpacing = 100f;
    public float SunDogIntensity  = 100f;
    public float SunDogSpacing    = 100f;


    [Header( "Sun/Moon Settings" )]
    public bool sunAutoUpdate = true;

    public float daySpeed   = 30f;
    public float nightSpeed = 10f;

    [UnityEngine.Range( 0f , 1f )]
    public float startNormalizedPosition = 0.25f; // 0.25 = midday

    public Vector3 sunAxis;

    public Gradient sunGradient = new()
    {
        colorKeys = new GradientColorKey[]
        {
            new(new Color( 1 , 0 , 0 ) , 0f) ,
            new(new Color( 1 , 1 , 1 ) , 0.5f) ,
            new(new Color( 1 , 0 , 0 ) , 1f)
        } ,
        alphaKeys = new GradientAlphaKey[]
        {
            new(1f , 0f) ,
            new(1f , 1f)
        }
    };

    public Gradient moonGradient = new()
    {
        colorKeys = new GradientColorKey[]
        {
            new(new Color( 0 , 0 , 1 ) , 0f) ,
            new(new Color( 1 , 1 , 1 ) , 0.5f) ,
            new(new Color( 0 , 0 , 1 ) , 1f)
        } ,
        alphaKeys = new GradientAlphaKey[]
        {
            new(1f , 0f) ,
            new(1f , 1f)
        }
    };

    public void SetValues(
        MainPost mainPost_Reference ,
        Bloom bloom_Reference ,
        ColorGrading colorGrading_Reference ,
        Vignette vignette_Reference ,
        LensDistortion lensDistortion_Reference ,
        ChromaticAberration chromaticAberration_Reference ,
        FogEffect fogEffect_Reference ,
        DepthOfField depthOfField_Reference ,
        GlitchEffect glitchEffect_Reference ,
        AmbientOcclusion ambientOcclusion_Reference ,
        SpaterPostSettings spaterPost_Reference ,
        Astigma astigma_Reference ,
        SketchEffect sketchEffect_Reference ,
        QuickDither.Dithered dither_Reference ,
        DistanceFogEffect distanceFogEffect_Reference ,
        HeatWaveEffect heatWaveEffect_Reference ,
        LensFlareEffect lensFlareEffect_Reference ,
        PlaceParticlesOnDepthMap placeParticlesOnDepthMap
    )
    {

        //Debug.Log("Setting Post Parameters");

        /*if (God.wren != null)
                {
                    CartToPolar(God.wren.transform.position);
                }*/

        mainPost_Reference.enabled.Override( mainPost );
        bloom_Reference.enabled.Override( bloom );
        colorGrading_Reference.enabled.Override( colorGrading );
        vignette_Reference.enabled.Override( vignette );
        lensDistortion_Reference.enabled.Override( lensDistortion );
        chromaticAberration_Reference.enabled.Override( chromaticAberration );
        fogEffect_Reference.enabled.Override( fogEffect );
        depthOfField_Reference.enabled.Override( depthOfField );
        glitchEffect_Reference.enabled.Override( glitchEffect );
        ambientOcclusion_Reference.enabled.Override( ambientOcclusion );
        spaterPost_Reference.enabled.Override( spaterPost );
        astigma_Reference.enabled.Override( astigma );
        sketchEffect_Reference.enabled.Override( sketchEffect );
        dither_Reference.enabled.Override( dithered );
        distanceFogEffect_Reference.enabled.Override( distanceFog );
        heatWaveEffect_Reference.enabled.Override( heatWaveEffect );
        lensFlareEffect_Reference.enabled.Override( lensFlareEffect );


        ambientOcclusion_Reference.intensity.value = ambientOcclusionIntensity;
        ambientOcclusion_Reference.color.value = ambientOcclusionColor;


        //        print(post);
        mainPost_Reference._Hue.value = mainHue;
        mainPost_Reference._Saturation.value = mainSaturation;
        mainPost_Reference._Lightness.value = mainLightness;
        mainPost_Reference._Blend.value = mainBlend;
        mainPost_Reference._Fade.value = mainFade;

        if ( God.wren != null && focusOnWren ) {
            depthOfFieldFocusDistance = Vector3.Distance( God.wren.transform.position , God.camera.transform.position );
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
        astigma_Reference.useTexture.value = astigmaUseTexture;
        astigma_Reference.texture.value = astigmaBokehTexture;
        astigma_Reference.color.value = astigmaColor;


        bloom_Reference.intensity.value = bloomIntensity;
        bloom_Reference.threshold.value = bloomThreshold;


        bloom_Reference.dirtIntensity.value = bloomDirtIntensity;


        colorGrading_Reference.colorFilter.value = colorFilter;
        colorGrading_Reference.hueShift.value = hueShift;
        colorGrading_Reference.contrast.value = contrast;
        colorGrading_Reference.postExposure.value = postExposure;
        colorGrading_Reference.saturation.value = saturation;
        colorGrading_Reference.brightness.value = brightness;
        colorGrading_Reference.lift.value = lift;
        colorGrading_Reference.gamma.value = gamma;
        colorGrading_Reference.gain.value = gain;


        vignette_Reference.intensity.value = vignetteIntensity;
        vignette_Reference.smoothness.value = vignetteSmoothness;
        vignette_Reference.roundness.value = vignetteRoundness;
        vignette_Reference.color.value = vignetteColor;
        vignette_Reference.rounded.value = vignetteRounded;

        fogEffect_Reference.intensity.value = fogIntensity;
        fogEffect_Reference._FogMultiplier.value = fogIntensity;
        fogEffect_Reference._LightToDarkMultiplier.value = fogLightToDarkMultiplier;
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
        spaterPost_Reference.frameTex.value = spaterFrameTexture;
        spaterPost_Reference.frameNoiseTex.value = spaterFrameNoiseTexture;
        spaterPost_Reference.frameNoiseScale.value = spaterFrameNoiseScale;
        spaterPost_Reference.frameNoiseSpeed.value = spaterFrameNoiseChangeSpeed;
        spaterPost_Reference.color.value = spaterFrameColor;
        spaterPost_Reference.frameNoiseWeight.value = spaterFrameNoiseWeight;
        spaterPost_Reference.overallMultiplier.value = spaterOverallMultiplier;
        spaterPost_Reference.audioPower.value = spaterAudioPower;
        spaterPost_Reference.audioBase.value = spaterAudioBase;
        spaterPost_Reference.audioDistort.value = spaterAudioDistort;
        spaterPost_Reference.audioLookupSize.value = spaterAudioLookupSize;
        spaterPost_Reference.frameNoiseRotation.value = spaterFrameNoiseRotation;


        sketchEffect_Reference.paintMap.value = sketchPaintMap;
        sketchEffect_Reference.intensity.value = sketchIntensity;
        sketchEffect_Reference.scale.value = sketchScale;
        sketchEffect_Reference.changeSpeed.value = sketchChangeSpeed;

        sketchEffect_Reference._NoiseSpeed.value = sketchNoiseSpeed;
        sketchEffect_Reference._NoiseScale.value = sketchNoiseScale;
        sketchEffect_Reference._NoiseSampleRotation.value = sketchNoiseSampleRotation;
        sketchEffect_Reference._NoiseSampleRotationSize.value = sketchNoiseSampleRotationSize;
        sketchEffect_Reference._NoiseSampleChromaticSplit.value = sketchNoiseSampleChromaticSplit;
        sketchEffect_Reference._NoiseSampleOffset.value = sketchNoiseSampleOffset;


        sketchEffect_Reference._BorderSubtractor.value = sketchBorderSubtractor;
        sketchEffect_Reference._BorderMultiplier.value = sketchBorderMultiplier;
        sketchEffect_Reference._BorderNoiseAdder.value = sketchBorderNoiseAdder;
        sketchEffect_Reference._BorderColor.value = sketchBorderColor;


        dither_Reference.intensity.value = ditherIntensity;
        dither_Reference.pixelScale.value = ditherPixelScale;
        dither_Reference.pattern.value = ditherPattern;
        dither_Reference.primary.value = ditherPrimary;
        dither_Reference.secondary.value = ditherSecondary;
        dither_Reference.noiseIntensity.value = ditherNoiseIntensity;

        distanceFogEffect_Reference.FogStart.value = DistanceFogStart;
        distanceFogEffect_Reference.FogEnd.value = DistanceFogEnd;
        distanceFogEffect_Reference.FogAmount.value = DistanceFogAmount;
        distanceFogEffect_Reference.FogStartColor.value = DistanceFogStartColor;
        distanceFogEffect_Reference.FogEndColor.value = DistanceFogEndColor;
        distanceFogEffect_Reference.SkyboxImportance.value = DistanceFogSkyboxImportance;

        heatWaveEffect_Reference.HeatWaveStart.value = HeatWaveStart;
        heatWaveEffect_Reference.HeatWaveEnd.value = HeatWaveEnd;
        heatWaveEffect_Reference.WaveIntensity.value = HeatWaveIntensity;
        heatWaveEffect_Reference.WaveSize.value = HeatWaveSize;
        heatWaveEffect_Reference.WaveSpeed.value = HeatWaveSpeed;
        heatWaveEffect_Reference.HeatWaveAberration.value = HeatWaveAberration;

        lensFlareEffect_Reference.LensFlareIntensity.value = LensFlareIntensity;
        lensFlareEffect_Reference.LensFlareSpacing.value = LensFlareSpacing;
        lensFlareEffect_Reference.SunDogIntensity.value = SunDogIntensity;
        lensFlareEffect_Reference.SunDogSpacing.value = SunDogSpacing;


        if ( splatAmount < 1 ) {
            splatAmount = 1;
        }

        if ( splatAmount != placeParticlesOnDepthMap.splatAmount ) {
            placeParticlesOnDepthMap.splatAmount = splatAmount;
            placeParticlesOnDepthMap.Reset();
        }

        placeParticlesOnDepthMap.isActive = splatEffect;

        placeParticlesOnDepthMap.backgroundColor = splatsBackgroundColor;
        placeParticlesOnDepthMap.renderBackground = renderBackground;
        placeParticlesOnDepthMap.splatAmount = splatAmount;
        placeParticlesOnDepthMap.splatSize = splatSize;
        placeParticlesOnDepthMap.splatSpeed = splatSpeed;
        placeParticlesOnDepthMap.splatMatchAmount = splatMatchAmount;
        placeParticlesOnDepthMap.normalForce = normalForce;
        placeParticlesOnDepthMap.curlForce = curlForce;
        placeParticlesOnDepthMap.curlSize = curlSize;
        placeParticlesOnDepthMap.normalOffset = normalOffset;
        placeParticlesOnDepthMap.hueRandomness = splatHueRandomness;
        placeParticlesOnDepthMap.saturationRandomness = splatSaturationRandomness;
        placeParticlesOnDepthMap.lightnessRandomness = splatLightnessRandomness;
        placeParticlesOnDepthMap.colorMultiplier = splatColorMultiplier;

        placeParticlesOnDepthMap.splatTexture = splatTexture;
        placeParticlesOnDepthMap.splatTextureSize = splatTextureSize;

        placeParticlesOnDepthMap.splatDiscardColor = splatDiscardColor;
        placeParticlesOnDepthMap.splatDiscardCutoff = splatDiscardCutoff;


    }


    private Vector2 CartToPolar( Vector3 position )
    {

        float angle = Mathf.Atan2( position.x , position.z );
        float radius = new Vector2( position.x , position.z ).magnitude;


        float x = (position.x + 2048) / 4096;
        float y = (position.z + 2048) / 4096;

        var c = biomeMap.GetPixelBilinear( x , y , 0 );

        //        print( c.a);


        float h , s , v;

        Color.RGBToHSV( c , out h , out s , out v );

        mainHue = h;
        mainBlend = c.a;


        /* angle = (angle > 0 ? angle : (2*Mathf.PI + angle));
         angle /= 2 * Mathf.PI;

         _Hue = angle;
         _Hue += angleOffset;
         _Hue %= 1;


         print( _Hue );*/

        return new Vector2( angle , radius );

    }

    public void CopyTo( PostParameters p )
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
        p.sketchEffect = sketchEffect;
        p.distanceFog = distanceFog;
        p.dithered = dithered;
        p.heatWaveEffect = heatWaveEffect;
        p.lensFlareEffect = lensFlareEffect;

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
        p.hueShift = hueShift;
        p.saturation = saturation;
        p.brightness = brightness;
        p.contrast = contrast;
        p.postExposure = postExposure;
        p.lift = lift;
        p.gamma = gamma;
        p.gain = gain;


        p.vignetteIntensity = vignetteIntensity;
        p.vignetteSmoothness = vignetteSmoothness;
        p.vignetteRoundness = vignetteRoundness;
        p.vignetteColor = vignetteColor;
        p.vignetteRounded = vignetteRounded;

        p.lensDistortionIntensity = lensDistortionIntensity;
        p.lensDistortionScale = lensDistortionScale;

        p.chromaticAberrationIntensity = chromaticAberrationIntensity;

        p.fogIntensity = fogIntensity;
        p.fogLightToDarkMultiplier = fogLightToDarkMultiplier;
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
        p.splatsBackgroundColor = splatsBackgroundColor;
        p.splatAmount = splatAmount;
        p.splatSize = splatSize;
        p.splatSpeed = splatSpeed;
        p.normalForce = normalForce;
        p.curlForce = curlForce;
        p.curlSize = curlSize;
        p.splatMatchAmount = splatMatchAmount;
        p.normalOffset = normalOffset;
        p.splatHueRandomness = splatHueRandomness;
        p.splatSaturationRandomness = splatSaturationRandomness;
        p.splatLightnessRandomness = splatLightnessRandomness;
        p.splatColorMultiplier = splatColorMultiplier;
        p.splatTexture = splatTexture;
        p.splatTextureSize = splatTextureSize;
        p.splatDiscardColor = splatDiscardColor;
        p.splatDiscardCutoff = splatDiscardCutoff;


        p.ambientOcclusionIntensity = ambientOcclusionIntensity;
        p.ambientOcclusionColor = ambientOcclusionColor;

        p.spaterPostFade = spaterPostFade;
        p.spaterFrameTexture = spaterFrameTexture;
        p.spaterFrameNoiseTexture = spaterFrameNoiseTexture;
        p.spaterFrameColor = spaterFrameColor;
        p.spaterFrameNoiseScale = spaterFrameNoiseScale;
        p.spaterFrameNoiseChangeSpeed = spaterFrameNoiseChangeSpeed;
        p.spaterOverallMultiplier = spaterOverallMultiplier;
        p.spaterFrameNoiseWeight = spaterFrameNoiseWeight;
        p.spaterAudioPower = spaterAudioPower;
        p.spaterAudioBase = spaterAudioBase;
        p.spaterAudioDistort = spaterAudioDistort;
        p.spaterAudioLookupSize = spaterAudioLookupSize;
        p.spaterFrameNoiseRotation = spaterFrameNoiseRotation;


        p.astigmaIntensity = astigmaIntensity;
        p.astigmaScale = astigmaScale;
        p.astigmaCutoff = astigmaCutoff;
        p.astimgaAngle = astimgaAngle;
        p.astigmaNumSamples = astigmaNumSamples;
        p.astigmaNumDirections = astigmaNumDirections;
        p.astigmaUseTexture = astigmaUseTexture;
        p.astigmaBokehTexture = astigmaBokehTexture;
        p.astigmaColor = astigmaColor;

        p.sketchIntensity = sketchIntensity;
        p.sketchScale = sketchScale;
        p.sketchChangeSpeed = sketchChangeSpeed;
        p.sketchPaintMap = sketchPaintMap;
        p.sketchNoiseSpeed = sketchNoiseSpeed;
        p.sketchNoiseScale = sketchNoiseScale;
        p.sketchNoiseSampleRotation = sketchNoiseSampleRotation;
        p.sketchNoiseSampleRotationSize = sketchNoiseSampleRotationSize;
        p.sketchNoiseSampleChromaticSplit = sketchNoiseSampleChromaticSplit;
        p.sketchNoiseSampleOffset = sketchNoiseSampleOffset;
        p.sketchBorderSubtractor = sketchBorderSubtractor;
        p.sketchBorderMultiplier = sketchBorderMultiplier;
        p.sketchBorderNoiseAdder = sketchBorderNoiseAdder;
        p.sketchBorderColor = sketchBorderColor;


        p.ditherIntensity = ditherIntensity;
        p.ditherPixelScale = ditherPixelScale;
        p.ditherPattern = ditherPattern;
        p.ditherPrimary = ditherPrimary;
        p.ditherSecondary = ditherSecondary;
        p.ditherNoiseIntensity = ditherNoiseIntensity;

        p.DistanceFogStart = DistanceFogStart;
        p.DistanceFogEnd = DistanceFogEnd;
        p.DistanceFogStartColor = DistanceFogStartColor;
        p.DistanceFogEndColor = DistanceFogEndColor;
        p.DistanceFogAmount = DistanceFogAmount;
        p.DistanceFogSkyboxImportance = DistanceFogSkyboxImportance;

        p.HeatWaveIntensity = HeatWaveIntensity;
        p.HeatWaveStart = HeatWaveStart;
        p.HeatWaveEnd = HeatWaveEnd;
        p.HeatWaveSize = HeatWaveSize;
        p.HeatWaveSpeed = HeatWaveSpeed;
        p.HeatWaveAberration = HeatWaveAberration;

        p.LensFlareSpacing = LensFlareSpacing;
        p.LensFlareSpacing = LensFlareSpacing;
        p.SunDogIntensity = SunDogIntensity;
        p.SunDogSpacing = SunDogSpacing;


        // Sun/Moon settings
        p.sunAutoUpdate = sunAutoUpdate;
        p.daySpeed = daySpeed;
        p.nightSpeed = nightSpeed;
        p.sunAxis = sunAxis;
        p.startNormalizedPosition = startNormalizedPosition;
        p.sunGradient = sunGradient;
        p.moonGradient = moonGradient;

    }


    public void OnValidate()
    {

#if UNITY_EDITOR
        if ( UnityEditor.BuildPipeline.isBuildingPlayer ) {
            return;
        }
#endif
        God.postController.OnPostParametersValidate( this );
    }
}