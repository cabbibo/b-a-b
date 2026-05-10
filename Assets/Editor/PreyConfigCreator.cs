using UnityEngine;
using UnityEditor;

public static class PreyConfigCreator
{
    const string kPath = "Assets/Resources/DesignConfigs/";

    [MenuItem( "Prey/Create All Config SOs" )]
    static void CreateAll()
    {
        CreateSongbird();
        CreateGoose();
        CreateMagpie();
        CreateStork();
        CreateRaven();
        CreateVulture();
        CreateButterfly();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log( "All prey configs created in " + kPath );
    }

    static T Make<T>( string name ) where T : PreyConfigSO
    {
        string path = kPath + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>( path );
        if ( existing != null ) {
            Debug.Log( name + " already exists, skipping." );
            return existing;
        }
        var so = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset( so, path );
        return so;
    }

    // ── Songbird ─────────────────────────────────────────────────────────────
    [MenuItem( "Prey/Create Songbird Config" )]
    static void CreateSongbird()
    {
        var c = Make<SongbirdConfigSO>( "SongbirdConfigSO" );

        c.modules.scale = true;  c.modules.despawn = true;   c.modules.altitude = true;
        c.modules.turning = true; c.modules.flap = true;     c.modules.noise = true;
        c.modules.circle = false; c.modules.flock = true;    c.modules.spline = false;
        c.modules.updraft = false; c.modules.thermal = false;
        c.modules.perch = true;   c.modules.takeOff = true;  c.modules.run = true;

        c.scale.maxScale = 0.05f;  c.scale.maxScaleStartLife = 1f;  c.scale.maxScaleEndLife = 0f;

        c.movement.speed = 0.15f;  c.movement.maxAngleTurnBetweenFrames = 8f;

        c.altitude.desiredAltitude = 5f;  c.altitude.minAltitude = 2f;
        c.altitude.maxAltitude = 10f;     c.altitude.strengthTowardsDesiredAltitude = 1.5f;

        c.physics.physicsResolution = 1;  c.physics.physicsInfoLerpSpeed = 0.15f;

        c.turning.groundTurnForce = 2f;   c.turning.forwardTurnForce = 3f;
        c.turning.distanceForStartTurn = 15f; c.turning.distanceForHardTurn = 4f;
        c.turning.bankStrength = 4f;      c.turning.bankSmoothing = 0.1f;

        c.flap.flapSpeed = 3f;  c.flap.upBounceSize = 0.5f;
        c.flap.forwardBounceSize = 0.3f;  c.flap.forwardBounceOffset = 0.2f;

        c.noise.noiseSpeed = 1.5f;  c.noise.noiseForce = 0.8f;  c.noise.noiseSize = 1f;

        c.flock.detectionRadius = 15f;   c.flock.separationRadius = 2f;
        c.flock.separationForce = 3f;    c.flock.alignmentForce = 0.5f;
        c.flock.cohesionForce = 0.4f;    c.flock.neighborQueryInterval = 0.3f;

        c.perch.snapDistance = 1f;       c.perch.startleRadius = 8f;
        c.perch.landDesireInterval = 8f; c.perch.landDesireVariance = 4f;
        c.perch.perchDuration = 5f;      c.perch.perchDurationVariance = 3f;
        c.perch.approachSpeedMult = 0.5f; c.perch.socialLandRadius = 10f;
        c.perch.landingSpacing = 1.5f;

        c.takeOff.upForce = 4f;   c.takeOff.runForce = 6f;
        c.takeOff.duration = 0.5f; c.takeOff.circleAfter = 0f;  c.takeOff.circleHeight = 0f;

        c.run.chaseInstead = false; c.run.startleRadius = 8f;  c.run.fullRunRadius = 3f;
        c.run.fleeForce = 5f;       c.run.speedMultiplier = 2f; c.run.jukeAmount = 3f;
        c.run.jukeFrequency = 2f;   c.run.calmDownTime = 5f;    c.run.calmDownDistance = 15f;

        c.despawn.minimumTimeAlive = 20f;  c.despawn.distanceBeforeNotCaught = 80f;

        c.crystals.crystalsOnCollect = 30;  c.crystals.crystalType = 0.3f;

        c.spawn.spawnSpeed = 1f;  c.spawn.dieSpeed = 0.8f;  c.spawn.ateDieSpeed = 0.2f;

        c.distance.maxForwardDistance = 40f;  c.distance.maxDownDistance = 30f;

        EditorUtility.SetDirty( c );
    }

