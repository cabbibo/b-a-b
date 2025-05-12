using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using WrenUtils;
using Crest;


#if UNITY_EDITOR
using UnityEditor;


[CustomEditor( typeof(PostController) )]
public class PostControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {

        var myScript = (PostController)target;

        if ( GUILayout.Button( "Fade Out" ) ) {
            myScript.FadeOut();
        }

        if ( GUILayout.Button( "Fade In" ) ) {
            myScript.FadeIn();
        }

        if ( GUILayout.Button( "Glitch Hit" ) ) {
            myScript.GlitchHit();
        }

        if ( GUILayout.Button( "Worm Hole" ) ) {
            myScript.WormHole();
        }

        DrawDefaultInspector();


    }
}

#endif


[ExecuteAlways]
public class PostController : MonoBehaviour
{
    public PostParameters   currentPostParameterRef;
    public PostParameters   tmpPostParameters;
    public PostParameters[] postParameters;


    public PostProcessVolume volume;
    public VolumeProfile     profile;

    public MainPost mainPost_Reference;
    public Bloom    bloom_Reference;

    public ColorGrading        colorGrading_Reference;
    public Vignette            vignette_Reference;
    public LensDistortion      lensDistortion_Reference;
    public ChromaticAberration chromaticAberration_Reference;
    public FogEffect           fogEffect_Reference;
    public DepthOfField        depthOfField_Reference;

    public GlitchEffect glitchEffect_Reference;

    public AmbientOcclusion ambientOcclusion_Reference;

    public SpaterPostSettings spaterPost_Reference;
    public Astigma            astigma_Reference;

    public SketchEffect         sketchEffect_Reference;
    public QuickDither.Dithered dithered_Reference;

    // other controllers
    public CustomFog          customFog;
    public UnderwaterRenderer crestUnderwaterRenderer;

    public PlaceParticlesOnDepthMap placeParticlesOnDepthMap;

    public bool updateOnValidate = true;

    private void OnEnable()
    {
        volume = GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings( out mainPost_Reference );
        volume.profile.TryGetSettings( out bloom_Reference );
        volume.profile.TryGetSettings( out colorGrading_Reference );
        volume.profile.TryGetSettings( out vignette_Reference );
        volume.profile.TryGetSettings( out lensDistortion_Reference );
        volume.profile.TryGetSettings( out chromaticAberration_Reference );
        volume.profile.TryGetSettings( out fogEffect_Reference );
        volume.profile.TryGetSettings( out depthOfField_Reference );
        volume.profile.TryGetSettings( out glitchEffect_Reference );
        volume.profile.TryGetSettings( out ambientOcclusion_Reference );
        volume.profile.TryGetSettings( out spaterPost_Reference );
        volume.profile.TryGetSettings( out astigma_Reference );
        volume.profile.TryGetSettings( out sketchEffect_Reference );
        volume.profile.TryGetSettings( out dithered_Reference );


    }


    public float angleOffset;
    public float sizeToFullSaturation;


    public void Update()
    {


//        print(placeParticlesOnDepthMap);

        tmpPostParameters.SetValues(
            mainPost_Reference ,
            bloom_Reference ,
            colorGrading_Reference ,
            vignette_Reference ,
            lensDistortion_Reference ,
            chromaticAberration_Reference ,
            fogEffect_Reference ,
            depthOfField_Reference ,
            glitchEffect_Reference ,
            ambientOcclusion_Reference ,
            spaterPost_Reference ,
            astigma_Reference ,
            sketchEffect_Reference ,
            dithered_Reference ,
            placeParticlesOnDepthMap
        );

        if ( tmpPostParameters.splatEffect ) {
            placeParticlesOnDepthMap.enabled = true;

            if ( tmpPostParameters.renderBackground ) {
                LayerMask everything = ~0;
                God.camera.cullingMask = everything;
            } else {

                LayerMask debug = 1 << LayerMask.NameToLayer( "Splats" );
                God.camera.cullingMask = debug;
            }
        } else {
            placeParticlesOnDepthMap.enabled = false;

            LayerMask everything = ~0;
            God.camera.cullingMask = everything;
        }


    }


    public void SetPostParameters( string name )
    {
        foreach (var p in postParameters)
            if ( p.name == name ) {
                currentPostParameterRef = p;
                p.CopyTo( tmpPostParameters );
                return;
            }
    }

    public void SetPostParameters( PostParameters p )
    {
        currentPostParameterRef = p;
        p.CopyTo( tmpPostParameters );
    }


    public void OnPostParametersValidate( PostParameters p )
    {
//        Debug.Log("OnPostParametersValidate");
        if ( !updateOnValidate ) {
            return;
        }

        if ( p == null ) {
            return;
        }

        if ( p != currentPostParameterRef ) {
            return;
        }

//   print("Made it here");
        p.CopyTo( tmpPostParameters );
    }


    public void FadeOut()
    {
        StartCoroutine( DoFadeOut() );
    }

    public void FadeIn()
    {
        StartCoroutine( DoFadeIn() );
    }

    public float fadeInSpeed  = 1;
    public float fadeOutSpeed = 1;

    public void SetFade( float f )
    {

        mainPost_Reference._Fade.value = f;

    }

    private IEnumerator DoFadeOut()
    {
        float t = 0;

        while (t < 1) {
            t += .03f * fadeOutSpeed;
            SetFade( t );
            yield return null;
        }
    }

