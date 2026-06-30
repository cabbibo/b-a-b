using UnityEngine;

public enum SpawnType { InsideBox, NextToCurve, Painted, DesiredAltitude, InDistance, OnPointOfInterest }

// How a bird approaches its perch when landing.
public enum PerchApproachStyle { Dive, Spiral }

// ─── Module toggles ──────────────────────────────────────────────────────────

[System.Serializable]
public class PreyModuleFlags
{
    [Header( "Core" )]
    public bool social    = false;
    public bool flap      = true;
    public bool collision = false;   // hard swept-collision so birds can't pass through geometry

    [Header( "Flavor" )]
    public bool noise = false;

    [Header( "Calm Behavior" )]
    public bool altitude = true;
    public bool circle   = false;
    public bool flock    = false;
    public bool spline   = false;
    public bool cage     = false;
    public bool bounce   = false;   // ballistic drop + bounce off the ground (replaces normal calm steering)
    public bool slide    = false;   // hug the ground when near it + steer down the steepest slope
    public bool settle   = false;   // a slow-enough bouncing bird lands into the Settled state
    public bool relaunch = false;   // a Settled bird springs back up when its settle time elapses
    public bool pop      = false;   // periodic upward impulse while near the ground (popcorn)
    // updraft / thermal removed — now defined by interest points

    [Header( "Behavior" )]
    public bool perch            = false;
    public bool takeOff          = false;
    public bool run              = false;
    public bool sprint           = false;
    public bool drive            = false;
    public bool avoidance        = true;
    public bool search           = false;
}

// ─── Core settings ───────────────────────────────────────────────────────────

// PreyScaleSettings moved to PreyVisualsConfigSO (visuals are authored/reused separately).

[System.Serializable]
public class PreyMovementSettings
{
    public float minSpeed     = 0.05f;
    public float maxSpeed     = 0.2f;
    public float desiredSpeed = 0.1f;
    public float dampening    = 0.1f;
    // public float forwardSpeed;                       // unused
    public float maxAngleTurnBetweenFrames = 4f;
    // public float minimumDotProductMatchForTurn;      // unused

    [Range( 0f , 1f )]
    [Tooltip( "Eases the force vector toward the active state's force each frame so state changes " +
              "blend instead of snapping. 1 = instant (no smoothing); lower = smoother (try 0.1–0.3)." )]
    public float newStateForceLerpSpeed = 1f;
}

// PreyDespawnSettings removed — despawn config (type/collider/distance/timing) now lives on PreyManager.

[System.Serializable]
public class PreyAltitudeSettings
{
    public float desiredAltitudeMin             = 8;
    public float desiredAltitudeMax             = 12;
    // public float minAltitude                 = 5;   // unused (only the dead SetHeight referenced it)
    // public float maxAltitude                 = 20;  // unused (only the dead SetHeight referenced it)
    public float minimumTotalY                  = 0;
    public float strengthTowardsDesiredAltitude = 1;
}

[System.Serializable]
public class PreyPhysicsSettings
{
    public int   physicsResolution    = 1;
    public float physicsInfoLerpSpeed = .1f;
}

// PreyTurningSettings (bank) moved to PreyVisualsConfigSO.

[System.Serializable]
public class PreyAvoidanceModule
{
    public bool  avoidGround            = true;
    public bool  avoidObjects           = true;
    public float avoidanceStartDistance = 20f;
    public float maxForce               = 3f;
}

// Hard swept collision (collide-and-slide). Avoidance steers away; this is the backstop that
// physically stops the bird at surfaces so it can't pass through geometry.
[System.Serializable]
public class PreyCollisionModule
{
    [Tooltip( "What the bird collides with. Exclude the prey's own layer to avoid self-hits." )]
    public LayerMask layers = ~0;
    [Tooltip( "Radius of the swept sphere — the bird's collision thickness." )]
    public float     radius = 0.5f;
    [Tooltip( "Stay this far off surfaces (prevents jitter / starting a cast inside a collider)." )]
    public float     skin   = 0.05f;
    [Tooltip( "Slide along the surface instead of stopping dead on contact." )]
    public bool      slide  = true;
}