    // ── Goose ─────────────────────────────────────────────────────────────────
    [MenuItem( "Prey/Create Goose Config" )]
    static void CreateGoose()
    {
        var c = Make<GooseConfigSO>( "GooseConfigSO" );

        c.modules.scale = true;  c.modules.despawn = false;  c.modules.altitude = true;
        c.modules.turning = true; c.modules.flap = true;     c.modules.noise = false;
        c.modules.circle = false; c.modules.flock = true;    c.modules.spline = true;
        c.modules.updraft = false; c.modules.thermal = false;
        c.modules.perch = false;  c.modules.takeOff = false; c.modules.run = true;

        c.scale.maxScale = 0.25f;  c.scale.maxScaleStartLife = 1f;  c.scale.maxScaleEndLife = 0f;

        c.movement.speed = 0.12f;  c.movement.maxAngleTurnBetweenFrames = 3f;

        c.altitude.desiredAltitude = 20f;  c.altitude.minAltitude = 15f;
        c.altitude.maxAltitude = 30f;      c.altitude.strengthTowardsDesiredAltitude = 1f;

        c.physics.physicsResolution = 2;  c.physics.physicsInfoLerpSpeed = 0.1f;

        c.turning.groundTurnForce = 1f;   c.turning.forwardTurnForce = 2f;
        c.turning.distanceForStartTurn = 40f; c.turning.distanceForHardTurn = 12f;
        c.turning.bankStrength = 2.5f;    c.turning.bankSmoothing = 0.06f;

        c.flap.flapSpeed = 0.8f;  c.flap.upBounceSize = 0.8f;
        c.flap.forwardBounceSize = 0.4f;  c.flap.forwardBounceOffset = 0.3f;

        c.flock.detectionRadius = 30f;   c.flock.separationRadius = 5f;
        c.flock.separationForce = 2f;    c.flock.alignmentForce = 2f;
        c.flock.cohesionForce = 0.4f;    c.flock.neighborQueryInterval = 0.6f;

        c.spline.pullRadius = 15f;   c.spline.pullForce = 1.5f;
        c.spline.returnForce = 4f;   c.spline.returnWhenCalm = true;

        c.run.chaseInstead = false; c.run.startleRadius = 12f; c.run.fullRunRadius = 6f;
        c.run.fleeForce = 2f;       c.run.speedMultiplier = 1.4f; c.run.jukeAmount = 1f;
        c.run.jukeFrequency = 0.5f; c.run.calmDownTime = 6f;   c.run.calmDownDistance = 20f;

        c.crystals.crystalsOnCollect = 60;  c.crystals.crystalType = 0.5f;

        c.spawn.spawnSpeed = 2f;  c.spawn.dieSpeed = 1f;  c.spawn.ateDieSpeed = 0.3f;

        c.distance.maxForwardDistance = 100f;  c.distance.maxDownDistance = 80f;

        EditorUtility.SetDirty( c );
    }

