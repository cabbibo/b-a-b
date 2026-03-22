using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using WrenUtils;
using Crest;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor( typeof(PostController) )]
public class PostControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var my = (PostController)target;

        if ( GUILayout.Button( "Fade Out" ) ) {
            my.FadeOut();
        }

        if ( GUILayout.Button( "Fade In" ) ) {
            my.FadeIn();
        }

        if ( GUILayout.Button( "Glitch Hit" ) ) {
            my.GlitchHit();
        }

        if ( GUILayout.Button( "Worm Hole" ) ) {
            my.WormHole();
        }

        if ( GUILayout.Button( "ClearCache" ) ) {
            my.ClearCachedReferences();
        }

        EditorGUILayout.Space();
        DrawDefaultInspector();

        if ( GUI.changed ) {
            my.ForceRefreshWorkingInstanceFromCurrentRef( true );
        }
    }
}
#endif

[ExecuteAlways]
public class PostController : MonoBehaviour
{
    [Header( "Preset References (immutable presets)" )]
    public PostParameters currentPostParameterRef;

    public PostParameters[] postParameters;
    public PostParameters   initialTemplateForRuntime;

    [HideInInspector]
    public PostParameters workingInstance;

    public bool updateOnValidate = true;

    public PostProcessVolume  volume;
    public PostProcessProfile profile;

    // Post-processing effect refs
    public MainPost             mainPost_Reference;
    public Bloom                bloom_Reference;
    public ColorGrading         colorGrading_Reference;
    public Vignette             vignette_Reference;
    public LensDistortion       lensDistortion_Reference;
    public ChromaticAberration  chromaticAberration_Reference;
    public FogEffect            fogEffect_Reference;
    public DepthOfField         depthOfField_Reference;
    public GlitchEffect         glitchEffect_Reference;
    public AmbientOcclusion     ambientOcclusion_Reference;
    public SpaterPostSettings   spaterPost_Reference;
    public Astigma              astigma_Reference;
    public SketchEffect         sketchEffect_Reference;
    public QuickDither.Dithered dithered_Reference;
    public DistanceFogEffect    distanceFogEffect_Reference;
    public HeatWaveEffect       heatWaveEffect_Reference;
    public LensFlareEffect      lensFlareEffect_Reference;


    // Other controllers
    public CustomFog                customFog;
    public UnderwaterRenderer       crestUnderwaterRenderer;
    public PlaceParticlesOnDepthMap placeParticlesOnDepthMap;
    public PushableCloudsPost       pushableCloudsPost;

    public float fadeInSpeed  = 1;
    public float fadeOutSpeed = 1;

    public float   glitchSpeed;
    public float   maxLensDistortion      = -20;
    public float   maxChromaticAberration = 20;
    public float   maxBloomIntensity      = 100;
    public float   maxBloomThreshold      = .1f;
    public Vector2 wormHoleSpeed          = new(.03f , .1f);

    public delegate void EmptyDelegate();

    public EmptyDelegate emptyDelegate;
    public EmptyDelegate emptyDelegate2;

    public void ClearCachedReferences()
    {
        mainPost_Reference = null;
        bloom_Reference = null;
        colorGrading_Reference = null;
        vignette_Reference = null;
        lensDistortion_Reference = null;
        chromaticAberration_Reference = null;
        fogEffect_Reference = null;
        depthOfField_Reference = null;
        glitchEffect_Reference = null;
        ambientOcclusion_Reference = null;
        spaterPost_Reference = null;
        astigma_Reference = null;
        sketchEffect_Reference = null;
        dithered_Reference = null;
        distanceFogEffect_Reference = null;
        heatWaveEffect_Reference = null;
        lensFlareEffect_Reference = null;

        // Reset the working instance too (optional)
        DestroyWorkingInstance();

        // Force Unity to flush cached PostProcessing settings
        if ( volume != null ) {
            var originalProfile = volume.sharedProfile;
            volume.sharedProfile = null;
            volume.sharedProfile = originalProfile;
        }

        Debug.Log( "🔄 PostController: Cached references cleared. They will re-bind on next OnEnable()." );
    }

    // ----------------------------------------------------
    // Clone-proof cache helper
    // ----------------------------------------------------
    private T CachePersistentSetting<T>( ref T field ) where T : PostProcessEffectSettings
    {
        if ( field != null ) {
            return field;
        }

        if ( profile != null && profile.TryGetSettings( out T found ) ) {
            field = found;
            return field;
        }

        return null;
    }