// Ballistic "drop and bounce" calm behavior (modules.bounce). Gravity pulls the bird down; it reflects
// off the surface normal with decreasing bounces and slides downhill. On its own it bounces forever —
// pair it with the Settle module to make birds come to rest, and the Relaunch module to spring them up.
[System.Serializable]
public class PreyBounceModule
{
    [Tooltip( "Downward acceleration per frame while falling." )]
    public float     gravity        = 0.01f;
    [Range( 0f , 1.2f )]
    [Tooltip( "Fraction of vertical speed kept on each bounce. 1 = perfect, <1 = decays toward Settle, >1 = grows." )]
    public float     restitution    = 0.6f;
    [Range( 0f , 1f )]
    [Tooltip( "Fraction of horizontal speed kept on each bounce." )]
    public float     bounceFriction = 0.85f;
    [Tooltip( "Keep the body this far above the ground (its 'radius')." )]
    public float     radius         = 0.5f;
    public LayerMask groundLayers   = ~0;

    [Header( "Terrain Drive" )]
    [Tooltip( "Horizontal (XZ) push along the ground/terrain normal under the bird — drives it downhill / " +
              "away from the surface it's bouncing on, on top of gravity. 0 = off." )]
    public float     terrainNormalDrive = 0f;
}

// Automatic landing (modules.settle). When a bouncing bird's speed drops below Settle Speed it switches
// to the SETTLED state — pinned where it landed — for Time To Remain Settled seconds. This is NOT the
// Perch state and uses none of the perch/takeoff params. With the Relaunch module on it springs back up
// when the time elapses; without it the bird just stays settled. (Needs the Bounce module to fall.)
[System.Serializable]
public class PreySettleModule
{
    [Tooltip( "Bounce/motion speed below which the bird settles (lands). Without the Settle module a weak " +
              "bounce instead keeps a minimum hop of this speed (bounces forever)." )]
    public float settleSpeed                 = 0.03f;
    [Tooltip( "Seconds to stay settled. Huge = effectively stays put; 0 = leave immediately (relaunch at once if Relaunch is on)." )]
    public float timeToRemainSettled         = 3f;
    [Tooltip( "Random ± variance (seconds) applied to Time To Remain Settled." )]
    public float timeToRemainSettledVariance = 0f;
    [Tooltip( "If the wren comes within this distance of a settled bird, it relaunches immediately " +
              "instead of waiting out its settle time (needs the Relaunch module on). 0 = disabled." )]
    public float relaunchWrenRadius          = 0f;
    [Tooltip( "How far above the surface a spawn-settled bird rests. It raycasts straight down to the " +
              "ground and sits this far along the surface normal above it." )]
    public float settleOffset                = 0.3f;
    [Tooltip( "Spawn-settled only: if the ground is further than this below the spawn point (or there's " +
              "no ground below), the bird does NOT settle — it stays a normal flying/bouncing bird. " +
              "0 = no limit (always settle)." )]
    public float maxSettleHeight             = 0f;
}

// Automatic launch (modules.relaunch). When a Settled bird's settle time elapses it launches back into
// the air — an upward kick plus a horizontal kick along its current heading (scattered) — and returns
// to Calm (resuming Bounce if that's on). Settle + Relaunch + Bounce together make a trampoline loop.
[System.Serializable]
public class PreyRelaunchModule
{
    [Tooltip( "Upward speed kicked in on relaunch." )]
    public float relaunchForce             = 0.2f;
    [Tooltip( "Horizontal speed kicked in on relaunch, along the bird's current heading." )]
    public float relaunchForwardVelocity   = 0.1f;
    [Range( 0f , 1f )]
    [Tooltip( "Scatter on the relaunch heading. 0 = straight along current heading; 1 = any horizontal direction." )]
    public float relaunchForwardRandomness = 0.25f;
    [Range( 0f , 1f )]
    [Tooltip( "Aim the upward kick along the terrain normal under the bird instead of straight up. " +
              "0 = straight up (world up); 1 = fully along the surface normal, so birds on a slope launch " +
              "out perpendicular to it. Uses the last ground normal under the bird (best paired with Bounce)." )]
    public float relaunchTerrainNormal     = 0f;
    [Tooltip( "Horizontal speed kicked AWAY from the wren on relaunch, so a disturbed flock scatters away " +
              "from it. Layered on top of the heading kick. 0 = no away-from-wren push." )]
    public float relaunchAwayFromWren      = 0f;
}

