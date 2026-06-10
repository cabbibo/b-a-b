using UnityEngine;

// Fun, self-contained debug overlay for the prey system. Drop it on any object.
//   F4            toggle the whole overlay
//   hover a bird  shows a live inspector panel for the nearest bird to the cursor
//   left-click    lock / unlock that selection
//
// Bottom-left it draws rolling time-series graphs:
//   • Prey compute (ms) with a red budget line
//   • Population by state (colorful stacked area)
//   • Rays per frame
//   • FPS
//
// It walks the birds itself (only while visible), so it works with or without
// MetaPreyController present. Compute/ray numbers come from PreyProfiler.
[DefaultExecutionOrder( -90 )]
public class PreyDebugGraphs : MonoBehaviour
{
    [Header( "Display" )]
    public bool    show          = true;
    public KeyCode toggleKey     = KeyCode.F4;
    public int     historyLength = 240;
    [Tooltip( "Pixels from the bottom-left corner." )]
    public Vector2 origin        = new Vector2( 12 , 12 );
    public int     panelWidth    = 300;
    public int     graphHeight   = 56;
    [Tooltip( "Red budget line on the ms graph." )]
    public float   msBudget      = 4f;

    [Header( "Inspector" )]
    public bool  enableInspector  = true;
    public float pickRadiusPixels = 60f;

    // ── history ring buffers ───────────────────────────────────────────────
    private float[] _ms;
    private int[]   _count;
    private int[]   _rays;
    private float[] _fps;
    private int[][] _stateHist;   // [sample][7]
    private int     _head, _filled;

    private float _maxMs    = 4f;
    private int   _maxCount = 1;
    private int   _maxRays  = 1;

    // ── per-frame scratch ──────────────────────────────────────────────────
    private PreyController[] _birds = new PreyController[0];
    private readonly int[]   _stateCounts = new int[7];
    private int _total;

    private PreyController _selected;
    private bool           _locked;

    private static readonly Color[] StateCols =
    {
        new Color( 0.33f , 0.87f , 0.33f ), // Calm
        new Color( 0.60f , 0.20f , 1.00f ), // Searching
        new Color( 1.00f , 1.00f , 0.33f ), // Landing
        new Color( 1.00f , 0.53f , 0.00f ), // Perched
        new Color( 0.33f , 0.87f , 0.67f ), // Updrafting
        new Color( 0.33f , 0.80f , 1.00f ), // TakingOff
        new Color( 1.00f , 0.33f , 0.33f ), // Disturbed
    };
    private static readonly string[] StateNames =
        { "Calm" , "Searching" , "Landing" , "Perched" , "Updrafting" , "TakingOff" , "Disturbed" };

    // ── lifecycle ──────────────────────────────────────────────────────────
    private void Awake() => Alloc();

    private void Alloc()
    {
        int n = Mathf.Max( 16 , historyLength );
        _ms = new float[n]; _count = new int[n]; _rays = new int[n]; _fps = new float[n];
        _stateHist = new int[n][];
        for ( int i = 0; i < n; i++ ) _stateHist[i] = new int[7];
        _head = 0; _filled = 0;
    }

    private int Cap    => _ms.Length;
    private int Newest => ( _head - 1 + Cap ) % Cap;

    private void Update()
    {
        PreyProfiler.FrameGate( Time.frameCount );

        if ( Input.GetKeyDown( toggleKey ) ) show = !show;
        if ( !show ) return;

        if ( _ms == null || _ms.Length != Mathf.Max( 16 , historyLength ) ) Alloc();

        Sample();
        Record();
        if ( enableInspector ) UpdateSelection();
    }

    private void Sample()
    {
        _birds = Object.FindObjectsByType<PreyController>( FindObjectsSortMode.None );
        for ( int i = 0; i < 7; i++ ) _stateCounts[i] = 0;
        _total = 0;
        for ( int i = 0; i < _birds.Length; i++ ) {
            var b = _birds[i];
            if ( b == null ) continue;
            int s = (int)b.state;
            if ( s >= 0 && s < 7 ) _stateCounts[s]++;
            _total++;
        }
    }

    private void Record()
    {
        float ms   = (float)( PreyProfiler.lastControllerMs + PreyProfiler.lastManagerMs + PreyProfiler.lastBatchMs );
        int   rays  = PreyProfiler.lastBatchedRaycastCount + PreyProfiler.lastRaycastCount;
        float fps   = Time.unscaledDeltaTime > 1e-5f ? 1f / Time.unscaledDeltaTime : 0f;

        _ms[_head] = ms; _count[_head] = _total; _rays[_head] = rays; _fps[_head] = fps;
        var sc = _stateHist[_head];
        for ( int i = 0; i < 7; i++ ) sc[i] = _stateCounts[i];

        // slow decay so the scale shrinks back down after a spike
        _maxMs    = Mathf.Max( _maxMs * 0.999f , ms , msBudget );
        _maxCount = Mathf.Max( _maxCount , _total , 1 );
        _maxRays  = Mathf.Max( _maxRays , rays , 1 );

        _head = ( _head + 1 ) % Cap;
        if ( _filled < Cap ) _filled++;
    }