    // ----------------------------------------------------
    // Lifecycle
    // ----------------------------------------------------
    private void OnEnable()
    {
        if ( volume == null ) {
            volume = GetComponent<PostProcessVolume>();
        }

        if ( volume == null || volume.sharedProfile == null ) {
            Debug.LogWarning( "PostController: No PostProcessVolume or Profile found." );
            return;
        }

        profile = volume.sharedProfile;

        // Lock first-found settings only once
        CachePersistentSetting( ref mainPost_Reference );
        CachePersistentSetting( ref bloom_Reference );
        CachePersistentSetting( ref colorGrading_Reference );
        CachePersistentSetting( ref vignette_Reference );
        CachePersistentSetting( ref lensDistortion_Reference );
        CachePersistentSetting( ref chromaticAberration_Reference );
        CachePersistentSetting( ref fogEffect_Reference );
        CachePersistentSetting( ref depthOfField_Reference );
        CachePersistentSetting( ref glitchEffect_Reference );
        CachePersistentSetting( ref ambientOcclusion_Reference );
        CachePersistentSetting( ref spaterPost_Reference );
        CachePersistentSetting( ref astigma_Reference );
        CachePersistentSetting( ref sketchEffect_Reference );
        CachePersistentSetting( ref dithered_Reference );
        CachePersistentSetting( ref distanceFogEffect_Reference );
        CachePersistentSetting( ref heatWaveEffect_Reference );
        CachePersistentSetting( ref lensFlareEffect_Reference );

        EnsureWorkingInstanceInitialized();
        SafeApplyToPipeline();
    }

    private void OnDisable()
    {
        DestroyWorkingInstance();
    }

    private void Update()
    {
        SafeApplyToPipeline();
    }

    // ----------------------------------------------------
    // Working-instance management
    // ----------------------------------------------------
    private void EnsureWorkingInstanceInitialized()
    {
        if ( workingInstance != null && !IsAsset( workingInstance ) ) {
            return;
        }

        var seed = currentPostParameterRef != null
            ? currentPostParameterRef
            : initialTemplateForRuntime != null
                ? initialTemplateForRuntime
                : postParameters != null && postParameters.Length > 0
                    ? postParameters[0]
                    : null;

        if ( seed == null ) {
            Debug.LogWarning( "PostController: No PostParameters seed found." );
            workingInstance = ScriptableObject.CreateInstance<PostParameters>();
            return;
        }

        workingInstance = Instantiate( seed );
#if UNITY_EDITOR
        workingInstance.name = "(runtime) " + seed.name;
#endif
    }

    private void DestroyWorkingInstance()
    {
        if ( workingInstance == null ) {
            return;
        }

        if ( Application.isPlaying ) {
            Destroy( workingInstance );
        } else {
            DestroyImmediate( workingInstance );
        }

        workingInstance = null;
    }

    private bool IsAsset( PostParameters p )
    {
#if UNITY_EDITOR
        return p != null && AssetDatabase.Contains( p );
#else
        return false;
#endif
    }

    public void ForceRefreshWorkingInstanceFromCurrentRef( bool editModeOnly = false )
    {
#if UNITY_EDITOR
        if ( editModeOnly && Application.isPlaying ) {
            return;
        }
#endif
        if ( currentPostParameterRef == null ) {
            return;
        }

        EnsureWorkingInstanceInitialized();
        currentPostParameterRef.CopyTo( workingInstance );
    }

    // ----------------------------------------------------
    // Apply parameters
    // ----------------------------------------------------
    private void SafeApplyToPipeline()
    {
        if ( workingInstance == null || profile == null ) {
            return;
        }

        workingInstance.SetValues(
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
            distanceFogEffect_Reference ,
            heatWaveEffect_Reference ,
            lensFlareEffect_Reference ,
            placeParticlesOnDepthMap
        );
    }

    // ----------------------------------------------------
    // Preset selection (instant switch)
    // ----------------------------------------------------
    public void SetPostParameters( string name )
    {
        if ( postParameters == null ) {
            return;
        }

        foreach (var p in postParameters)
            if ( p != null && p.name == name ) {
                SetPostParameters( p );
                return;
            }

        Debug.LogWarning( $"PostController: No PostParameters named '{name}' found." );
    }

    public void SetPostParameters( PostParameters p )
    {
        if ( p == null ) {
            Debug.LogWarning( "PostController: SetPostParameters called with null." );
            return;
        }

        currentPostParameterRef = p;
        EnsureWorkingInstanceInitialized();
        p.CopyTo( workingInstance );
        SafeApplyToPipeline();
        ApplySunSettings( p );
    }

    private void ApplySunSettings( PostParameters p )
    {


        if ( p == null ) {
            return;
        }

        var sunManager = God.weatherManager?.sunManager;

        if ( sunManager == null ) {
            return;
        }

        sunManager.auto = p.sunAutoUpdate;
        sunManager.daySpeed = p.daySpeed;
        sunManager.nightSpeed = p.nightSpeed;
        sunManager.dayColor = p.sunGradient;
        sunManager.nightColor = p.moonGradient;
        sunManager.sunAxis = p.sunAxis;

        // Set start position: convert normalized [0,1] to raw time in cycle
        float totalCycleLength = p.daySpeed + p.nightSpeed;

        print( p.startNormalizedPosition );
        sunManager.rawTimeInCycle = p.startNormalizedPosition * totalCycleLength;
    }

    // ----------------------------------------------------
    // Validation hook
    // ----------------------------------------------------
    public void OnPostParametersValidate( PostParameters asset )
    {


        if ( !updateOnValidate || asset == null ) {
            return;
        }

        if ( asset != currentPostParameterRef ) {
            return;
        }

        EnsureWorkingInstanceInitialized();
        asset.CopyTo( workingInstance );
        SafeApplyToPipeline();
    }

