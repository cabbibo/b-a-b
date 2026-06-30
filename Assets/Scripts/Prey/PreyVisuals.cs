using UnityEngine;

// Visual half of a bird, sitting alongside PreyController on the prey prefab. It owns the
// PreyVisualsConfigSO (scale / flap / bank settings + the render mode) and renders the bird
// in one of several modes (line / quad / winged mesh). PreyController still computes the body
// flap/scale/bank and DRIVES this each tick via Tick() — there is intentionally no per-bird
// Update here, so the manager's single-loop / LOD architecture is preserved.
public class PreyVisuals : MonoBehaviour
{
    [Tooltip( "Scale / flap / bank settings + render mode for this prey type. Normally supplied by " +
              "PreyManager via PreyController.Initialize; a value set here is used as a fallback." )]
    public PreyVisualsConfigSO config;

    private PreyController _controller;

    // ── Procedural wing renderers (Line / Quad) ───────────────────────────────
    // One entry per wing (0 = left/-1, 1 = right/+1). Built on demand for the active mode.
    private struct Wing
    {
        public Transform           root;       // child GameObject holding the renderer
        public LineRenderer        line;       // LineRendererProcedural
        public MeshRenderer        quad;       // QuadBasedProcedural
        public MaterialPropertyBlock mpb;
        public Vector3             tip;        // spring-smoothed world tip position
        public Vector3             tipVel;     // spring velocity
    }
    private Wing[] _wings;

    // ── Winged Mesh renderer ──────────────────────────────────────────────────
    private MeshRenderer        _meshRenderer;
    private MaterialPropertyBlock _meshMpb;
    private float               _meshSpanX = 1f;   // +X bounding-box extent of the assigned mesh

    // Shared shader property ids
    private static readonly int _RootPosId = Shader.PropertyToID( "_RootPos" );
    private static readonly int _TipPosId  = Shader.PropertyToID( "_TipPos" );
    private static readonly int _WidthId   = Shader.PropertyToID( "_Width" );
    private static readonly int _FlapId    = Shader.PropertyToID( "_Flap" );
    private static readonly int _SpanXId   = Shader.PropertyToID( "_SpanX" );
    private static readonly int _FlapAngId = Shader.PropertyToID( "_FlapAngle" );

    // ── Setup ──────────────────────────────────────────────────────────────────

    // Called by PreyController.Initialize. Stores the config and builds the renderer for the mode.
    public void Initialize( PreyVisualsConfigSO cfg, PreyController controller )
    {
        if ( cfg != null ) config = cfg;
        _controller = controller;
        Build();
    }

    private void Build()
    {
        if ( config == null ) return;

        switch ( config.visualType ) {
            case PreyVisualType.LineRendererProcedural: BuildWings( quad: false ); break;
            case PreyVisualType.QuadBasedProcedural:    BuildWings( quad: true  ); break;
            case PreyVisualType.WingedMesh:             BuildWingedMesh();         break;
            case PreyVisualType.TrailMesh:              /* not implemented yet */  break;
        }
    }

