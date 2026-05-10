using UnityEngine;

// Magpies: territorial birds that perch nearby and CHASE the wren when it enters
// their territory. Retreat to their perch once the wren escapes far enough.
// Modules: perch + takeOff + run (chaseInstead=true) + altitude + turning + flap
// Manager setup: assign perchPoints (high vantage points); bugsPerCluster 1-3
[CreateAssetMenu( fileName = "MagpieConfigSO" , menuName = "Prey/MagpieConfigSO" , order = 8 )]
public class MagpieConfigSO : PreyConfigSO
{
    [ContextMenu( "Apply Magpie Defaults" )]
    private void ApplyDefaults()
    {
        modules.scale    = true;
        modules.despawn  = false;  // magpies always present in their territory
        modules.altitude = true;
        modules.turning  = true;
        modules.flap     = true;
        modules.noise    = false;
        modules.circle   = false;
        modules.flock    = false;
        modules.spline   = false;
        modules.updraft  = false;
        modules.thermal  = false;
        modules.perch    = true;
        modules.takeOff  = true;
        modules.run      = true;

        scale.maxScale          = 0.12f;
        scale.maxScaleStartLife = 1f;
        scale.maxScaleEndLife   = 0f;

        // quick and aggressive
        movement.speed                     = 0.14f;
        movement.maxAngleTurnBetweenFrames = 6f;

        altitude.desiredAltitude                = 8f;
        altitude.minAltitude                    = 4f;
        altitude.maxAltitude                    = 15f;
        altitude.strengthTowardsDesiredAltitude = 1.2f;

        physics.physicsResolution    = 1;
        physics.physicsInfoLerpSpeed = 0.12f;

        turning.groundTurnForce      = 1.5f;
        turning.forwardTurnForce     = 2f;
        turning.distanceForStartTurn = 20f;
        turning.distanceForHardTurn  = 6f;

        flap.flapSpeed           = 2f;
        flap.upBounceSize        = 0.8f;
        flap.forwardBounceSize   = 0.4f;
        flap.forwardBounceOffset = 0.3f;

        // perch on high vantage points, watching
        perch.snapDistance          = 1.5f;
        perch.startleRadius         = 18f;  // takes off the moment wren enters territory
        perch.landDesireInterval    = 12f;
        perch.landDesireVariance    = 5f;
        perch.perchDuration         = 20f;
        perch.perchDurationVariance = 8f;
        perch.approachSpeedMult     = 0.4f;
        perch.socialLandRadius      = 15f;
        perch.landingSpacing        = 3f;

        // aggressive launch
        takeOff.upForce      = 6f;
        takeOff.runForce     = 3f;
        takeOff.duration     = 0.8f;
        takeOff.circleAfter  = 0f;
        takeOff.circleHeight = 0f;

        // CHASE instead of flee — the key difference
        run.chaseInstead    = true;
        run.startleRadius   = 18f;
        run.fullRunRadius   = 5f;
        run.fleeForce       = 4f;
        run.speedMultiplier = 1.6f;
        run.jukeAmount      = 1.5f;   // lateral juke while chasing makes it feel alive
        run.jukeFrequency   = 1.2f;
        run.calmDownTime    = 3f;
        run.calmDownDistance = 20f;   // gives up once wren escapes past 20 units

        crystals.crystalsOnCollect = 40;
        crystals.crystalType       = 0.6f;

        spawn.spawnSpeed  = 1.5f;
        spawn.dieSpeed    = 1f;
        spawn.ateDieSpeed = 0.3f;

        distance.maxForwardDistance = 60f;
        distance.maxDownDistance    = 50f;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty( this );
#endif
        Debug.Log( "Magpie defaults applied." );
    }
}