    // ── Magpie ────────────────────────────────────────────────────────────────
    [MenuItem( "Prey/Create Magpie Config" )]
    static void CreateMagpie()
    {
        var c = Make<MagpieConfigSO>( "MagpieConfigSO" );

        c.modules.scale = true;  c.modules.despawn = false;  c.modules.altitude = true;
        c.modules.turning = true; c.modules.flap = true;     c.modules.noise = false;
        c.modules.circle = false; c.modules.flock = false;   c.modules.spline = false;
        c.modules.updraft = false; c.modules.thermal = false;
        c.modules.perch = true;   c.modules.takeOff = true;  c.modules.run = true;

        c.scale.maxScale = 0.12f;  c.scale.maxScaleStartLife = 1f;  c.scale.maxScaleEndLife = 0f;

        c.movement.speed = 0.14f;  c.movement.maxAngleTurnBetweenFrames = 6f;

        c.altitude.desiredAltitude = 8f;   c.altitude.minAltitude = 4f;
        c.altitude.maxAltitude = 15f;      c.altitude.strengthTowardsDesiredAltitude = 1.2f;

        c.physics.physicsResolution = 1;  c.physics.physicsInfoLerpSpeed = 0.12f;

        c.turning.groundTurnForce = 1.5f;  c.turning.forwardTurnForce = 2f;
        c.turning.distanceForStartTurn = 20f; c.turning.distanceForHardTurn = 6f;
        c.turning.bankStrength = 5f;       c.turning.bankSmoothing = 0.08f;

        c.flap.flapSpeed = 2f;  c.flap.upBounceSize = 0.8f;
        c.flap.forwardBounceSize = 0.4f;  c.flap.forwardBounceOffset = 0.3f;

        c.perch.snapDistance = 1.5f;       c.perch.startleRadius = 18f;
        c.perch.landDesireInterval = 12f;  c.perch.landDesireVariance = 5f;
        c.perch.perchDuration = 20f;       c.perch.perchDurationVariance = 8f;
        c.perch.approachSpeedMult = 0.4f;  c.perch.socialLandRadius = 15f;
        c.perch.landingSpacing = 3f;

        c.takeOff.upForce = 6f;    c.takeOff.runForce = 3f;
        c.takeOff.duration = 0.8f; c.takeOff.circleAfter = 0f;  c.takeOff.circleHeight = 0f;

        c.run.chaseInstead = true;  c.run.startleRadius = 18f;  c.run.fullRunRadius = 5f;
        c.run.fleeForce = 4f;       c.run.speedMultiplier = 1.6f; c.run.jukeAmount = 1.5f;
        c.run.jukeFrequency = 1.2f; c.run.calmDownTime = 3f;    c.run.calmDownDistance = 20f;

        c.crystals.crystalsOnCollect = 40;  c.crystals.crystalType = 0.6f;

        c.spawn.spawnSpeed = 1.5f;  c.spawn.dieSpeed = 1f;  c.spawn.ateDieSpeed = 0.3f;

        c.distance.maxForwardDistance = 60f;  c.distance.maxDownDistance = 50f;

        EditorUtility.SetDirty( c );
    }

