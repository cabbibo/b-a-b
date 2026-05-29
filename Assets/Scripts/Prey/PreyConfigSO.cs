using UnityEngine;

public enum SpawnType    { InsideBox, NextToCurve, BiomePaint }
public enum AltitudeType { RandomRange, DesiredAltitude, OnGround }

// ─── Module toggles ──────────────────────────────────────────────────────────

[System.Serializable]
public class PreyModuleFlags
{
    [Header( "Core" )]
    public bool social   = false;
    public bool altitude = true;
    public bool flap     = true;

    [Header( "Flavor" )]
    public bool noise = false;

    public bool circle  = false;
    public bool flock   = false;
    public bool spline  = false;
    public bool updraft = false;
    public bool thermal = false;

    [Header( "Behavior" )]
    public bool perch            = false;
    public bool takeOff          = false;
    public bool run              = false;
    public bool sprint           = false;
    public bool drive            = false;
    public bool cage             = false;
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
    public float forwardSpeed;
    public float maxAngleTurnBetweenFrames = 4f;
    public float minimumDotProductMatchForTurn;
}

[System.Serializable]
public class PreyDespawnSettings
{
    public bool  onWrenExit                                   = true;
    public float minimumTimeAlive                            = 30f;
    public float distanceBeforeNotCaught                     = 100f;
    public float timeOutsideDistanceBeforeNotCaughtTriggered = 5f;
}

[System.Serializable]
public class PreyAltitudeSettings
{
    public float desiredAltitudeMin             = 8;
    public float desiredAltitudeMax             = 12;
    public float minAltitude                    = 5;
    public float maxAltitude                    = 20;
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

[System.Serializable]
public class PreySpawnSettings
{
    [Header( "Animation" )]
    public float spawnSpeed  = 3f;
    public float dieSpeed    = 1f;
    public float ateDieSpeed = .3f;

    [Header( "Placement" )]
    public SpawnType    spawnType    = SpawnType.InsideBox;
    public AltitudeType altitudeType = AltitudeType.RandomRange;
    public float spawnRadius = 5f;

    [Header( "Box Bounds" )]
    public Vector3 boundsMin;
    public Vector3 boundsMax;

    [Header( "Curve" )]
    public float curveOffset = 5f;

    [Header( "Bird Proximity" )]
    [Range( 0f , 1f )]
    [Tooltip( "0 = spawn anywhere in region,  1 = spawn at closest point in region to bird" )]
    public float closenessToBird = 0f;
}

[System.Serializable]
public class PreyDistanceSettings
{
    public float maxDistanceFromSpawnPoint;
    public float maxDistanceStart             = 100;
    public float maxDistanceEnd               = 200;
    public float forceInwardsAtMaxDistanceEnd = 1;
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

[System.Serializable]
public class PreyCircleSettings
{
    public float circleForce       = 1;
    public float circleRadius      = 10;
    public float updraft           = 0;
    public float alwaysCenterForce = 0;
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

[System.Serializable]
public class PreyUpdraftModule
{
    public float detectionRadius = 40f;
    public float spiralForce     = 2f;
    public float liftForce       = 3f;
}

[System.Serializable]
public class PreyThermalModule
{
    // thermalCenter lives on PreyManager so different scenes can configure it
    public float circleSpeed      = 0.3f; // radians/sec
    public float minAltitude      = 30f; // switch to tight thermaling below this
    public float maxAltitude      = 80f; // return to soaring above this
    public float thermalTightness = 0.5f; // radius multiplier when thermaling
}

// ─── Behavior modules ────────────────────────────────────────────────────────

[System.Serializable]
public class PreyPerchModule
{
    // perchPoints lives on PreyManager; anchorPoint is for soft wander (butterfly-style)
    public float anchorRadius          = 15f; // wander radius around manager.anchorPoint
    public float anchorPullForce       = 1f;
    public float snapDistance          = 1.5f; // how close = landed
    public float startleRadius         = 15f; // wren distance that triggers takeoff
    public float landDesireInterval    = 20f; // how often the bird wants to land
    public float landDesireVariance    = 10f;
    public float getBored         = 10f;
    public float getBoredVariance = 5f;
    public float approachSpeedMult     = 0.4f;
    public float socialLandRadius      = 20f;
    public float landingSpacing        = 3f;
    [Header( "Landing Approach" )]
    public float landingBlendDuration  = 2.5f; // seconds to ramp from calm forces to landing forces
    public float approachHeight        = 6f;   // height above target to aim for before diving
    public float approachRadius        = 14f;  // switch from aim-above to direct aim within this distance
    public float landingFlapMult       = 4f;   // flap rate multiplier when close to surface
}

[System.Serializable]
public class PreyTakeOffModule
{
    public float upForce      = 5f; // initial upward burst
    public float runForce     = 4f; // away-from-wren burst
    public float duration     = 2f; // seconds of burst before transitioning
    public float circleAfter  = 10f; // seconds of circling before returning to Calm
    public float circleHeight = 5f; // height offset above wren while circling
}

[System.Serializable]
public class PreyRunModule
{
    public bool  chaseInstead     = false; // if true, bird chases wren instead of fleeing (magpie)
    public float startleRadius    = 15f; // wren enters this → Disturbed
    public float fullRunRadius    = 5f; // wren enters this → max flee/chase force
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
    public float noticeChance             = 1f;   // checks per second when already within noticeRadius
    public float moveForce                = 2f;   // force toward the target while searching
    public float arrivalRadius            = 4f;   // how close = "arrived"
    public float giveUpTime              = 30f;  // give up if still not arrived after this long
}

[System.Serializable]
public class PreySprintModule
{
    public float maxSprintSpeed    = 0.4f;
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

    [Header( "Crystals" )]
    public PreyCrystalSettings crystals;

    [Header( "Spawn" )]
    public PreySpawnSettings spawn;

    [Header( "Despawn" )]
    public PreyDespawnSettings despawn;

    [Header( "Physics" )]
    public PreyPhysicsSettings  physics;
    public PreyTurningSettings  turning;

    [Header( "Core" )]
    public PreyScaleSettings    scale;
    public PreyMovementSettings movement;
    public PreyAltitudeSettings altitude;
    public PreyFlapSettings     flap;
    public PreyDistanceSettings distance;

    [Header( "Flavor" )]
    public PreyNoiseSettings noise;

    public PreyCircleSettings circle;
    public PreyFlockModule    flock;
    public PreySplineModule   spline;
    public PreyUpdraftModule  updraft;
    public PreyThermalModule  thermal;

    [Header( "Behavior" )]
    public PreyPerchModule    perch;
    public PreyTakeOffModule  takeOff;
    public PreyRunModule      run;
    public PreySprintModule   sprint;
    public PreyDriveModule    drive;
    public PreyCageModule     cage;
    public PreyAvoidanceModule avoidance;
    public PreySearchModule    search;
    public PreySocialModule    social;
}