    private void UpdateSelection()
    {
        var cam = Camera.main;
        if ( cam == null ) { _selected = null; return; }

        if ( Input.GetMouseButtonDown( 0 ) && _selected != null ) _locked = !_locked;
        if ( _locked && _selected == null ) _locked = false;
        if ( _locked ) return;

        Vector3 mouse = Input.mousePosition;
        float   best  = pickRadiusPixels * pickRadiusPixels;
        PreyController bestBird = null;

        for ( int i = 0; i < _birds.Length; i++ ) {
            var b = _birds[i];
            if ( b == null ) continue;
            Vector3 sp = cam.WorldToScreenPoint( b.transform.position );
            if ( sp.z <= 0f ) continue;   // behind camera
            float dx = sp.x - mouse.x, dy = sp.y - mouse.y;
            float d  = dx * dx + dy * dy;
            if ( d < best ) { best = d; bestBird = b; }
        }
        _selected = bestBird;
    }

    // ── drawing ────────────────────────────────────────────────────────────
    private static Texture2D _white;
    private static Texture2D White
    {
        get {
            if ( _white == null ) {
                _white = new Texture2D( 1 , 1 );
                _white.SetPixel( 0 , 0 , Color.white );
                _white.Apply();
            }
            return _white;
        }
    }

    private GUIStyle _label;

    private void Fill( float x , float y , float w , float h , Color c )
    {
        if ( h <= 0f || w <= 0f ) return;
        GUI.color = c;
        GUI.DrawTexture( new Rect( x , y , w , h ) , White );
        GUI.color = Color.white;
    }

