using UnityEngine;
using System.Collections.Generic;

// Drop ONE of these anywhere in the scene to get a system-wide readout of
// every PreyManager at once: total birds, per-state breakdown, real compute
// cost (ms in PreyController/PreyManager Update) and physics-cast budget,
// plus a per-manager table.
//
// It owns no birds and changes no behavior — it only reads. State counts are
// only walked when the HUD is visible. Timing/raycast numbers come from
// PreyProfiler, which the prey code feeds every frame.
[DefaultExecutionOrder( -100 )] // run before prey Updates so FrameGate publishes cleanly
public class MetaPreyController : MonoBehaviour
{
    [Header( "Display" )]
    public bool      show       = true;
    public KeyCode   toggleKey  = KeyCode.F3;
    [Tooltip( "Top-right by default. X/Y are pixels from the top-right corner." )]
    public Vector2   margin     = new Vector2( 10 , 10 );
    public int       width      = 320;

    [Header( "Discovery" )]
    [Tooltip( "How often (s) to rescan the scene for PreyManagers." )]
    public float     rescanInterval = 1f;

    // ── cached managers ────────────────────────────────────────────────────
    private readonly List<PreyManager> _managers = new List<PreyManager>();
    private float _nextRescan;

    // ── aggregated, recomputed each frame the HUD is visible ───────────────
    private int   _total, _cap;
    private readonly int[] _stateCounts = new int[7]; // indexed by (int)PreyState
    private readonly List<ManagerRow> _rows = new List<ManagerRow>();

    private struct ManagerRow
    {
        public string name;
        public int    count;
        public int    cap;
        public int    calm, searching, landing, updrafting, perched, takingOff, disturbed;
    }

    // order MUST match the PreyState enum: Calm,Searching,Landing,Perched,Updrafting,TakingOff,Disturbed
    private static readonly string[] StateNames =
        { "Calm" , "Searching" , "Landing" , "Perched" , "Updrafting" , "TakingOff" , "Disturbed" };
    private static readonly string[] StateColors =
        { "#55dd55" , "#9933FF" , "#ffff55" , "#ff8800" , "#55ddaa" , "#55ddff" , "#ff5555" };

    private void Awake()
    {
        Rescan();
    }

    private void Update()
    {
        // publish last frame's profiler totals before any prey Update runs this frame
        PreyProfiler.FrameGate( Time.frameCount );

        if ( Input.GetKeyDown( toggleKey ) ) show = !show;

        if ( Time.unscaledTime >= _nextRescan ) {
            _nextRescan = Time.unscaledTime + Mathf.Max( 0.1f , rescanInterval );
            Rescan();
        }

        // walk birds once per frame (not per OnGUI event) only while visible
        if ( show ) Aggregate();
    }

    private void Rescan()
    {
        _managers.Clear();
        var found = Object.FindObjectsByType<PreyManager>( FindObjectsSortMode.None );
        for ( int i = 0; i < found.Length; i++ )
            if ( found[i] != null ) _managers.Add( found[i] );
    }

    private void Aggregate()
    {
        _total = 0;
        _cap   = 0;
        for ( int i = 0; i < _stateCounts.Length; i++ ) _stateCounts[i] = 0;
        _rows.Clear();

        for ( int m = 0; m < _managers.Count; m++ ) {
            var mgr = _managers[m];
            if ( mgr == null || mgr.preyHolder == null ) continue;

            var row = new ManagerRow { name = mgr.name , cap = mgr.maxPray };

            int n = mgr.preyHolder.childCount;
            for ( int i = 0; i < n; i++ ) {
                var bird = mgr.preyHolder.GetChild( i ).GetComponent<PreyController>();
                if ( bird == null ) continue;
                row.count++;

                int s = (int)bird.state;
                if ( s >= 0 && s < _stateCounts.Length ) _stateCounts[s]++;

                switch ( bird.state ) {
                    case PreyState.Calm:       row.calm++;       break;
                    case PreyState.Searching:  row.searching++;  break;
                    case PreyState.Landing:    row.landing++;    break;
                    case PreyState.Updrafting: row.updrafting++; break;
                    case PreyState.Perched:    row.perched++;    break;
                    case PreyState.TakingOff:  row.takingOff++;  break;
                    case PreyState.Disturbed:  row.disturbed++;  break;
                }
            }

            _total += row.count;
            _cap   += row.cap;
            _rows.Add( row );
        }
    }

    private void OnGUI()
    {
        if ( !show || !Application.isPlaying ) return;

        int    x       = Screen.width - width - (int)margin.x;
        double totalMs = PreyProfiler.lastControllerMs + PreyProfiler.lastManagerMs + PreyProfiler.lastBatchMs;

        GUILayout.BeginArea( new Rect( x , margin.y , width , Screen.height - margin.y - 10 ) );
        GUI.Box( new Rect( 0 , 0 , width , LineH * (12 + _rows.Count) + 12 ) , "" );
        GUILayout.Space( 4 );

        Label( $"<b>PREY SYSTEMS</b>   {_managers.Count} manager(s)" );
        Label( $"<b>Birds:</b> {_total} / {_cap}" );

        // states on two lines, color-coded
        var sb = "";
        for ( int i = 0; i < 4; i++ )
            sb += $"<color={StateColors[i]}>{StateNames[i]} {_stateCounts[i]}</color>   ";
        Label( sb );
        sb = "";
        for ( int i = 4; i < 7; i++ )
            sb += $"<color={StateColors[i]}>{StateNames[i]} {_stateCounts[i]}</color>   ";
        Label( sb );

        Label( "<b>── Compute (last frame) ──</b>" );
        Label( $"PreyController.Update: <b>{PreyProfiler.lastControllerMs:0.00} ms</b>" );
        Label( $"PreyManager.Update:    <b>{PreyProfiler.lastManagerMs:0.00} ms</b>" );
        Label( $"RaycastBatcher:        <b>{PreyProfiler.lastBatchMs:0.00} ms</b>" );
        Label( $"Total prey script:     <color={(totalMs > 4f ? "#ff5555" : "#55dd55")}><b>{totalMs:0.00} ms</b></color>" );
        Label( $"Rays batched: <b>{PreyProfiler.lastBatchedRaycastCount}</b>  sync: {PreyProfiler.lastRaycastCount}  sphere: {PreyProfiler.lastSpherecastCount}" );
        Label( $"Spline queries: <b>{PreyProfiler.lastSplineQueryCount}</b> / frame" );

        Label( "<b>── Per manager ──</b>" );
        for ( int i = 0; i < _rows.Count; i++ ) {
            var r = _rows[i];
            Label( $"{r.name}: <b>{r.count}</b>/{r.cap}  " +
                   $"<color=#55dd55>{r.calm}</color>/" +
                   $"<color=#9933FF>{r.searching}</color>/" +
                   $"<color=#ffff55>{r.landing}</color>/" +
                   $"<color=#ff8800>{r.perched}</color>/" +
                   $"<color=#ff5555>{r.disturbed}</color>" );
        }

        Label( $"<size=10>[{toggleKey} to toggle]  c/s/l/p/d</size>" );
        GUILayout.EndArea();
    }

    private const float LineH = 18f;

    private void Label( string s )
    {
        GUILayout.Label( s , RichStyle );
    }

    private GUIStyle _rich;
    private GUIStyle RichStyle
    {
        get {
            if ( _rich == null ) {
                _rich = new GUIStyle( GUI.skin.label ) { richText = true , wordWrap = false };
                _rich.normal.textColor = Color.white;
            }
            return _rich;
        }
    }
}