    private IEnumerator DoFadeIn()
    {
        float t = 1;

        while (t > 0) {
            t -= .03f * fadeInSpeed;
            SetFade( t );
            yield return null;
        }
    }


    [Space( 50 )]
    [Header( "Post Processing Effects" )]
    public float glitchSpeed;

    public void GlitchHit()
    {
        StartCoroutine( DoGlitchHit() );
    }

    private IEnumerator DoGlitchHit()
    {
        bool tmpGlitch = tmpPostParameters.glitchEffect;

        tmpPostParameters.glitchEffect = true;
        tmpPostParameters.glitchIntensity = 0;

        float t = 0;

        while (t < 1) {
            t += .03f * 1;
            tmpPostParameters.glitchIntensity = t;
            yield return null;
        }


        while (t > 0) {
            t -= .03f * 1;
            tmpPostParameters.glitchIntensity = t;
            yield return null;
        }

        tmpPostParameters.glitchEffect = tmpGlitch;

    }


    public void WormHole()
    {
        emptyDelegate = null;
        emptyDelegate2 = null;
        StartCoroutine( DoWormHole() );
    }

    public void WormHole( EmptyDelegate ed )
    {
        emptyDelegate = ed;
        emptyDelegate2 = null;
        StartCoroutine( DoWormHole() );
    }

    public void WormHole( EmptyDelegate ed , EmptyDelegate ed2 )
    {
        emptyDelegate = ed;
        emptyDelegate2 = ed2;
        StartCoroutine( DoWormHole() );
    }

    public float maxLensDistortion      = -20;
    public float maxChromaticAberration = 20;

    public float maxBloomIntensity = 100;
    public float maxBloomThreshold = .1f;

    public Vector2 wormHoleSpeed = new(.03f , .1f);

    public delegate void EmptyDelegate();

    public EmptyDelegate emptyDelegate;
    public EmptyDelegate emptyDelegate2;

    public void test()
    {
        print( "hiiiiiiiii" );
    }

    private IEnumerator DoWormHole()
    {


        print( "hi9ii" );
        bool tmpChromaticAberration = tmpPostParameters.chromaticAberration;
        float tmpChromaticAberrationIntensity = tmpPostParameters.chromaticAberrationIntensity;

        bool tmpLensDistortion = tmpPostParameters.lensDistortion;
        float tmpLensDistortionIntensity = tmpPostParameters.lensDistortionIntensity;

        bool tmpBloom = tmpPostParameters.bloom;
        float tmpBloomIntensity = tmpPostParameters.bloomIntensity;
        float tmpBloomThreshold = tmpPostParameters.bloomThreshold;


        float startingChromaticIntensity = 0;

        if ( tmpChromaticAberration ) {

            startingChromaticIntensity = tmpChromaticAberrationIntensity;
        }

        float startingLensIntensity = 0;

        if ( tmpLensDistortion ) {
            startingLensIntensity = tmpLensDistortionIntensity;
        }

        float startingBloomIntensity = 0;
        float startingBloomThreshold = 1;

        if ( tmpBloom ) {
            startingBloomIntensity = tmpBloomIntensity;
            startingBloomThreshold = tmpBloomThreshold;
        }


        tmpPostParameters.chromaticAberration = true;
        tmpPostParameters.lensDistortion = true;

        tmpPostParameters.chromaticAberrationIntensity = startingChromaticIntensity;
        tmpPostParameters.lensDistortionIntensity = startingLensIntensity;

        tmpPostParameters.bloom = true;
        tmpPostParameters.bloomIntensity = startingBloomIntensity;
        tmpPostParameters.bloomThreshold = startingBloomThreshold;


        float val = startingChromaticIntensity / maxChromaticAberration;


        float t = val;

        while (t < 1) {

            t += wormHoleSpeed.x * 1;

            float ft = t * t;
            tmpPostParameters.chromaticAberrationIntensity = Mathf.Lerp( 0 , maxChromaticAberration , ft );
            tmpPostParameters.lensDistortionIntensity = Mathf.Lerp( 0 , maxLensDistortion , ft * t );

            tmpPostParameters.bloomIntensity = Mathf.Lerp( startingBloomIntensity , maxBloomIntensity , ft * ft * ft );
            tmpPostParameters.bloomThreshold = Mathf.Lerp( startingBloomThreshold , maxBloomThreshold , ft * ft );

            yield return null;
        }


        if ( emptyDelegate != null ) {
            emptyDelegate();
        }

        while (t > 0) {
            t -= wormHoleSpeed.y * 1;

            float ft = t * t;
            tmpPostParameters.chromaticAberrationIntensity = Mathf.Lerp( startingChromaticIntensity , maxChromaticAberration , ft );
            tmpPostParameters.lensDistortionIntensity = Mathf.Lerp( startingLensIntensity , maxLensDistortion , ft * t );


            tmpPostParameters.bloomIntensity = Mathf.Lerp( startingBloomIntensity , maxBloomIntensity , ft * ft * ft );
            tmpPostParameters.bloomThreshold = Mathf.Lerp( startingBloomThreshold , maxBloomThreshold , ft * ft );
            yield return null;
        }

        if ( emptyDelegate2 != null ) {
            emptyDelegate2();
        }


    }
}