    private void OnGUI()
    {
        if ( !show || !Application.isPlaying ) return;
        if ( _label == null ) {
            _label = new GUIStyle( GUI.skin.label ) { richText = true , fontSize = 11 , wordWrap = false };
            _label.normal.textColor = Color.white;
        }

        const float row = 16f;
        float ph    = graphHeight;
        float block = row + ph + 4f;
        float totalH = block * 4f + 4f;

        float x = origin.x;
        float y = Screen.height - origin.y - totalH;

        Fill( x - 5 , y - 5 , panelWidth + 10 , totalH + 10 , new Color( 0 , 0 , 0 , 0.55f ) );

        float cy = y;

        // 1) compute ms
        float msNow = _filled > 0 ? _ms[Newest] : 0f;
        GUI.Label( new Rect( x , cy , panelWidth , row ),
            $"<b>Prey compute</b>  <color=#88ddff>{msNow:0.00} ms</color>  <size=9>(scale {_maxMs:0.0})</size>" , _label );
        cy += row;
        DrawArea( x , cy , panelWidth , ph , _ms , _maxMs , new Color( 0.3f , 0.75f , 1f , 0.9f ) );
        // red budget line
        float by = cy + ph - Mathf.Clamp01( msBudget / _maxMs ) * ph;
        Fill( x , by , panelWidth , 1 , new Color( 1f , 0.3f , 0.3f , 0.8f ) );
        cy += ph + 4f;

        // 2) population by state (stacked)
        GUI.Label( new Rect( x , cy , panelWidth , row ), $"<b>Population</b>  {_total} birds  {Legend()}" , _label );
        cy += row;
        DrawStacked( x , cy , panelWidth , ph );
        cy += ph + 4f;

        // 3) rays
        int raysNow = _filled > 0 ? _rays[Newest] : 0;
        GUI.Label( new Rect( x , cy , panelWidth , row ),
            $"<b>Rays / frame</b>  <color=#ffdd55>{raysNow}</color>  <size=9>(batched {PreyProfiler.lastBatchedRaycastCount} + sync {PreyProfiler.lastRaycastCount})</size>" , _label );
        cy += row;
        DrawAreaInt( x , cy , panelWidth , ph , _rays , _maxRays , new Color( 1f , 0.85f , 0.3f , 0.9f ) );
        cy += ph + 4f;

        // 4) fps
        float fpsNow = _filled > 0 ? _fps[Newest] : 0f;
        GUI.Label( new Rect( x , cy , panelWidth , row ),
            $"<b>FPS</b>  <color={(fpsNow < 30 ? "#ff5555" : "#55dd55")}>{fpsNow:0}</color>" , _label );
        cy += row;
        DrawArea( x , cy , panelWidth , ph , _fps , 120f , new Color( 0.5f , 1f , 0.6f , 0.9f ) );
        // 60fps line
        float fy = cy + ph - ( 60f / 120f ) * ph;
        Fill( x , fy , panelWidth , 1 , new Color( 1f , 1f , 1f , 0.35f ) );

        DrawInspector();
    }

    private string Legend()
    {
        string s = "<size=9>";
        for ( int i = 0; i < 7; i++ ) {
            if ( _stateCounts[i] == 0 ) continue;
            string hex = ColorUtility.ToHtmlStringRGB( StateCols[i] );
            s += $"<color=#{hex}>{StateNames[i]} {_stateCounts[i]}</color>  ";
        }
        return s + "</size>";
    }

    private void DrawArea( float x , float y , float w , float h , float[] hist , float scale , Color col )
    {
        Fill( x , y , w , h , new Color( 1 , 1 , 1 , 0.05f ) );
        if ( _filled <= 0 || scale <= 0f ) return;
        float colW = w / Cap;
        for ( int i = 0; i < _filled; i++ ) {
            int idx = ( _head - _filled + i + Cap * 2 ) % Cap;
            float v = Mathf.Clamp01( hist[idx] / scale );
            float bh = v * h;
            Fill( x + i * colW , y + h - bh , Mathf.Max( 1f , colW ) , bh , col );
        }
    }

    private void DrawAreaInt( float x , float y , float w , float h , int[] hist , int scale , Color col )
    {
        Fill( x , y , w , h , new Color( 1 , 1 , 1 , 0.05f ) );
        if ( _filled <= 0 || scale <= 0 ) return;
        float colW = w / Cap;
        for ( int i = 0; i < _filled; i++ ) {
            int idx = ( _head - _filled + i + Cap * 2 ) % Cap;
            float v = Mathf.Clamp01( hist[idx] / (float)scale );
            float bh = v * h;
            Fill( x + i * colW , y + h - bh , Mathf.Max( 1f , colW ) , bh , col );
        }
    }

    private void DrawStacked( float x , float y , float w , float h )
    {
        Fill( x , y , w , h , new Color( 1 , 1 , 1 , 0.05f ) );
        if ( _filled <= 0 ) return;
        float colW = w / Cap;
        float scale = Mathf.Max( 1 , _maxCount );
        for ( int i = 0; i < _filled; i++ ) {
            int idx = ( _head - _filled + i + Cap * 2 ) % Cap;
            var sc = _stateHist[idx];
            float yb = y + h;
            for ( int s = 0; s < 7; s++ ) {
                if ( sc[s] <= 0 ) continue;
                float bh = ( sc[s] / scale ) * h;
                Fill( x + i * colW , yb - bh , Mathf.Max( 1f , colW ) , bh , StateCols[s] );
                yb -= bh;
            }
        }
    }

    private void DrawInspector()
    {
        if ( !enableInspector || _selected == null ) return;
        var cam = Camera.main;
        if ( cam == null ) return;

        var b = _selected;
        Vector3 sp = cam.WorldToScreenPoint( b.transform.position );
        if ( sp.z <= 0f ) return;
        float gx = sp.x;
        float gy = Screen.height - sp.y;   // GUI space (top-left origin)

        // crosshair box around the bird
        Color mark = _locked ? new Color( 1f , 0.4f , 0.4f ) : new Color( 1f , 1f , 0.4f );
        float r = 14f;
        Fill( gx - r , gy - r , 2 * r , 1 , mark );
        Fill( gx - r , gy + r , 2 * r , 1 , mark );
        Fill( gx - r , gy - r , 1 , 2 * r , mark );
        Fill( gx + r , gy - r , 1 , 2 * r + 1 , mark );

        int   si  = (int)b.state;
        Color scol = ( si >= 0 && si < 7 ) ? StateCols[si] : Color.white;
        string scolHex = ColorUtility.ToHtmlStringRGB( scol );

        float speed = b.velocity.magnitude;
        float pw = 188f, py0 = gy + r + 4f, px0 = gx + r + 4f;
        if ( px0 + pw > Screen.width ) px0 = gx - r - 4f - pw;

        string txt =
            $"<b>{b.name}</b>\n" +
            $"State: <color=#{scolHex}>{b.state}</color>{( _locked ? "  <color=#ff6666>[LOCK]</color>" : "" )}\n" +
            $"Speed: {speed:0.0}\n" +
            $"Ground: {b.distanceToGround:0.0}   Fwd: {b.distanceToForward:0.0}\n" +
            $"Stamina: {b.stamina:0.0}\n" +
            $"Life: {b.life:0.00}   ToWren: {b.vectorToWren.magnitude:0.0}\n" +
            $"Mgr: {( b.manager != null ? b.manager.name : "-" )}";

        // count actual lines for the box height
        int lines = 7;
        float ph = lines * 14f + 8f;
        Fill( px0 - 4 , py0 - 2 , pw , ph , new Color( 0 , 0 , 0 , 0.78f ) );
        GUI.Label( new Rect( px0 , py0 , pw , ph ) , txt , _label );
    }
}
