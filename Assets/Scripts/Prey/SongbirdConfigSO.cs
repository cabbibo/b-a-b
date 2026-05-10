using UnityEngine;

// Songbirds: small, quick, fluttery birds. Perch frequently, hop around,
// spook easily and dart away fast. Lots of noise in their movement.
// Modules: noise + perch + takeOff + flock + altitude + turning + flap + run
// Manager setup: assign perchPoints (branches, rocks); bugsPerCluster 2-6
[CreateAssetMenu( fileName = "SongbirdConfigSO" , menuName = "Prey/SongbirdConfigSO" , order = 7 )]
public class SongbirdConfigSO : PreyConfigSO
{
    [ContextMenu( "Apply Songbird Defaults" )]
    private void ApplyDefaults()
    {
        modules.scale    = true;
        modules.despawn  = true;
        modules.altitude = true;
        modules.turning  = true;
        modules.flap     = true;
        modules.noise    = true;
        modules.circle   = false;
        modules.flock    = true;
        modules.spline   = false;
        modules.updraft  = false;
        modules.thermal  = false;
        modules.perch    = true;
        modules.takeOff  = true;
        modules.run      = true;

        // tiny
        scale.maxScale          = 0.05f;
        scale.maxScaleStartLife = 1f;
        scale.maxScaleEndLife   = 0f;

        // fast and twitchy
        movement.speed                     = 0.15f;
        movement.maxAngleTurnBetweenFrames = 8f;

        altitude.desiredAltitude                = 5f;
        altitude.minAltitude                    = 2f;
        altitude.maxAltitude                    = 10f;
        altitude.strengthTowardsDesiredAltitude = 1.5f;

        physics.physicsResolution    = 1;
        physics.physicsInfoLerpSpeed = 0.15f;

        turning.groundTurnForce      = 2f;
        turning.forwardTurnForce     = 3f;
        turning.distanceForStartTurn = 15f;
        turning.distanceForHardTurn  = 4f;

        // fast chaotic flap
        flap.flapSpeed           = 3f;
        flap.upBounceSize        = 0.5f;
        flap.forwardBounceSize   = 0.3f;
        flap.forwardBounceOffset = 0.2f;

        // jittery noise movement
        noise.noiseSpeed = 1.5f;
        noise.noiseForce = 0.8f;
        noise.noiseSize  = 1f;

        // loose flock
        flock.detectionRadius       = 15f;
        flock.separationRadius      = 2f;
        flock.separationForce       = 3f;
        flock.alignmentForce        = 0.5f;
        flock.cohesionForce         = 0.4f;
        flock.neighborQueryInterval = 0.3f;

        // perch often and briefly
        perch.snapDistance          = 1f;
        perch.startleRadius         = 8f;
        perch.landDesireInterval    = 8f;
        perch.landDesireVariance    = 4f;
        perch.perchDuration         = 5f;
        perch.perchDurationVariance = 3f;
        perch.approachSpeedMult     = 0.5f;
        perch.socialLandRadius      = 10f;
        perch.landingSpacing        = 1.5f;

        // quick pop off perch
        takeOff.upForce      = 4f;
        takeOff.runForce     = 6f;
        takeOff.duration     = 0.5f;
        takeOff.circleAfter  = 0f;
        takeOff.circleHeight = 0f;

        // very easily spooked, darts away fast
        run.chaseInstead    = false;
        run.startleRadius   = 8f;
        run.fullRunRadius   = 3f;
        run.fleeForce       = 5f;
        run.speedMultiplier = 2f;
        run.jukeAmount      = 3f;
        run.jukeFrequency   = 2f;
        run.calmDownTime    = 5f;
        run.calmDownDistance = 15f;

        despawn.minimumTimeAlive        = 20f;
        despawn.distanceBeforeNotCaught = 80f;

        crystals.crystalsOnCollect = 30;
        crystals.crystalType       = 0.3f;

        spawn.spawnSpeed  = 1f;
        spawn.dieSpeed    = 0.8f;
        spawn.ateDieSpeed = 0.2f;

        distance.maxForwardDistance = 40f;
        distance.maxDownDistance    = 30f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty( this );
#endif
        Debug.Log( "Songbird defaults applied." );
    }
}
