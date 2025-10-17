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

        if ( GUILayout.Button( "Reset Clones" ) ) {
            my.ResetClones();
        }

        EditorGUILayout.Space();

        DrawDefaultInspector();

        if ( GUI.changed ) {
            // Keep Edit-Mode preview snappy without mutating assets
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

    [Header( "Optional: Template used only to seed working instance if no current preset is set" )]
    public PostParameters initialTemplateForRuntime;

    [Header( "Apply/Authoring Behavior" )]
    public bool updateOnValidate = true;

    // INTERNAL: runtime-only working copy (never an asset)
    [HideInInspector]
    public PostParameters workingInstance;

    // Post-process plumbing
    public PostProcessVolume  volume;
    public PostProcessProfile profile;

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

    // other controllers
    public CustomFog                customFog;
    public UnderwaterRenderer       crestUnderwaterRenderer;
    public PlaceParticlesOnDepthMap placeParticlesOnDepthMap;

    public float angleOffset;
    public float sizeToFullSaturation;

    // Fade/Glitch/Wormhole params (unchanged)
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

    public void ResetClones()
    {
        if ( volume == null || volume.profile == null ) {
            return;
        }

        var originalProfile = volume.profile;

        // Force Unity to tear down internal cached states
        volume.profile = null;
        volume.profile = originalProfile;

        // If you're holding a runtime clone, reset it too
        if ( workingInstance != null ) {
            DestroyImmediate( workingInstance );
            workingInstance = Instantiate( currentPostParameterRef );
        }

        Debug.Log( "✅ PostProcessing clones cleared and instances reset." );
    }


    // ----------------------------------------------------
// Clone-proof setting cache — lock first instance only
// ----------------------------------------------------
    private T CachePersistentSetting<T>( ref T field ) where T : PostProcessEffectSettings
    {
        if ( field != null ) {
            return field; // Already locked
        }

        if ( profile != null && profile.TryGetSettings( out T found ) ) {
            field = found; // Lock Unity's first given reference
            return field;
        }

        return null;
    }

    // ----------------------------------------------------
    // LIFECYCLE
    // ----------------------------------------------------
    private void OnEnable()
    {
        // Cache volume/settings
        if ( volume == null ) {
            volume = GetComponent<PostProcessVolume>();
        }


        if ( volume.profile != null ) {

            profile = volume.profile;
            // profile.TryGetSettings( out mainPost_Reference );
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
        }

        EnsureWorkingInstanceInitialized();
        // Initial push so Edit Mode shows something
        SafeApplyToPipeline();
    }

    private void OnDisable()
    {
        DestroyWorkingInstance();
    }

    private void Update()
    {
        // Keep the pipeline driven by the working instance
        SafeApplyToPipeline();

        // Handle splat / camera culling based on working values
        if ( workingInstance != null && placeParticlesOnDepthMap != null ) {
            if ( workingInstance.splatEffect ) {
                placeParticlesOnDepthMap.enabled = true;

                if ( workingInstance.renderBackground ) {
                    LayerMask everything = ~0;

                    if ( God.camera != null ) {
                        God.camera.cullingMask = everything;
                    }
                } else {
                    LayerMask debug = 1 << LayerMask.NameToLayer( "Splats" );

                    if ( God.camera != null ) {
                        God.camera.cullingMask = debug;
                    }
                }
            } else {
                placeParticlesOnDepthMap.enabled = false;
                LayerMask everything = ~0;

                if ( God.camera != null ) {
                    God.camera.cullingMask = everything;
                }
            }
        }
    }

    // ----------------------------------------------------
    // WORKING INSTANCE MANAGEMENT
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
            Debug.LogWarning( "PostController: No PostParameters seed available to create working instance." );

            if ( workingInstance == null ) {
                workingInstance = ScriptableObject.CreateInstance<PostParameters>();
            }

            return;
        }

        workingInstance = Instantiate( seed );
        MarkRuntimeOnly( workingInstance );
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

    private void MarkRuntimeOnly( PostParameters p )
    {
#if UNITY_EDITOR
        if ( p != null ) {
            p.name = $"(runtime) {p.name}";
        }
#endif
    }

    // Non-destructive refresh when user tweaks the selected preset reference
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

        // Copy values into the working instance (do not replace reference so coroutines keep the same target)
        EnsureWorkingInstanceInitialized();
        currentPostParameterRef.CopyTo( workingInstance );
    }

    // ----------------------------------------------------
    // APPLY TO PIPELINE
    // ----------------------------------------------------
    private void SafeApplyToPipeline()
    {
        if ( workingInstance == null ) {
            return;
        }

        if ( volume.profile == null ) {
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
            placeParticlesOnDepthMap
        );
    }

    // ----------------------------------------------------
    // PRESET SELECTION (OLD BEHAVIOR: INSTANT SWITCH)
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

        // Old behavior: instant swap — copy values into existing working instance
        EnsureWorkingInstanceInitialized();
        p.CopyTo( workingInstance );

        // Immediate apply so it reflects this frame (both Play and Edit modes)
        SafeApplyToPipeline();
    }

    // ----------------------------------------------------
    // VALIDATION HOOK (from PostParameters.OnValidate -> God.postController.OnPostParametersValidate)
    // Only used during Play (matches your original)
    // ----------------------------------------------------
    public void OnPostParametersValidate( PostParameters asset )
    {
        if ( !updateOnValidate || asset == null ) {
            return;
        }

        if ( asset != currentPostParameterRef ) {
            return;
        }

        // Copy current preset values into working instance (non-destructive to asset)
        EnsureWorkingInstanceInitialized();
        asset.CopyTo( workingInstance );
        SafeApplyToPipeline();
    }

    // ----------------------------------------------------
    // EFFECTS (now target the workingInstance only)
    // ----------------------------------------------------
    public void SetFade( float f )
    {
        if ( mainPost_Reference != null ) {
            mainPost_Reference._Fade.value = f;
        }

        // Keep working copy in sync so future blends start from the visual state
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
            t += .03f * 1f;
            workingInstance.glitchIntensity = t;
            SafeApplyToPipeline();
            yield return null;
        }

        while (t > 0f) {
            t -= .03f * 1f;
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

        float val = maxChromaticAberration != 0f ? startCA / maxChromaticAberration : 0f;
        float t = val;

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

        if ( emptyDelegate != null ) {
            emptyDelegate();
        }

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

        if ( emptyDelegate2 != null ) {
            emptyDelegate2();
        }
    }

    // ----------------------------------------------------
    // UTIL
    // ----------------------------------------------------
    public void test()
    {
        Debug.Log( "hiiiiiiiii" );
    }
}