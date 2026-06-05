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
    public float     spawnRadius          = 5f;
    public float     spawnDistanceMin     = 80f;           // InDistance ring
    public float     spawnDistanceMax     = 150f;
    [Range( 0f , 1f )]
    public float     spawnClosenessToBird = 0f;

    [Header( "Despawn" )]
    public DespawnType despawnType              = DespawnType.Distance;
    public bool        despawnOnWrenExit        = true;
    public float       minimumTimeAlive         = 30f;
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