// Periodic upward impulse (modules.pop) — "popcorn". While within Ground Closeness of the surface
// below, the bird gets a Pop Force upward kick every Time Between Pops (±variance) seconds. Best
// paired with Bounce (which owns the vertical axis); pops a Settled bird back into Calm so it takes.
[System.Serializable]
public class PreyPopModule
{
    [Tooltip( "Upward speed kicked in on each pop." )]
    public float popForce                = 0.2f;
    [Tooltip( "Seconds between pops." )]
    public float timeBetweenPops         = 2f;
    [Tooltip( "Random ± variance (seconds) applied to Time Between Pops." )]
    public float timeBetweenPopsVariance = 0.5f;
    [Tooltip( "Only pop when the bird is within this distance of the ground below it. 0 = pop at any height." )]
    public float groundCloseness         = 2f;
}

// Hug-the-terrain calm behavior (modules.slide). While within Ground Range of the surface below, pull
// toward the ground and steer down the steepest-descent direction (zero on flat ground). A steering
// module — it adds forces; it doesn't take over the vertical axis like Bounce does.
[System.Serializable]
public class PreySlideModule
{
    [Tooltip( "Only acts while the bird is within this distance of the ground (straight down)." )]
    public float     groundRange   = 8f;
    [Tooltip( "Downward force that keeps the bird hugging the ground while it's within range." )]
    public float     pullForce     = 2f;
    [Tooltip( "Force along the steepest-downhill direction of the surface below — automatically zero on " +
              "flat ground and stronger on steeper slopes." )]
    public float     downhillForce = 3f;
    public LayerMask groundLayers  = ~0;
}

[System.Serializable]
public class PreyCageModule
{
    public float borderTurnDistance = 10f;
    public float borderTurnForce    = 2f;
}

// PreyFlapSettings moved to PreyVisualsConfigSO.

[System.Serializable]
public class PreyCrystalSettings
{
    public float eatRadius         = 1.5f;
    public float focusRadius       = 15f;
    public int   crystalsOnCollect = 50;
    public float crystalType       = .5f;
}

// Spawn/despawn placement now lives on PreyManager. Only the spawn/die animation timings live here.
[System.Serializable]
public class PreySpawnSettings
{
    public float spawnSpeed  = 3f;   // grow-in time
    public float dieSpeed    = 1f;   // shrink-out time when despawning
    public float ateDieSpeed = .3f;  // shrink-out time when eaten
}

[System.Serializable]
public class PreyDistanceSettings
{
    // public float maxDistanceFromSpawnPoint;          // unused
    // public float maxDistanceStart          = 100;    // unused
    // public float maxDistanceEnd            = 200;    // unused
    // public float forceInwardsAtMaxDistanceEnd = 1;   // unused
    public float maxForwardDistance           = 100;
    public float maxDownDistance              = 100;
}

// ─── Flavor modules ──────────────────────────────────────────────────────────

[System.Serializable]
public class PreyNoiseSettings
{
    public float noiseSize  = 1;
    public float noiseSpeed = 1;
    public float noiseForce = 1;
}

// Mirrors UpdraftPointSettings — a soaring circle centered on the bird's PreyManager.
[System.Serializable]
public class PreyCircleSettings
{
    public float         forceUp       = 0f;   // 0 = horizontal circling (let the altitude module own Y)
    public float         forceIn       = 1f;
    public float         curlForce     = 2f;
    public CurlDirection curlDirection = CurlDirection.CounterClockwise;

    [Header( "Desired Altitude" )]
    public float desiredAltitude      = 40f;   // only used when forceUp > 0
    public float altitudeRange        = 15f;
    public float altitudeHoldStrength = 1f;
}

[System.Serializable]
public class PreyFlockModule
{
    public float detectionRadius       = 20f;
    public float separationRadius      = 3f;
    public float separationForce       = 2f;
    public float alignmentForce        = 0.5f;
    public float cohesionForce         = 0.3f;
    public float neighborQueryInterval = 0.5f;
}

[System.Serializable]
public class PreySplineModule
{
    public float pullForce        = 1f;
    public float splineForwardForce = 2f;
}

// removed — updraft is now defined per-point on Updraft interest points (UpdraftPointSettings)
// [System.Serializable]
// public class PreyUpdraftModule
// {
//     public float detectionRadius = 40f;
//     public float spiralForce     = 2f;
//     public float liftForce       = 3f;
// }

// removed — thermal soaring is now handled via interest points
// [System.Serializable]
// public class PreyThermalModule
// {
//     public float circleSpeed      = 0.3f;
//     public float minAltitude      = 30f;
//     public float maxAltitude      = 80f;
//     public float thermalTightness = 0.5f;
// }

// ─── Behavior modules ────────────────────────────────────────────────────────

