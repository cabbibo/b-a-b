using UnityEngine;

// ─── Module toggles ──────────────────────────────────────────────────────────

[System.Serializable]
public class PreyModuleFlags
{
    [Header( "Core" )]
    public bool scale = true;

    public bool despawn  = true;
    public bool altitude = true;
    public bool turning  = true;
    public bool flap     = true;

    [Header( "Flavor" )]
    public bool noise = false;

    public bool circle  = false;
    public bool flock   = false;
    public bool spline  = false;
    public bool updraft = false;
    public bool thermal = false;

    [Header( "Behavior" )]
    public bool perch = false;

    public bool takeOff = false;
    public bool run     = false;
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
    public float speed = .1f;
    public float forwardSpeed;
    public float maxAngleTurnBetweenFrames = 4f;
    public float minimumDotProductMatchForTurn;
}

[System.Serializable]
public class PreyDespawnSettings
{
    public float minimumTimeAlive                            = 30f;
    public float distanceBeforeNotCaught                     = 100f;
    public float timeOutsideDistanceBeforeNotCaughtTriggered = 5f;
}

[System.Serializable]
public class PreyAltitudeSettings
{
    public float desiredAltitude                = 10;
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
    public float groundTurnForce      = 1;
    public float forwardTurnForce     = 1;
    public float distanceForStartTurn = 30;
    public float distanceForHardTurn  = 10;
    public float bankStrength         = 5f;
    public float bankSmoothing        = 0.08f;
}

[System.Serializable]
public class PreyFlapSettings
{
    public float flapSpeed           = 1;
    public float upBounceSize        = 1f;
    public float forwardBounceSize   = .5f;
    public float forwardBounceOffset = .5f;
}

[System.Serializable]
public class PreyCrystalSettings
{
    public int   crystalsOnCollect = 50;
    public float crystalType       = .5f;
}

[System.Serializable]
public class PreySpawnSettings
{
    public float spawnSpeed  = 3f;
    public float dieSpeed    = 1f;
    public float ateDieSpeed = .3f;
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
    public float pullRadius     = 20f; // distance from spline before pull kicks in
    public float pullForce      = 1f; // force when inside corridor — pushes along path
    public float returnForce    = 3f; // force when outside corridor — pulls back to path
    public bool  returnWhenCalm = true;
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
    public float perchDuration         = 10f; // how long to stay perched
    public float perchDurationVariance = 5f;
    public float approachSpeedMult     = 0.4f; // speed fraction when landing
    public float socialLandRadius      = 20f; // if another bird lands nearby, also want to
    public float landingSpacing        = 3f; // min gap between landed birds
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


// ─── ScriptableObject ────────────────────────────────────────────────────────

[CreateAssetMenu( fileName = "PreyConfigSO" , menuName = "Prey/PreyConfigSO" , order = 1 )]
public class PreyConfigSO : ScriptableObject
{
    [Header( "Modules" )]
    public PreyModuleFlags modules;

    [Header( "Core" )]
    public PreyScaleSettings scale;

    public PreyMovementSettings movement;
    public PreyDespawnSettings  despawn;
    public PreyAltitudeSettings altitude;
    public PreyPhysicsSettings  physics;
    public PreyTurningSettings  turning;
    public PreyFlapSettings     flap;
    public PreyCrystalSettings  crystals;
    public PreySpawnSettings    spawn;
    public PreyDistanceSettings distance;

    [Header( "Flavor" )]
    public PreyNoiseSettings noise;

    public PreyCircleSettings circle;
    public PreyFlockModule    flock;
    public PreySplineModule   spline;
    public PreyUpdraftModule  updraft;
    public PreyThermalModule  thermal;

    [Header( "Behavior" )]
    public PreyPerchModule perch;

    public PreyTakeOffModule takeOff;
    public PreyRunModule     run;
}