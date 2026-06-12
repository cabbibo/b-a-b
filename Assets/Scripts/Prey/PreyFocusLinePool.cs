using UnityEngine;
using System.Collections.Generic;

// Shared pool of focus-line LineRenderers.
//
// Previously EVERY bird carried its own LineRenderer component + its own
// Material instance (created in PreyController.Initialize) and toggled it every
// frame — even though only the handful of birds within `focusRadius` of the wren
// ever draw one. With hundreds of birds that's hundreds of components + material
// instances sitting idle.
//
// Now birds borrow a LineRenderer from this pool only while in focus and return
// it when they leave focus. A single shared material is reused across all lines;
// per-bird shader values (_BirdPos / _EatRadius) ride on a MaterialPropertyBlock
// so no per-bird material instance is allocated.
//
// Zero config: the first bird to ask lazily creates this object (same pattern as
// PreyRaycastBatcher).
public class PreyFocusLinePool : MonoBehaviour
{
    private static PreyFocusLinePool _instance;
    private static bool _quitting;

    public static bool HasInstance => _instance != null;

    public static PreyFocusLinePool Instance
    {
        get {
            if ( _instance == null && !_quitting && Application.isPlaying ) {
                var go = new GameObject( "PreyFocusLinePool" );
                _instance = go.AddComponent<PreyFocusLinePool>();
            }
            return _instance;
        }
    }

    private readonly Stack<LineRenderer> _free = new Stack<LineRenderer>();
    private Material _sharedMat;
    private int      _created;

    private void Awake()
    {
        if ( _instance != null && _instance != this ) { Destroy( this ); return; }
        _instance = this;

        var shader = Shader.Find( "Prey/FocusLine" );
        if ( shader != null ) _sharedMat = new Material( shader );
    }

    private void OnApplicationQuit() => _quitting = true;

    // Borrow an enabled LineRenderer. Returns null only if the focus shader is missing.
    public LineRenderer Acquire()
    {
        if ( _sharedMat == null ) return null;

        LineRenderer lr = _free.Count > 0 ? _free.Pop() : CreateLine();
        if ( lr != null ) lr.enabled = true;
        return lr;
    }

    public void Release( LineRenderer lr )
    {
        if ( lr == null ) return;
        lr.enabled = false;
        _free.Push( lr );
    }

    private LineRenderer CreateLine()
    {
        var go = new GameObject( "PreyFocusLine_" + _created++ );
        go.transform.SetParent( transform , false );

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount       = 2;
        lr.startWidth          = 0.15f;
        lr.endWidth            = 0.04f;
        lr.useWorldSpace       = true;
        lr.startColor          = new Color( 1f , 1f , 1f , 0.9f );
        lr.endColor            = new Color( 1f , 1f , 1f , 0.2f );
        lr.shadowCastingMode   = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows      = false;
        lr.sharedMaterial      = _sharedMat;
        lr.enabled             = false;
        return lr;
    }

    private void OnDestroy()
    {
        if ( _instance == this ) _instance = null;
    }
}
