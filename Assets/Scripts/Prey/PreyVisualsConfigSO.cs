using UnityEngine;

// ─── Visual settings ───────────────────────────────────────────────────────────
// These used to live on PreyConfigSO. They were moved here so a bird's *look* (scale,
// flap, bank) and its *behavior* (movement, states, forces) can be authored and reused
// independently. PreyController still computes flap/scale/bank — it just reads the values
// from here now (relocate-config-only; see PreyVisuals).

// How a bird is rendered. All procedural modes share the same wing-tip flap computation
// (center + left/right tips driven by positionInFlapCycle); they only differ in what they
// draw between the center and each tip.
public enum PreyVisualType
{
    LineRendererProcedural, // a LineRenderer from the body center out to each wing tip
    QuadBasedProcedural,    // two quads (one per wing); tip points are fed to the shader to align them
    WingedMesh,             // an assigned mesh, deformed in a shader to flap using its bounding box
    TrailMesh,              // (not implemented yet)
}

[System.Serializable]
public class PreyScaleSettings
{
    public float maxScaleStartLife = 1;
    public float maxScaleEndLife   = 0;
    public float maxScale          = .1f;
}

[System.Serializable]
public class PreyTurningSettings
{
    public float bankStrength  = 5f;
    public float bankSmoothing = 0.08f;
}

[System.Serializable]
public class PreyFlapSettings
{
    public float flapSpeed           = 1;
    public float upBounceSize        = 1f;
    public float forwardBounceSize   = .5f;
    public float forwardBounceOffset = .5f;

    [Range( 0f , 1f )]
    [Tooltip( "Forces flapping based on climb direction. 0 = off (normal flap behavior). 1 = the bird MUST " +
              "flap whenever it's moving, at a rate set by how steeply it climbs: ~4x flapSpeed going " +
              "straight up, ~0.5x going level/forward. Makes launching birds flap hard to climb." )]
    public float climbSpeedFlapMultiplier = 0f;

    [Header( "Ambient Flapping" )]
    public float defaultFlapRate   = 0f;   // 0 = disabled; matches flapSpeed units (radians/frame)
    public float medianFlapCluster = 2f;   // average flaps per burst (geometric distribution)
    public float glideTimeMin      = 0.5f; // seconds between bursts (min)
    public float glideTimeMax      = 2.0f; // seconds between bursts (max)
}

// Wing geometry shared by every procedural mode (Line / Quad / WingedMesh). The wing tips are
// computed from the controller's positionInFlapCycle; each mode decides what to draw to them.
[System.Serializable]
public class PreyWingSettings
{
    [Tooltip( "Distance from the body center to each wing tip (in the bird's local space)." )]
    public float span        = 2f;
    [Tooltip( "How far the tip rises/falls over a flap, as a fraction of span. 0 = flat wings." )]
    public float flapHeight  = 0.5f;
    [Tooltip( "How far forward/back the tip is offset (sweep). 0 = straight out to the side." )]
    public float sweep       = 0f;
    [Header( "Tip smoothing (spring)" )]
    [Tooltip( "How hard the tip is pulled toward its target each frame. Higher = snappier." )]
    public float spring      = 1f;
    [Range( 0f , 1f )]
    [Tooltip( "Velocity retained each frame. Lower = more damped/heavier wings. 1 = no damping." )]
    public float dampening   = 0.7f;
}

// ─── ScriptableObject ────────────────────────────────────────────────────────
// The visual half of a prey type: how a bird scales in, flaps, banks, and what it renders.
// Assigned on the PreyManager (parallel to PreyConfigSO) and applied by the PreyVisuals
// component on each spawned bird.
[CreateAssetMenu( fileName = "PreyVisualsConfigSO" , menuName = "Prey/PreyVisualsConfigSO" , order = 2 )]
public class PreyVisualsConfigSO : ScriptableObject
{
    [Header( "Render Mode" )]
    public PreyVisualType visualType = PreyVisualType.LineRendererProcedural;

    // Initialized so a runtime-created fallback config (ScriptableObject.CreateInstance, used when a
    // bird spawns without a visuals config) still has valid settings instead of null sub-objects.
    [Header( "Visuals" )]
    public PreyScaleSettings   scale   = new PreyScaleSettings();
    public PreyFlapSettings    flap    = new PreyFlapSettings();
    public PreyTurningSettings turning = new PreyTurningSettings();
    public PreyWingSettings    wing    = new PreyWingSettings();

    // ── Line Renderer Procedural ──────────────────────────────────────────────
    [Header( "Line Renderer Procedural" )]
    [Tooltip( "Material for the wing line renderers. Null = the LineRenderer's default." )]
    public Material lineMaterial;
    public float    lineWidth = 0.1f;

    // ── Quad Based Procedural ─────────────────────────────────────────────────
    [Header( "Quad Based Procedural" )]
    [Tooltip( "Material whose shader aligns the quad between the body center and the wing tip " +
              "(tips fed in via _RootPos/_TipPos). Try the Prey/WingQuad shader." )]
    public Material quadMaterial;
    [Tooltip( "Half-width (chord) of each wing quad." )]
    public float    quadWidth = 0.5f;

    // ── Winged Mesh ───────────────────────────────────────────────────────────
    [Header( "Winged Mesh" )]
    [Tooltip( "Mesh deformed in the shader to flap. Its bounding box defines the wing span: " +
              "+X / -X extents are the wing tips, X=0 is the spine." )]
    public Mesh     mesh;
    [Tooltip( "Material whose shader bends the wings. Try the Prey/WingedMesh shader." )]
    public Material wingedMeshMaterial;
    [Tooltip( "Max bend angle (degrees) of the wing tips at full flap." )]
    public float    wingedFlapAngle = 35f;
}