[System.Serializable]
public class PreyPerchModule
{
    public float snapDistance    = 1.5f;  // how close to the surface = landed
    public float landingOffset   = 0.3f;  // float above the surface normal when perched
    public float startleRadius   = 15f;   // wren distance that triggers takeoff
    [Tooltip( "Seconds of the wren's approach to lead by — startle reach grows by how far the wren " +
              "travels toward the prey in this time, so a fast head-on approach spooks earlier. 0 = plain ring." )]
    public float startleLeadTime = 1f;

    [Header( "Landing (forces)" )]
    public float landingBlendDuration = 2.5f; // seconds to ramp calm forces → landing forces
    public float approachHeight       = 6f;   // height above target to aim for before diving
    public float approachRadius       = 14f;  // switch from aim-above to direct aim within this distance
    public float approachSpeedMult    = 0.4f; // speed multiplier while approaching
    public float landingFlapMult      = 4f;   // flap-rate multiplier when close to the surface
    [Tooltip( "Within this distance to the landing surface, avoidance fades to 0 — so the bird avoids " +
              "obstacles on the way in but doesn't fight the landing point itself. 0 = avoid the whole way." )]
    public float landingAvoidanceFalloff = 4f;

    [Header( "Landing (style)" )]
    [Tooltip( "Dive: fly to a point above the perch, then drop straight down. Spiral: corkscrew in on a " +
              "logarithmic spiral that winds tighter and descends onto the perch." )]
    public PerchApproachStyle approachStyle     = PerchApproachStyle.Dive;
    [Tooltip( "Spiral: starting radius of the corkscrew around the perch." )]
    public float              spiralRadius      = 6f;
    [Tooltip( "Spiral: how fast the radius shrinks per radian (logarithmic-spiral tightness)." )]
    public float              spiralTightness   = 0.15f;
    [Tooltip( "Spiral: angular speed (radians/sec) around the perch." )]
    public float              spiralSpeed       = 3f;
    [Tooltip( "Spiral: how fast it descends toward the perch (height lost per radian)." )]
    public float              spiralDescentRate = 1.5f;

    [Header( "Social landing" )]
    public float socialLandRadius     = 20f;
    public float landingSpacing       = 3f;
}

[System.Serializable]
public class PreyTakeOffModule
{
    public float upForce         = 5f;   // upward pop on takeoff (on startle, or scheduled after perching)
    public float runForce        = 4f;   // away-from-wren push (all takeoffs)
    public float takeOffDistance = 15f;  // once this far from the takeoff point, drop the forces → Calm
    public float takeOffFlapMult = 3f;   // flap-rate boost during takeoff
    [Tooltip( "Launch along the normal of the surface the bird was sitting on, scaled by this × Up Force. " +
              "0 = straight up (old behavior); higher = pushed off perpendicular to a slope/wall." )]
    public float colliderNormalForce = 0f;
    [Tooltip( "Extra away-from-the-wren push applied ONLY when takeoff was triggered by being startled " +
              "(wren got close) — so panic jumps fling away from the wren. 0 = no extra (timed/social " +
              "takeoffs are unaffected either way)." )]
    public float startleAwayForce = 0f;
}

[System.Serializable]
public class PreyRunModule
{
    public bool  chaseInstead     = false; // if true, bird chases wren instead of fleeing (magpie)
    public float startleRadius    = 15f; // wren enters this → Disturbed
    public float fullRunRadius    = 5f; // wren enters this → max flee/chase force
    [Tooltip( "Seconds of the wren's approach to lead by — startle reach grows by how far the wren " +
              "travels toward the prey in this time, so a fast head-on approach spooks earlier. 0 = plain ring." )]
    public float startleLeadTime  = 1f;
    public float fleeForce        = 3f;
    public float speedMultiplier  = 1.5f; // speed boost while disturbed
    public float jukeAmount       = 2f;
    public float jukeFrequency    = 1f; // jukes per second
    public float calmDownTime     = 8f; // seconds before considering calm
    public float calmDownDistance = 30f; // wren must be outside this to calm down
}


[System.Serializable]
public class PreyDriveModule
{
    public float driveForce = 1f;
}

[System.Serializable]
public class PreySearchModule
{
    public float calmBeforeSearch         = 15f;  // seconds calm before a forced search scan
    public float calmBeforeSearchVariance = 5f;
    // public float noticeChance          = 1f;   // unused (replaced by per-point noticeUrgency)
    public float moveForce                = 2f;   // force toward the target while searching
    // public float arrivalRadius         = 4f;   // unused (replaced by per-point enterRadius)
    public float giveUpTime              = 30f;  // give up if still not arrived after this long