    private void BuildWings( bool quad )
    {
        _wings = new Wing[2];
        for ( int i = 0; i < 2; i++ ) {
            var go = new GameObject( quad ? $"WingQuad_{i}" : $"WingLine_{i}" );
            go.transform.SetParent( transform, worldPositionStays: false );

            var w = new Wing { root = go.transform, mpb = new MaterialPropertyBlock(), tip = transform.position };

            if ( quad ) {
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = UnitWingQuad();
                w.quad = go.AddComponent<MeshRenderer>();
                w.quad.sharedMaterial = config.quadMaterial;
                w.quad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            } else {
                w.line = go.AddComponent<LineRenderer>();
                w.line.useWorldSpace = true;
                w.line.positionCount = 2;
                w.line.widthMultiplier = config.lineWidth;
                w.line.sharedMaterial = config.lineMaterial;   // shared → no per-bird material instance
                w.line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            _wings[i] = w;
        }
    }

    private void BuildWingedMesh()
    {
        if ( config.mesh == null ) return;

        var go = new GameObject( "WingedMesh" );
        go.transform.SetParent( transform, worldPositionStays: false );

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = config.mesh;
        _meshRenderer = go.AddComponent<MeshRenderer>();
        _meshRenderer.sharedMaterial = config.wingedMeshMaterial;
        _meshMpb = new MaterialPropertyBlock();

        // +X extent of the mesh = the wing tip distance the deform shader bends around.
        _meshSpanX = Mathf.Max( 0.0001f, config.mesh.bounds.extents.x );
    }

    // A unit quad spanning uv.x 0→1 (root→tip) and uv.y -0.5→0.5 (across the chord). The shader
    // places the vertices in world space from _RootPos/_TipPos, so the local positions are unused
    // for placement but kept sane so culling bounds aren't degenerate.
    private static Mesh _unitWingQuad;
    private static Mesh UnitWingQuad()
    {
        if ( _unitWingQuad != null ) return _unitWingQuad;
        var m = new Mesh { name = "PreyWingQuad" };
        m.vertices  = new[] {
            new Vector3( 0f, -0.5f, 0f ), new Vector3( 1f, -0.5f, 0f ),
            new Vector3( 1f,  0.5f, 0f ), new Vector3( 0f,  0.5f, 0f ),
        };
        m.uv        = new[] { new Vector2( 0, 0 ), new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 0, 1 ) };
        m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();
        m.bounds = new Bounds( Vector3.zero, Vector3.one * 1000f ); // never cull (verts move in shader)
        _unitWingQuad = m;
        return m;
    }

    // ── Per-tick drive (called by PreyController.UpdateBird) ────────────────────

    public void Tick()
    {
        if ( config == null || _controller == null ) return;

        switch ( config.visualType ) {
            case PreyVisualType.LineRendererProcedural: TickWings( quad: false ); break;
            case PreyVisualType.QuadBasedProcedural:    TickWings( quad: true  ); break;
            case PreyVisualType.WingedMesh:             TickWingedMesh();         break;
            case PreyVisualType.TrailMesh:              break;
        }
    }

    private void TickWings( bool quad )
    {
        if ( _wings == null ) return;

        var t   = transform;
        var w   = config.wing;
        float h = Mathf.Sin( _controller.positionInFlapCycle + Mathf.PI ) * w.span * w.flapHeight;

        for ( int i = 0; i < _wings.Length; i++ ) {
            float side   = i == 0 ? -1f : 1f;
            Vector3 target = t.position
                           + t.TransformVector( Vector3.right   * w.span * side )
                           + t.TransformVector( Vector3.up      * h )
                           + t.TransformVector( Vector3.forward * w.sweep );

            // spring the tip toward the target (same feel as the old PreyWing)
            ref Wing wing = ref _wings[i];
            wing.tipVel += (target - wing.tip) * w.spring;
            wing.tipVel *= w.dampening;
            wing.tip    += wing.tipVel;

            if ( quad ) {
                wing.quad.GetPropertyBlock( wing.mpb );
                wing.mpb.SetVector( _RootPosId, t.position );
                wing.mpb.SetVector( _TipPosId,  wing.tip );
                wing.mpb.SetFloat(  _WidthId,   config.quadWidth );
                wing.quad.SetPropertyBlock( wing.mpb );
            } else {
                wing.line.SetPosition( 0, t.position );
                wing.line.SetPosition( 1, wing.tip );
            }
        }
    }

    private void TickWingedMesh()
    {
        if ( _meshRenderer == null ) return;

        float flap = Mathf.Sin( _controller.positionInFlapCycle );
        _meshRenderer.GetPropertyBlock( _meshMpb );
        _meshMpb.SetFloat( _FlapId,    flap );
        _meshMpb.SetFloat( _SpanXId,   _meshSpanX );
        _meshMpb.SetFloat( _FlapAngId, config.wingedFlapAngle );
        _meshRenderer.SetPropertyBlock( _meshMpb );
    }
}
