using UnityEngine;

// Storks: large birds that hang out in big groups on the ground / low perches.
// When the wren approaches they take off in one wave and fly away together.
// Modules: perch + takeOff + flock + altitude + turning + flap + run
// Manager setup: assign perchPoints (ground-level transforms), set bugsPerCluster high (6-15)
[CreateAssetMenu( fileName = "StorkConfigSO" , menuName = "Prey/StorkConfigSO" , order = 5 )]
public class StorkConfigSO : PreyConfigSO
{
    [ContextMenu( "Apply Stork Defaults" )]
    private void ApplyDefaults()
    {
        // modules
        modules.scale    = true;
        modules.despawn  = true;
        modules.altitude = true;
        modules.turning  = true;
        modules.flap     = true;
        modules.noise    = false;
        modules.circle   = false;
        modules.flock    = true;
        modules.spline   = false;
        modules.updraft  = false;
        modules.thermal  = false;
        modules.perch    = true;
        modules.takeOff  = true;
        modules.run      = true;

        // scale — big bird
        scale.maxScale          = 0.35f;
        scale.maxScaleStartLife = 1f;
        scale.maxScaleEndLife   = 0f;

        // movement — slow and deliberate
        movement.speed                     = 0.06f;
        movement.maxAngleTurnBetweenFrames = 2f;

        // altitude — flies low-to-mid
        altitude.desiredAltitude                = 6f;
        altitude.minAltitude                    = 3f;
        altitude.maxAltitude                    = 12f;
        altitude.strengthTowardsDesiredAltitude = 0.8f;

        // physics
        physics.physicsResolution    = 1;
        physics.physicsInfoLerpSpeed = 0.08f;

        // turning — patient obstacle avoidance
        turning.groundTurnForce      = 1.2f;
        turning.forwardTurnForce     = 1.5f;
        turning.distanceForStartTurn = 25f;
        turning.distanceForHardTurn  = 8f;

        // flap — slow heavy wingbeats
        flap.flapSpeed           = 0.6f;
        flap.upBounceSize        = 1.5f;
        flap.forwardBounceSize   = 0.3f;
        flap.forwardBounceOffset = 0.5f;

        // flock — stay close together
        flock.detectionRadius       = 25f;
        flock.separationRadius      = 4f;
        flock.separationForce       = 3f;
        flock.alignmentForce        = 1f;
        flock.cohesionForce         = 0.8f;
        flock.neighborQueryInterval = 0.4f;

        // perch — spend a long time on the ground, don't spook easily
        perch.snapDistance          = 2f;
        perch.startleRadius         = 20f;
        perch.landDesireInterval    = 15f;
        perch.landDesireVariance    = 8f;
        perch.perchDuration         = 30f;
        perch.perchDurationVariance = 15f;
        perch.approachSpeedMult     = 0.3f;
        perch.socialLandRadius      = 30f;
        perch.landingSpacing        = 5f;

        // takeOff — big burst, then fly away fast
        takeOff.upForce      = 8f;
        takeOff.runForce     = 5f;
        takeOff.duration     = 1.5f;
        takeOff.circleAfter  = 0f;   // no circling — just fly away
        takeOff.circleHeight = 0f;

        // run — flee as a group
        run.chaseInstead    = false;
        run.startleRadius   = 20f;
        run.fullRunRadius   = 10f;
        run.fleeForce       = 4f;
        run.speedMultiplier = 1.8f;
        run.jukeAmount      = 0.5f;   // storks don't juke much
        run.jukeFrequency   = 0.3f;
        run.calmDownTime    = 10f;
        run.calmDownDistance = 40f;

        // despawn — stays around for a while
        despawn.minimumTimeAlive         = 45f;
        despawn.distanceBeforeNotCaught  = 150f;

        // crystals
        crystals.crystalsOnCollect = 80;
        crystals.crystalType       = 0.7f;

        // spawn
        spawn.spawnSpeed  = 2f;
        spawn.dieSpeed    = 1.5f;
        spawn.ateDieSpeed = 0.5f;

        // distance
        distance.maxForwardDistance = 80f;
        distance.maxDownDistance    = 60f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty( this );
#endif
        Debug.Log( "Stork defaults applied." );
    }
}