    // ── Stork ─────────────────────────────────────────────────────────────────
    [MenuItem( "Prey/Create Stork Config" )]
    static void CreateStork()
    {
        var c = Make<StorkConfigSO>( "StorkConfigSO" );

        c.modules.scale = true;  c.modules.despawn = true;   c.modules.altitude = true;
        c.modules.turning = true; c.modules.flap = true;     c.modules.noise = false;
        c.modules.circle = false; c.modules.flock = true;    c.modules.spline = false;
        c.modules.updraft = false; c.modules.thermal = false;
        c.modules.perch = true;   c.modules.takeOff = true;  c.modules.run = true;

        c.scale.maxScale = 0.35f;  c.scale.maxScaleStartLife = 1f;  c.scale.maxScaleEndLife = 0f;

        c.movement.speed = 0.06f;  c.movement.maxAngleTurnBetweenFrames = 2f;

        c.altitude.desiredAltitude = 6f;   c.altitude.minAltitude = 3f;
        c.altitude.maxAltitude = 12f;      c.altitude.strengthTowardsDesiredAltitude = 0.8f;

        c.physics.physicsResolution = 1;  c.physics.physicsInfoLerpSpeed = 0.08f;

        c.turning.groundTurnForce = 1.2f;  c.turning.forwardTurnForce = 1.5f;
        c.turning.distanceForStartTurn = 25f; c.turning.distanceForHardTurn = 8f;
        c.turning.bankStrength = 1.5f;     c.turning.bankSmoothing = 0.05f;

        c.flap.flapSpeed = 0.6f;  c.flap.upBounceSize = 1.5f;
        c.flap.forwardBounceSize = 0.3f;  c.flap.forwardBounceOffset = 0.5f;

        c.flock.detectionRadius = 25f;   c.flock.separationRadius = 4f;
        c.flock.separationForce = 3f;    c.flock.alignmentForce = 1f;
        c.flock.cohesionForce = 0.8f;    c.flock.neighborQueryInterval = 0.4f;

        c.perch.snapDistance = 2f;         c.perch.startleRadius = 20f;
        c.perch.landDesireInterval = 15f;  c.perch.landDesireVariance = 8f;
        c.perch.perchDuration = 30f;       c.perch.perchDurationVariance = 15f;
        c.perch.approachSpeedMult = 0.3f;  c.perch.socialLandRadius = 30f;
        c.perch.landingSpacing = 5f;

        c.takeOff.upForce = 8f;    c.takeOff.runForce = 5f;
        c.takeOff.duration = 1.5f; c.takeOff.circleAfter = 0f;  c.takeOff.circleHeight = 0f;

        c.run.chaseInstead = false; c.run.startleRadius = 20f;  c.run.fullRunRadius = 10f;
        c.run.fleeForce = 4f;       c.run.speedMultiplier = 1.8f; c.run.jukeAmount = 0.5f;
        c.run.jukeFrequency = 0.3f; c.run.calmDownTime = 10f;   c.run.calmDownDistance = 40f;

        c.despawn.minimumTimeAlive = 45f;  c.despawn.distanceBeforeNotCaught = 150f;

        c.crystals.crystalsOnCollect = 80;  c.crystals.crystalType = 0.7f;

        c.spawn.spawnSpeed = 2f;  c.spawn.dieSpeed = 1.5f;  c.spawn.ateDieSpeed = 0.5f;

        c.distance.maxForwardDistance = 80f;  c.distance.maxDownDistance = 60f;

        EditorUtility.SetDirty( c );
    }

