using UnityEngine;

// Tunable parameters for a PreyManager. Scene references (colliders, transforms, splines, prefab,
// interest points) and runtime state stay on the PreyManager component — only value-type params
// and enums live here. The manager exposes same-named proxy properties that forward to this asset.
[CreateAssetMenu( fileName = "PreyManagerConfigSO" , menuName = "Prey/PreyManagerConfigSO" , order = 2 )]
public class PreyManagerConfigSO : ScriptableObject
{
    [Header( "Spawn Timing" )]
    public float spawnInterval      = 3f;
    public int   preyPerCluster     = 1;
    [Tooltip( "Radius (world units) the birds in a cluster scatter across around the spawn point. " +
              "0 = all on the exact same spot (they stack). Tightened by Cluster Closeness." )]
    public float clusterRadius      = 0f;
    public bool  spawnMaxOnWrenEnter = false;
    public bool  wrenEnterOnEnabled  = false;

    [Header( "Capacity" )]
    public int maxPray = 100;

    [Tooltip( "What to do when at maxPray. Despawn Old: gracefully fade out the oldest bird, then spawn " +
              "the replacement once it has finished despawning. Hold Til Despawned: never spawn until an " +
              "existing bird despawns on its own." )]
    public WhenFull whenFull = WhenFull.DespawnOld;

    [Header( "Spawn Placement" )]
    public SpawnType spawnType            = SpawnType.InsideBox;
    [Tooltip( "Spawn birds already resting in the Settled state, on the ground straight below the spawn " +
              "point (the placement decides X/Z; the bird drops to the ground). Ignored for On Point Of Interest." )]
    public bool      spawnSettled         = false;
    public float     spawnRadius          = 5f;
    public float     spawnDistanceMin     = 80f;           // InDistance ring
    public float     spawnDistanceMax     = 150f;
    [Range( 0f , 1f )]
    public float     spawnClosenessToBird = 0f;
    [Range( 0f , 1f )]
    [Tooltip( "How tightly birds in a multi-bird cluster pack together. 0 = scattered across Cluster " +
              "Radius; 1 = all on the same spot. (Only matters when Prey Per Cluster > 1.)" )]
    public float     clusterCloseness     = 0f;
    [Range( 0f , 1f )]
    [Tooltip( "How strongly a new spawn is pulled toward another existing prey (flock cohesion). 0 = spawn " +
              "at the placement spot; 1 = spawn right on a random existing prey." )]
    public float     spawnNearBirdImportance = 0f;
    [Tooltip( "Bias the spawn toward this distance from the wren (all non-POI spawn types). Pulls the " +
              "spawn point onto a sphere of this radius around the wren, keeping its direction." )]
    public float     spawnDesiredDistance           = 80f;
    [Range( 0f , 1f )]
    [Tooltip( "How strongly to pull the spawn to Desired Distance from the wren. 0 = ignore; " +
              "1 = exactly at that distance." )]
    public float     spawnDesiredDistanceImportance = 0f;
    [Tooltip( "Never spawn below the ground: if a computed spawn point is under the terrain, lift it to the " +
              "surface + clearance. (Does not apply to On Point Of Interest spawns, which place on surfaces.)" )]
    public bool      spawnAboveGround     = true;
    [Tooltip( "How far above the ground surface to place a spawn that would otherwise be underground." )]
    public float     spawnGroundClearance = 0.5f;
    [Tooltip( "What counts as ground for the spawn-above-ground check." )]
    public LayerMask spawnGroundLayers    = ~0;

    [Tooltip( "OnPointOfInterest: ideal distance IN FRONT of the wren for a spawn point." )]
    public float     poiIdealDistance        = 80f;
    [Tooltip( "OnPointOfInterest: tolerance radius around the ideal point (how far off-ideal still counts as close)." )]
    public float     poiIdealSpread          = 30f;
    [Tooltip( "OnPointOfInterest: 0 = random POI, 1 = strongly prefer the POI nearest the ideal point. " +
              "Mid values allow a few near-ideal points but exclude far ones." )]
    [Range( 0f , 1f )]
    public float     poiIdealDistanceWeight  = 0f;

    [Header( "Painted Region" )]
    [Tooltip( "Which food-map channels gate Painted spawn/despawn (R=0, G=1, B=2, A=3). " +
              "Multiple channels are OR'd: a spot counts as painted if ANY listed channel is above threshold." )]
    public int[] paintedChannels = { 0 };
    [Tooltip( "A channel counts as painted where its value is at least this." )]
    [Range( 0f , 1f )]
    public float paintedThreshold = 0.5f;

    [Header( "Despawn" )]
    public DespawnType    despawnType    = DespawnType.Distance;
    [Tooltip( "Collider/Region despawn: test the WREN's position (shared, cheap) or each PREY's own position." )]
    public DespawnSubject despawnSubject = DespawnSubject.Wren;
    public bool        despawnOnWrenExit        = true;
    public float       minimumTimeAlive         = 30f;
    [Tooltip( "Hard lifetime cap: once a bird has been alive longer than this it despawns no matter what " +
              "(ignores the wren-distance / region / collider tests). 0 = no cap." )]
    public float       maximumTimeAlive         = 0f;
    public float       timeOutsideBeforeDespawn = 5f;       // grace period once "outside" before despawning
    public float       distanceBeforeNotCaught  = 100f;     // Distance type: wren distance that counts as outside

    [Header( "Region Detection" )]
    public RegionType regionType          = RegionType.Box;
    public float      splineEnterDistance = 20f;
    public float      splineExitDistance  = 30f;
    public float      splineCheckInterval = 0.1f;

    [Header( "On Eat Effects" )]
    public float preyFullnessIncrease;
    public float preyStaminaIncrease;
    [Tooltip( "Which shared God.particleSystems effect to play where the prey was eaten. None = no particles." )]
    public GodParticleType gotAteParticle = GodParticleType.Eat;
}
