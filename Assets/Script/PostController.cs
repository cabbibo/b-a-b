using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

using WrenUtils;
using Crest;

[ExecuteAlways]
public class PostController : MonoBehaviour
{


    public PostProcessVolume volume;
    private VolumeProfile profile;


    private MainPost mainPost_Reference;
    private Bloom bloom_Reference;
    private ColorGrading colorGrading_Reference;
    private Vignette vignette_Reference;
    private LensDistortion lensDistortion_Reference;
    private ChromaticAberration chromaticAberration_Reference;
    private FogEffect fogEffect_Reference;
    private DepthOfField depthOfField_Reference;

    private GlitchEffect glitchEffect_Reference;

    private AmbientOcclusion ambientOcclusion_Reference;

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



    // other controllers
    public CustomFog customFog;
    public UnderwaterRenderer crestUnderwaterRenderer;

    public PlaceParticlesOnDepthMap placeParticlesOnDepthMap;


    [Header("Main Post Settings")]

    public float _Hue;
    public float _Saturation;
    public float _Lightness;
    public float _Blend;
    public float _Fade;

    public Texture2D biomeMap;



    [Header("Depth Of Field Settings")]

    public float depthOfFieldFocusDistance;




    [Header("Fog Settings")]
    public float fogIntensity;
    public float fogHeightPower;

    [Header("Color Grading Settings")]
    public Color colorFilter;


    [Header("Vignette Settings")]
    public float vignetteIntensity;


    [Header("Bloom Settings")]
    public float bloomIntensity;
    public float bloomThreshold;

    [Header("Lens Distortion Settings")]
    public float lensDistortionIntensity;
    public float lensDistortionScale;

    [Header("Chromatic Aberration Settings")]
    public float chromaticAberrationIntensity;

    [Header("Depth Of Field Settings")]
    public float depthOfFieldAperture;
    public float depthOfFieldFocalLength;
    public bool focusOnWren;

    [Header("Glitch Settings")]
    public float glitchIntensity;

    [Header("Splat Settings")]
    public bool renderBackground;
    public float splatsAmount;
    public float splatSize;

    public float splatSpeed;

    [Header("Ambient Occlusion Settings")]
    public float ambientOcclusionIntensity;
    public Color ambientOcclusionColor;







    void OnEnable()
    {
        volume = GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings(out mainPost_Reference);
        volume.profile.TryGetSettings(out bloom_Reference);
        volume.profile.TryGetSettings(out colorGrading_Reference);
        volume.profile.TryGetSettings(out vignette_Reference);
        volume.profile.TryGetSettings(out lensDistortion_Reference);
        volume.profile.TryGetSettings(out chromaticAberration_Reference);
        volume.profile.TryGetSettings(out fogEffect_Reference);
        volume.profile.TryGetSettings(out depthOfField_Reference);
        volume.profile.TryGetSettings(out glitchEffect_Reference);
        volume.profile.TryGetSettings(out ambientOcclusion_Reference);


    }


    public float angleOffset;
    public float sizeToFullSaturation;



    public void Update()
    {

        if (God.wren != null)
        {
            CartToPolar(God.wren.transform.position);
        }


        //        print(post);
        mainPost_Reference._Hue.value = _Hue;
        mainPost_Reference._Saturation.value = _Saturation;
        mainPost_Reference._Lightness.value = _Lightness;
        mainPost_Reference._Blend.value = _Blend;
        mainPost_Reference._Fade.value = _Fade;

        if (God.wren != null)
        {
            depthOfFieldFocusDistance = Vector3.Distance(God.wren.transform.position, God.camera.transform.position);
        }
        // todo if we are focusing on something else make that be the focus object ( even better make them both be in focus)
        depthOfField_Reference.focusDistance.value = depthOfFieldFocusDistance;



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

        if (splatEffect)
        {
            placeParticlesOnDepthMap.enabled = true;
            if (renderBackground)
            {
                LayerMask everything = ~0;
                God.camera.cullingMask = everything;
            }
            else
            {

                LayerMask debug = (1 << LayerMask.NameToLayer("Splats"));
                //print(debug);
                God.camera.cullingMask = debug;
            }
        }
        else
        {
            placeParticlesOnDepthMap.enabled = false;

            LayerMask everything = ~0;
            God.camera.cullingMask = everything;
        }

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

        _Hue = h;
        _Blend = c.a;



        /* angle = (angle > 0 ? angle : (2*Mathf.PI + angle));
         angle /= 2 * Mathf.PI;

         _Hue = angle;
         _Hue += angleOffset;
         _Hue %= 1;


         print( _Hue );*/

        return new Vector2(angle, radius);

    }


    public void FadeOut()
    {
        StartCoroutine(DoFadeOut());
    }

    public void FadeIn()
    {
        StartCoroutine(DoFadeIn());
    }

    public float fadeInSpeed = 1;
    public float fadeOutSpeed = 1;

    IEnumerator DoFadeOut()
    {
        float t = 0;
        while (t < 1)
        {
            t += .03f * fadeOutSpeed;
            _Fade = t;
            yield return null;
        }
    }

    IEnumerator DoFadeIn()
    {
        float t = 1;
        while (t > 0)
        {
            t -= .03f * fadeInSpeed;
            _Fade = t;
            yield return null;
        }
    }



}