    // ── Raven ─────────────────────────────────────────────────────────────────
    [MenuItem( "Prey/Create Raven Config" )]
    static void CreateRaven()
    {
        var c = Make<RavenConfigSO>( "RavenConfigSO" );

        c.modules.scale = true;  c.modules.despawn = true;   c.modules.altitude = true;
        c.modules.turning = true; c.modules.flap = true;     c.modules.noise = false;
        c.modules.circle = true;  c.modules.flock = true;    c.modules.spline = false;
        c.modules.updraft = false; c.modules.thermal = false;
        c.modules.perch = true;   c.modules.takeOff = true;  c.modules.run = false;

        c.scale.maxScale = 0.2f;  c.scale.maxScaleStartLife = 1f;  c.scale.maxScaleEndLife = 0f;

        c.movement.speed = 0.12f;  c.movement.maxAngleTurnBetweenFrames = 5f;

        c.altitude.desiredAltitude = 12f;  c.altitude.minAltitude = 8f;
        c.altitude.maxAltitude = 25f;      c.altitude.strengthTowardsDesiredAltitude = 1f;

        c.physics.physicsResolution = 1;  c.physics.physicsInfoLerpSpeed = 0.1f;

        c.turning.groundTurnForce = 1.2f;  c.turning.forwardTurnForce = 2f;
        c.turning.distanceForStartTurn = 25f; c.turning.distanceForHardTurn = 8f;
        c.turning.bankStrength = 5f;       c.turning.bankSmoothing = 0.08f;

        c.flap.flapSpeed = 1.5f;  c.flap.upBounceSize = 1.2f;
        c.flap.forwardBounceSize = 0.5f;  c.flap.forwardBounceOffset = 0.4f;

        c.circle.circleForce = 0.8f;  c.circle.circleRadius = 20f;

        c.flock.detectionRadius = 20f;   c.flock.separationRadius = 4f;
        c.flock.separationForce = 2f;    c.flock.alignmentForce = 1f;
        c.flock.cohesionForce = 0.4f;    c.flock.neighborQueryInterval = 0.5f;

        c.perch.snapDistance = 2f;         c.perch.startleRadius = 12f;
        c.perch.landDesireInterval = 15f;  c.perch.landDesireVariance = 7f;
        c.perch.perchDuration = 12f;       c.perch.perchDurationVariance = 6f;
        c.perch.approachSpeedMult = 0.4f;  c.perch.socialLandRadius = 15f;
        c.perch.landingSpacing = 3f;

        c.takeOff.upForce = 5f;    c.takeOff.runForce = 4f;
        c.takeOff.duration = 1f;   c.takeOff.circleAfter = 8f;  c.takeOff.circleHeight = 4f;

        c.despawn.minimumTimeAlive = 30f;  c.despawn.distanceBeforeNotCaught = 100f;

        c.crystals.crystalsOnCollect = 60;  c.crystals.crystalType = 0.4f;

        c.spawn.spawnSpeed = 2f;  c.spawn.dieSpeed = 1f;  c.spawn.ateDieSpeed = 0.3f;

        c.distance.maxForwardDistance = 80f;  c.distance.maxDownDistance = 60f;

        EditorUtility.SetDirty( c );
    }

    // ── Vulture ───────────────────────────────────────────────────────────────
    [MenuItem( "Prey/Create Vulture Config" )]
    static void CreateVulture()
    {
        var c = Make<VultureConfigSO>( "VultureConfigSO" );

        c.modules.scale = true;  c.modules.despawn = false;  c.modules.altitude = true;
        c.modules.turning = true; c.modules.flap = true;     c.modules.noise = false;
        c.modules.circle = true;  c.modules.flock = false;   c.modules.spline = false;
        c.modules.updraft = false; c.modules.thermal = true;
        c.modules.perch = true;   c.modules.takeOff = true;  c.modules.run = false;

        c.scale.maxScale = 0.45f;  c.scale.maxScaleStartLife = 1f;  c.scale.maxScaleEndLife = 0f;

        c.movement.speed = 0.07f;  c.movement.maxAngleTurnBetweenFrames = 1.5f;

        c.altitude.desiredAltitude = 60f;  c.altitude.minAltitude = 40f;
        c.altitude.maxAltitude = 80f;      c.altitude.strengthTowardsDesiredAltitude = 0.4f;

        c.physics.physicsResolution = 2;  c.physics.physicsInfoLerpSpeed = 0.07f;

        c.turning.groundTurnForce = 0.5f;  c.turning.forwardTurnForce = 0.8f;
        c.turning.distanceForStartTurn = 60f; c.turning.distanceForHardTurn = 20f;
        c.turning.bankStrength = 2f;       c.turning.bankSmoothing = 0.04f;

        c.flap.flapSpeed = 0.3f;  c.flap.upBounceSize = 2.5f;
        c.flap.forwardBounceSize = 0.3f;  c.flap.forwardBounceOffset = 0.5f;

        c.circle.circleForce = 0.4f;  c.circle.circleRadius = 50f;

        c.thermal.circleSpeed = 0.2f;  c.thermal.minAltitude = 30f;
        c.thermal.maxAltitude = 80f;   c.thermal.thermalTightness = 0.6f;

        c.perch.snapDistance = 2.5f;       c.perch.startleRadius = 25f;
        c.perch.landDesireInterval = 20f;  c.perch.landDesireVariance = 10f;
        c.perch.perchDuration = 30f;       c.perch.perchDurationVariance = 15f;
        c.perch.approachSpeedMult = 0.2f;  c.perch.socialLandRadius = 25f;
        c.perch.landingSpacing = 5f;

        c.takeOff.upForce = 10f;   c.takeOff.runForce = 3f;
        c.takeOff.duration = 2.5f; c.takeOff.circleAfter = 15f; c.takeOff.circleHeight = 8f;

        c.crystals.crystalsOnCollect = 120;  c.crystals.crystalType = 0.9f;

        c.spawn.spawnSpeed = 4f;  c.spawn.dieSpeed = 2f;  c.spawn.ateDieSpeed = 0.5f;

        c.distance.maxDistanceStart = 150f; c.distance.maxDistanceEnd = 250f;
        c.distance.maxForwardDistance = 200f; c.distance.maxDownDistance = 200f;

        EditorUtility.SetDirty( c );
    }