    [Range( 0f , 1f )]
    [Tooltip( "How strongly to prefer the closest point of interest. 1 = always pick the closest; " +
              "0 = ignore distance (priority-weighted random)." )]
    public float closenessImportance = 0.5f;

    [Tooltip( "Max chained NewInterest hops before the bird is forced into a full calm (no searching " +
              "until the calm period completes). Stops endless search→search ping-ponging. 0 = unlimited." )]
    public int maxNewInterestsBeforeForcedCalm = 3;
}

[System.Serializable]
public class PreySprintModule
{
    public float maxSprintSpeedMultiplier = 2f;    // multiple of movement.maxSpeed (2 = 2× normal max)
    public float staminaDrainRate  = 0.5f;  // stamina/sec drained at full sprint speed
    public float staminaRefillRate = 0.1f;  // stamina/sec constant refill
    public float maxStamina        = 1.0f;
}

// ─── Social pressure ─────────────────────────────────────────────────────────

[System.Serializable]
public class PreySocialModule
{
    public float neighborRadius   = 25f;  // only birds within this range influence us
    public float socialWeight     = 1f;   // global multiplier on all social pressure
    public float decayRate        = 1.5f; // desire decays this many units/second when not fed
    public float sampleInterval   = 0.25f;

    [Header( "Thresholds  (desire ≥ value → trigger)" )]
    public float takeOffThreshold = 1f;
    public float disturbThreshold = 1f;
    public float calmThreshold    = 1f;
    public float landThreshold    = 1f;
}

// ─── Debug settings ──────────────────────────────────────────────────────────

[System.Serializable]
public class PreyDebugSettings
{
    public bool showSocialDebug = false;
    public bool showForceArrows = true;
    public bool showVelocity    = true;
    public bool showFlap        = false;
    public bool showStateLabel  = true;
    public bool showCalmDebug   = true;
    public bool showSearchDebug = true;
    public bool showRunDebug    = true;
    public bool showLandDebug   = true;
    public bool showEatRadius   = false;
}

// ─── ScriptableObject ────────────────────────────────────────────────────────

[CreateAssetMenu( fileName = "PreyConfigSO" , menuName = "Prey/PreyConfigSO" , order = 1 )]
public class PreyConfigSO : ScriptableObject
{
    [Header( "Debug" )]
    public PreyDebugSettings debug;

    [Header( "Modules" )]
    public PreyModuleFlags modules;

    // ── Core: fundamental movement / sensing / lifecycle ──────────────────────
    // scale / flap / turning(bank) moved to PreyVisualsConfigSO (assigned on the PreyManager).
    [Header( "Core" )]
    public PreyMovementSettings movement;
    public PreyPhysicsSettings  physics;
    public PreyDistanceSettings distance;
    public PreyCrystalSettings  crystals;
    [Tooltip( "Spawn/die animation timings. Spawn & despawn placement live on the PreyManager." )]
    public PreySpawnSettings    animation;

    // ── Calm: what the bird does while in the Calm state ──────────────────────
    [Header( "Calm" )]
    public PreyAltitudeSettings altitude;
    public PreyCircleSettings   circle;
    public PreyFlockModule      flock;
    public PreySplineModule     spline;
    public PreyCageModule       cage;
    public PreyBounceModule     bounce;
    public PreySlideModule      slide;
    public PreySettleModule     settle;
    public PreyRelaunchModule   relaunch;
    public PreyPopModule        pop;
    // public PreyUpdraftModule  updraft;   // removed — defined per Updraft interest point
    // public PreyThermalModule  thermal;   // removed — handled via interest points

    // ── Points of Interest Behaviors ──────────────────────────────────────────
    [Header( "Points of Interest Behaviors" )]
    public PreyPerchModule    perch;
    public PreyTakeOffModule  takeOff;

    // ── Behaviors: states the bird can switch into ────────────────────────────
    [Header( "Behaviors" )]
    public PreyRunModule       run;
    public PreySprintModule    sprint;
    public PreyDriveModule     drive;
    public PreyAvoidanceModule avoidance;
    public PreyCollisionModule collision;
    public PreySearchModule    search;
    public PreySocialModule    social;

    [Header( "Flavor" )]
    public PreyNoiseSettings noise;
}