    // ----------------------------------------------------
    // Fade / Glitch / Wormhole effects
    // ----------------------------------------------------
    public void SetFade( float f )
    {
        if ( mainPost_Reference != null ) {
            mainPost_Reference._Fade.value = f;
        }

        if ( workingInstance != null ) {
            workingInstance.mainFade = f;
        }
    }

    public void FadeOut()
    {
        StartCoroutine( DoFadeOut() );
    }

    public void FadeIn()
    {
        StartCoroutine( DoFadeIn() );
    }

    private IEnumerator DoFadeOut()
    {
        EnsureWorkingInstanceInitialized();
        float t = workingInstance != null ? workingInstance.mainFade : 0f;

        while (t < 1f) {
            t += .03f * fadeOutSpeed;
            SetFade( t );
            yield return null;
        }

        SetFade( 1f );
    }

    private IEnumerator DoFadeIn()
    {
        EnsureWorkingInstanceInitialized();
        float t = workingInstance != null ? workingInstance.mainFade : 1f;

        while (t > 0f) {
            t -= .03f * fadeInSpeed;
            SetFade( t );
            yield return null;
        }

        SetFade( 0f );
    }

    public void GlitchHit()
    {
        StartCoroutine( DoGlitchHit() );
    }

    private IEnumerator DoGlitchHit()
    {
        EnsureWorkingInstanceInitialized();

        if ( workingInstance == null ) {
            yield break;
        }

        bool tmpGlitch = workingInstance.glitchEffect;
        float startInt = workingInstance.glitchIntensity;

        workingInstance.glitchEffect = true;
        workingInstance.glitchIntensity = 0;

        float t = 0;

        while (t < 1f) {
            t += .03f;
            workingInstance.glitchIntensity = t;
            SafeApplyToPipeline();
            yield return null;
        }

        while (t > 0f) {
            t -= .03f;
            workingInstance.glitchIntensity = t;
            SafeApplyToPipeline();
            yield return null;
        }

        workingInstance.glitchEffect = tmpGlitch;
        workingInstance.glitchIntensity = startInt;
        SafeApplyToPipeline();
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

    private IEnumerator DoWormHole()
    {
        EnsureWorkingInstanceInitialized();

        if ( workingInstance == null ) {
            yield break;
        }

        bool tmpCA = workingInstance.chromaticAberration;
        float tmpCA_I = workingInstance.chromaticAberrationIntensity;
        bool tmpLD = workingInstance.lensDistortion;
        float tmpLD_I = workingInstance.lensDistortionIntensity;
        bool tmpB = workingInstance.bloom;
        float tmpB_I = workingInstance.bloomIntensity;
        float tmpB_T = workingInstance.bloomThreshold;

        float startCA = tmpCA ? tmpCA_I : 0f;
        float startLD = tmpLD ? tmpLD_I : 0f;
        float startBI = tmpB ? tmpB_I : 0f;
        float startBT = tmpB ? tmpB_T : 1f;

        workingInstance.chromaticAberration = true;
        workingInstance.lensDistortion = true;
        workingInstance.bloom = true;
        workingInstance.chromaticAberrationIntensity = startCA;
        workingInstance.lensDistortionIntensity = startLD;
        workingInstance.bloomIntensity = startBI;
        workingInstance.bloomThreshold = startBT;

        float t = startCA / (maxChromaticAberration != 0f ? maxChromaticAberration : 1f);

        while (t < 1f) {
            t += wormHoleSpeed.x;
            float ft = t * t;
            workingInstance.chromaticAberrationIntensity = Mathf.Lerp( 0 , maxChromaticAberration , ft );
            workingInstance.lensDistortionIntensity = Mathf.Lerp( 0 , maxLensDistortion , ft * t );
            workingInstance.bloomIntensity = Mathf.Lerp( startBI , maxBloomIntensity , ft * ft * ft );
            workingInstance.bloomThreshold = Mathf.Lerp( startBT , maxBloomThreshold , ft * ft );
            SafeApplyToPipeline();
            yield return null;
        }

        emptyDelegate?.Invoke();

        while (t > 0f) {
            t -= wormHoleSpeed.y;
            float ft = t * t;
            workingInstance.chromaticAberrationIntensity = Mathf.Lerp( startCA , maxChromaticAberration , ft );
            workingInstance.lensDistortionIntensity = Mathf.Lerp( startLD , maxLensDistortion , ft * t );
            workingInstance.bloomIntensity = Mathf.Lerp( startBI , maxBloomIntensity , ft * ft * ft );
            workingInstance.bloomThreshold = Mathf.Lerp( startBT , maxBloomThreshold , ft * ft );
            SafeApplyToPipeline();
            yield return null;
        }

        emptyDelegate2?.Invoke();
    }

    // ----------------------------------------------------
    public void test()
    {
        Debug.Log( "hiiiiiiiii" );
    }
}