    // ── Butterfly ─────────────────────────────────────────────────────────────
    [MenuItem( "Prey/Create Butterfly Config" )]
    static void CreateButterfly()
    {
        var c = Make<ButterflyConfigSO>( "ButterflyConfigSO" );

        c.modules.scale = true;  c.modules.despawn = true;   c.modules.altitude = true;
        c.modules.turning = true; c.modules.flap = true;     c.modules.noise = true;
        c.modules.circle = false; c.modules.flock = false;   c.modules.spline = false;
        c.modules.updraft = false; c.modules.thermal = false;
        c.modules.perch = true;   c.modules.takeOff = false; c.modules.run = false;

        c.scale.maxScale = 0.03f;  c.scale.maxScaleStartLife = 1f;  c.scale.maxScaleEndLife = 0f;

        c.movement.speed = 0.05f;  c.movement.maxAngleTurnBetweenFrames = 10f;

        c.altitude.desiredAltitude = 2f;  c.altitude.minAltitude = 1f;
        c.altitude.maxAltitude = 5f;      c.altitude.strengthTowardsDesiredAltitude = 1.5f;

        c.physics.physicsResolution = 1;  c.physics.physicsInfoLerpSpeed = 0.15f;

        c.turning.groundTurnForce = 1.5f;  c.turning.forwardTurnForce = 2f;
        c.turning.distanceForStartTurn = 10f; c.turning.distanceForHardTurn = 3f;
        c.turning.bankStrength = 1f;       c.turning.bankSmoothing = 0.15f;

        c.flap.flapSpeed = 4f;  c.flap.upBounceSize = 0.3f;
        c.flap.forwardBounceSize = 0.1f;  c.flap.forwardBounceOffset = 0.3f;

        c.noise.noiseSize = 1.5f;  c.noise.noiseSpeed = 2f;  c.noise.noiseForce = 1.2f;

        c.perch.anchorRadius = 8f;         c.perch.anchorPullForce = 0.5f;
        c.perch.snapDistance = 0.5f;       c.perch.startleRadius = 5f;
        c.perch.landDesireInterval = 5f;   c.perch.landDesireVariance = 3f;
        c.perch.perchDuration = 3f;        c.perch.perchDurationVariance = 2f;
        c.perch.approachSpeedMult = 0.6f;  c.perch.socialLandRadius = 5f;
        c.perch.landingSpacing = 0.5f;

        c.despawn.minimumTimeAlive = 20f;  c.despawn.distanceBeforeNotCaught = 60f;

        c.crystals.crystalsOnCollect = 20;  c.crystals.crystalType = 0.2f;

        c.spawn.spawnSpeed = 1f;  c.spawn.dieSpeed = 0.5f;  c.spawn.ateDieSpeed = 0.15f;

        c.distance.maxForwardDistance = 20f;  c.distance.maxDownDistance = 10f;

        EditorUtility.SetDirty( c );
    }
}
