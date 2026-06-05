using UnityEngine;

public enum SpawnType { InsideBox, NextToCurve, BiomePaint, DesiredAltitude, InDistance }

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

[System.Serializable]
public class PreyScaleSettings
{
    public float maxScaleStartLife = 1;
    public float maxScaleEndLife   = 0;
    public float maxScale          = .1f;
}

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

[System.Serializable]
public class PreyTurningSettings
{
    public float bankStrength  = 5f;
    public float bankSmoothing = 0.08f;
}

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

[System.Serializable]
public class PreyCageModule
{
    public float borderTurnDistance = 10f;
    public float borderTurnForce    = 2f;
}

[System.Serializable]
public class PreyFlapSettings
{
    public float flapSpeed           = 1;
    public float upBounceSize        = 1f;
    public float forwardBounceSize   = .5f;
    public float forwardBounceOffset = .5f;

    [Header( "Ambient Flapping" )]
    public float defaultFlapRate   = 0f;   // 0 = disabled; matches flapSpeed units (radians/frame)
    public float medianFlapCluster = 2f;   // average flaps per burst (geometric distribution)
    public float glideTimeMin      = 0.5f; // seconds between bursts (min)
    public float glideTimeMax      = 2.0f; // seconds between bursts (max)
}

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
    public float runForce        = 4f;   // away-from-wren push
    public float takeOffDistance = 15f;  // once this far from the takeoff point, drop the forces → Calm
    public float takeOffFlapMult = 3f;   // flap-rate boost during takeoff
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
    [Header( "Core" )]
    public PreyScaleSettings    scale;
    public PreyMovementSettings movement;
    public PreyFlapSettings     flap;
    public PreyTurningSettings  turning;
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