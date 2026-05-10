using UnityEngine;

// Geese: fly in formation along a spline path. Disturbed when wren gets close
// but want to return to the spline after calming. Constant fliers, never land.
// Modules: spline + flock + altitude + turning + flap + run
// Manager setup: assign a PreySpline in the scene; set bugsPerCluster to 5-12
[CreateAssetMenu( fileName = "GooseConfigSO" , menuName = "Prey/GooseConfigSO" , order = 6 )]
public class GooseConfigSO : PreyConfigSO
{
    [ContextMenu( "Apply Goose Defaults" )]
    private void ApplyDefaults()
    {
        modules.scale    = true;
        modules.despawn  = false;  // geese disappear only when they reach spline end
        modules.altitude = true;
        modules.turning  = true;
        modules.flap     = true;
        modules.noise    = false;
        modules.circle   = false;
        modules.flock    = true;
        modules.spline   = true;
        modules.updraft  = false;
        modules.thermal  = false;
        modules.perch    = false;
        modules.takeOff  = false;
        modules.run      = true;

        scale.maxScale          = 0.25f;
        scale.maxScaleStartLife = 1f;
        scale.maxScaleEndLife   = 0f;

        // movement — steady medium speed
        movement.speed                     = 0.12f;
        movement.maxAngleTurnBetweenFrames = 3f;

        // altitude — flies mid-high
        altitude.desiredAltitude                = 20f;
        altitude.minAltitude                    = 15f;
        altitude.maxAltitude                    = 30f;
        altitude.strengthTowardsDesiredAltitude = 1f;

        physics.physicsResolution    = 2;
        physics.physicsInfoLerpSpeed = 0.1f;

        turning.groundTurnForce      = 1f;
        turning.forwardTurnForce     = 2f;
        turning.distanceForStartTurn = 40f;
        turning.distanceForHardTurn  = 12f;

        // flap — medium rate, glide between flaps
        flap.flapSpeed           = 0.8f;
        flap.upBounceSize        = 0.8f;
        flap.forwardBounceSize   = 0.4f;
        flap.forwardBounceOffset = 0.3f;

        // flock — loose V-ish formation
        flock.detectionRadius       = 30f;
        flock.separationRadius      = 5f;
        flock.separationForce       = 2f;
        flock.alignmentForce        = 2f;   // strong alignment keeps the V shape
        flock.cohesionForce         = 0.4f;
        flock.neighborQueryInterval = 0.6f;

        // spline — strong pull back to path after disturbance
        spline.pullRadius    = 15f;
        spline.pullForce     = 1.5f;
        spline.returnForce   = 4f;
        spline.returnWhenCalm = true;

        // run — disturbed by wren but will return to spline
        run.chaseInstead    = false;
        run.startleRadius   = 12f;
        run.fullRunRadius   = 6f;
        run.fleeForce       = 2f;
        run.speedMultiplier = 1.4f;
        run.jukeAmount      = 1f;
        run.jukeFrequency   = 0.5f;
        run.calmDownTime    = 6f;
        run.calmDownDistance = 20f;

        crystals.crystalsOnCollect = 60;
        crystals.crystalType       = 0.5f;

        spawn.spawnSpeed  = 2f;
        spawn.dieSpeed    = 1f;
        spawn.ateDieSpeed = 0.3f;

        distance.maxForwardDistance = 100f;
        distance.maxDownDistance    = 80f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty( this );
#endif
        Debug.Log( "Goose defaults applied." );
    }
}
