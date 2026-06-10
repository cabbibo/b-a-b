using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using WrenUtils;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum RegionType
{
    Box ,
    Collider ,
    Spline ,
    Painted
}

public enum DespawnType
{
    Distance , // despawn when far enough from the wren (distance + grace time)
    Collider , // despawn when outside a dedicated despawn collider
    Region     // despawn when the region is exited (box / collider / painted)
}

// Whether Collider / Region despawn is tested against the wren or each prey's own position.
public enum DespawnSubject
{
    Wren , // shared test against the wren — one test/frame for all prey (cheap, default)
    Prey   // per-prey test against each bird's own position
}

public enum WhenFull
{
    DespawnOld , // at max: fade out the oldest bird, then spawn the new one once it's gone
    HoldTilDespawned // at max: never spawn — wait until existing birds despawn on their own
}


public class PreyManager : MonoBehaviour
{
    /*

        Can spawn in clumps for butterflies etc.

    */

    // ── Debug (lives on the component, not the shared config — per-instance / per-session) ──────
    [Header( "Debug" )]
    public Transform debugWren;

    public bool stepThrough = false;

    [Range( 0.01f , 1f )]
    public float simulationSpeed = 1f;

    public bool showRegionEntrance     = true; // region box / spline enter-exit gizmos
    public bool showInterestPointDebug = true; // interest-point markers (via PreyManagerDebug)
    public bool showDespawnDebug       = true; // per-prey despawn decision viz (on selected prey)
    public bool showPaintedDebug       = true; // Painted spawn/despawn: contour the configured food channel(s)
    [Range( 16 , 256 )]
    public int  paintedDebugResolution = 96;
    public bool showSpawnPointDebug    = true; // OnPointOfInterest: ring-mark the toggled spawn POIs
    public bool showPOIRegionDebug     = true; // OnPointOfInterest: label each spawn POI in/out of THIS region

    // ── Tunable params live on this asset; scene refs + runtime stay on the component ──────────
    [Header( "Manager Config" )]
    public PreyManagerConfigSO managerConfig;

    [Header( "Scene References" )]
    public PreyInterestPoint[] interestPoints; // perch spots, thermals, anchors, updrafts, investigate points

    // OnPointOfInterest spawn: which of the above interest points to spawn at (toggled in the
    // custom PreyManager inspector). Perch → spawn landed, Updraft → circling, others → sphere.
    public List<PreyInterestPoint> spawnAtPoints = new List<PreyInterestPoint>();
    private readonly List<PreyInterestPoint> _poiPick    = new List<PreyInterestPoint>();
    private readonly List<float>             _poiWeights = new List<float>();

    [Header( "Config" )]
    public PreyConfigSO preyConfig; // the prey's params

    public GameObject preyPrefab; // what to spawn

    [Header( "Scene Wiring" )]
    public Collider despawnCollider; // Collider despawn type: outside this → despawn

    public Transform       preyHolder;
    public Transform       boxRegion; // Box region
    public Collider        regionCollider; // Collider region
    public SplineContainer regionSpline; // Spline region

    // ── Runtime state ─────────────────────────────────────────────────────────────────────────
    public bool              birdInsideRegion;
    public List<Transform[]> clusters;
    public float             lastSpawnTime;
    public int               currentNumberOfPrey;

    [HideInInspector]
    public bool wrenOutsideRegion;          // computed once/frame; Region despawn (Wren subject)
    [HideInInspector]
    public bool wrenOutsideDespawnCollider; // computed once/frame; Collider despawn (Wren subject)

    private Vector3   splineBoundsCenter;
    private float     splineBoundingRadius;
    private Coroutine splineCheckCoroutine;

    // ── Neighbor spatial hash (rebuilt once/frame; used by GetNearbyBirds) ─────
    private PreySpatialGrid _neighborGrid;
    private int             _gridFrame = -1;

    // ── Baked spline lookup table (shared by all birds following regionSpline) ─
    [Header( "Spline Cache" )]
    [Tooltip( "Samples baked along regionSpline for fast nearest-point lookups. Higher = more accurate, one-time bake cost only." )]
    public int splineCacheSamples = 128;
    private PreySplineCache _splineCache;

    // ── Proxy properties: forward to managerConfig, null-safe with the old defaults ────────────
    public float spawnInterval => managerConfig != null ? managerConfig.spawnInterval : 3f;
    public int preyPerCluster => managerConfig != null ? managerConfig.preyPerCluster : 1;
    public float clusterRadius => managerConfig != null ? managerConfig.clusterRadius : 0f;
    public bool spawnMaxOnWrenEnter => managerConfig != null ? managerConfig.spawnMaxOnWrenEnter : false;
    public bool wrenEnterOnEnabled => managerConfig != null ? managerConfig.wrenEnterOnEnabled : false;

    public int maxPray => managerConfig != null ? managerConfig.maxPray : 100;
    public WhenFull whenFull => managerConfig != null ? managerConfig.whenFull : WhenFull.DespawnOld;

    public SpawnType spawnType => managerConfig != null ? managerConfig.spawnType : SpawnType.InsideBox;
    public float spawnRadius => managerConfig != null ? managerConfig.spawnRadius : 5f;
    public float spawnDistanceMin => managerConfig != null ? managerConfig.spawnDistanceMin : 80f;
    public float spawnDistanceMax => managerConfig != null ? managerConfig.spawnDistanceMax : 150f;
    public float spawnClosenessToBird => managerConfig != null ? managerConfig.spawnClosenessToBird : 0f;

    public int[] paintedChannels  => managerConfig != null ? managerConfig.paintedChannels  : null;
    public float paintedThreshold => managerConfig != null ? managerConfig.paintedThreshold : 0.5f;

    public float poiIdealDistance       => managerConfig != null ? managerConfig.poiIdealDistance       : 80f;
    public float poiIdealSpread         => managerConfig != null ? managerConfig.poiIdealSpread         : 30f;
    public float poiIdealDistanceWeight => managerConfig != null ? managerConfig.poiIdealDistanceWeight : 0f;

    public DespawnType    despawnType    => managerConfig != null ? managerConfig.despawnType    : DespawnType.Distance;
    public DespawnSubject despawnSubject => managerConfig != null ? managerConfig.despawnSubject : DespawnSubject.Wren;
    public bool despawnOnWrenExit => managerConfig != null ? managerConfig.despawnOnWrenExit : true;
    public float minimumTimeAlive => managerConfig != null ? managerConfig.minimumTimeAlive : 30f;
    public float timeOutsideBeforeDespawn => managerConfig != null ? managerConfig.timeOutsideBeforeDespawn : 5f;
    public float distanceBeforeNotCaught => managerConfig != null ? managerConfig.distanceBeforeNotCaught : 100f;

    public RegionType regionType => managerConfig != null ? managerConfig.regionType : RegionType.Box;
    public float splineEnterDistance => managerConfig != null ? managerConfig.splineEnterDistance : 20f;
    public float splineExitDistance => managerConfig != null ? managerConfig.splineExitDistance : 30f;
    public float splineCheckInterval => managerConfig != null ? managerConfig.splineCheckInterval : 0.1f;

    public float preyFullnessIncrease => managerConfig != null ? managerConfig.preyFullnessIncrease : 0f;
    public float preyStaminaIncrease => managerConfig != null ? managerConfig.preyStaminaIncrease : 0f;
    public GodParticleType gotAteParticle => managerConfig != null ? managerConfig.gotAteParticle : GodParticleType.Eat;


    public virtual void OnEnable()
    {
        if ( managerConfig == null ) {
            return; // no params assigned → manager is inert
        }

        lastSpawnTime = Time.time - spawnInterval;
        while (preyHolder.childCount > 0) DestroyImmediate( preyHolder.GetChild( 0 ).gameObject );

        if ( wrenEnterOnEnabled ) {
            OnWrenEnter();
        }

        if ( regionType == RegionType.Spline ) {
            CacheSplineBounds();
            splineCheckCoroutine = StartCoroutine( SplineCheckRoutine() );
        }

        // bake the spline LUT up front (birds may follow regionSpline regardless of region type)
        if ( regionSpline != null ) RebakeSplineCache();
    }

    private void OnDisable()
    {
        if ( splineCheckCoroutine != null ) {
            StopCoroutine( splineCheckCoroutine );
            splineCheckCoroutine = null;
        }
    }

    private void Update()
    {
        PreyProfiler.FrameGate( Time.frameCount );
        long __t0 = PreyProfiler.Now;
        UpdateManager();
        PreyProfiler.managerTicks += PreyProfiler.Now - __t0;
    }

    private void UpdateManager()
    {
        if ( managerConfig == null ) {
            return; // no params assigned → manager is inert
        }

        currentNumberOfPrey = preyHolder.childCount;
        BuildNeighborGrid();

        // Update region membership FIRST so spawning this frame sees the current value.
        if ( regionType == RegionType.Box ) {
            CheckBoxRegion();
        } else if ( regionType == RegionType.Collider ) {
            CheckColliderRegion();
        } else if ( regionType == RegionType.Painted ) {
            CheckPaintedRegion();
        }

        // Wren-subject despawn tests: compute ONCE here; every prey reads them (no per-prey work).
        var wp = GetWrenPosition();
        wrenOutsideRegion          = wp.HasValue && IsOutsideRegion( wp.Value );
        wrenOutsideDespawnCollider = wp.HasValue && IsOutsideDespawnCollider( wp.Value );

        CheckForNewPrey();
    }

    // Painted region membership: the wren is "in" the region when it's standing on painted ground.
    private void CheckPaintedRegion()
    {
        var wp = GetWrenPosition();
        if ( !wp.HasValue ) return;

        bool inside = IsPainted( wp.Value );
        if ( !birdInsideRegion && inside ) OnWrenEnter();
        else if ( birdInsideRegion && !inside ) OnWrenExit();
    }

    private void CheckBoxRegion()
    {
        if ( boxRegion == null ) {
            return;
        }

        var wrenT = God.wren != null ? God.wren.transform
            : debugWren != null ? debugWren
            : null;

        if ( wrenT == null ) {
            return;
        }

        var wrenPos = wrenT.position;
        var half = boxRegion.lossyScale * 0.5f;
        var center = boxRegion.position;
        bool inside = wrenPos.x >= center.x - half.x && wrenPos.x <= center.x + half.x
                                                     && wrenPos.y >= center.y - half.y && wrenPos.y <= center.y + half.y
                                                     && wrenPos.z >= center.z - half.z && wrenPos.z <= center.z + half.z;

        if ( !birdInsideRegion && inside ) {
            OnWrenEnter();
        } else if ( birdInsideRegion && !inside ) {
            OnWrenExit();
        }
    }

    private void CheckColliderRegion()
    {
        if ( regionCollider == null ) {
            return;
        }

        var wrenT = God.wren != null ? God.wren.transform
            : debugWren != null ? debugWren
            : null;

        if ( wrenT == null ) {
            return;
        }

        var wrenPos = wrenT.position;
        bool isInside = (regionCollider.ClosestPoint( wrenPos ) - wrenPos).sqrMagnitude < 0.001f;

        if ( !birdInsideRegion && isInside ) {
            OnWrenEnter();
        } else if ( birdInsideRegion && !isInside ) {
            OnWrenExit();
        }
    }

    // Despawn helpers ────────────────────────────────────────────────────────
    // Is a world position outside this manager's region (box / collider / painted)? Used by Region despawn.
    public bool IsOutsideRegion( Vector3 pos )
    {
        if ( regionType == RegionType.Box && boxRegion != null ) {
            var half = boxRegion.lossyScale * 0.5f;
            var c = boxRegion.position;
            return pos.x < c.x - half.x || pos.x > c.x + half.x
                                        || pos.y < c.y - half.y || pos.y > c.y + half.y
                                        || pos.z < c.z - half.z || pos.z > c.z + half.z;
        }

        if ( regionType == RegionType.Collider && regionCollider != null ) {
            return (regionCollider.ClosestPoint( pos ) - pos).sqrMagnitude > 0.0001f;
        }

        // Painted region: "outside" = the spot isn't painted by any of our channels.
        if ( regionType == RegionType.Painted ) {
            return !IsPainted( pos );
        }

        return false;
    }

    // Is a world position inside this manager's region (box / collider / painted)?
    // Spline / unset → no spatial filter (always inside).
    public bool IsInsideRegion( Vector3 pos )
    {
        switch ( regionType ) {
            case RegionType.Box:
            case RegionType.Collider:
            case RegionType.Painted:
                return !IsOutsideRegion( pos );
            default:
                return true;
        }
    }

    // Is a world position outside the dedicated despawn collider? Used by Collider despawn.
    public bool IsOutsideDespawnCollider( Vector3 pos )
    {
        if ( despawnCollider == null ) {
            return true;
        }

        return (despawnCollider.ClosestPoint( pos ) - pos).sqrMagnitude > 0.0001f;
    }

    public virtual void CheckForNewPrey()
    {
        if ( managerConfig == null || preyConfig == null || preyPrefab == null ) {
            return;
        }

        if ( Time.time - lastSpawnTime <= spawnInterval ) {
            return;
        }

        // Spawning is gated by region membership for EVERY spawn type. For a Painted region,
        // "inside" means the wren is on painted ground (see CheckPaintedRegion).
        if ( birdInsideRegion ) {
            SpawnNewBug();
        }
    }


    public void OnWrenEnter()
    {
        birdInsideRegion = true;

        if ( spawnMaxOnWrenEnter ) {
            // Bounded: stop if a spawn attempt adds nothing (e.g. no valid POI in the wren's chunk
            // this frame) — otherwise this loops forever and hangs the editor.
            int guard = maxPray + 8;
            while ( preyHolder.childCount < maxPray && guard-- > 0 ) {
                int before = preyHolder.childCount;
                SpawnNewBug();
                if ( preyHolder.childCount <= before ) break;
            }
        }
    }

    public void OnWrenExit()
    {
        birdInsideRegion = false;

        if ( !despawnOnWrenExit ) {
            return;
        }

        for ( int i = 0; i < preyHolder.childCount; i++ ) {
            var prey = preyHolder.GetChild( i ).GetComponent<PreyController>();

            if ( prey != null ) {
                prey.ForceDespawn();
            }
        }
    }


    public virtual void SpawnNewBug()
    {
        // at capacity: don't spawn yet. DespawnOld starts the oldest bird fading out (one at a time)
        // so a slot frees up; HoldTilDespawned just waits for natural despawns. The new bird spawns
        // on a later tick once childCount actually drops below maxPray.
        if ( preyHolder.childCount >= maxPray ) {
            if ( whenFull == WhenFull.DespawnOld && !AnyPreyDespawning() ) {
                var oldest = OldestLivePrey();

                if ( oldest != null ) {
                    oldest.ForceDespawn();
                }
            }

            return;
        }


        // OnPointOfInterest does its own placement + state setup per bird, so it bypasses the
        // generic single-position spawn loop below.
        if ( spawnType == SpawnType.OnPointOfInterest ) { SpawnOnPointOfInterest(); return; }

        Vector3 spawnPos;

        switch (spawnType) {
            case SpawnType.NextToCurve: spawnPos = SpawnNextToCurve(); break;
            case SpawnType.Painted:
                if ( !TryGetPaintedSpawn( out spawnPos ) ) return; // no painted area ahead → skip this tick
                break;
            case SpawnType.DesiredAltitude: spawnPos = SpawnAtDesiredAltitude(); break;
            case SpawnType.InDistance: spawnPos = SpawnInDistance(); break;
            case SpawnType.InsideBox:
            default: spawnPos = SpawnInsideBox(); break;
        }

        for ( int i = 0; i < preyPerCluster; i++ ) {
            spawnPos += Random.insideUnitSphere * clusterRadius;

            var newPrey = Instantiate( preyPrefab , spawnPos , Quaternion.identity ).GetComponent<PreyController>();
            newPrey.Initialize( preyConfig , this );
            newPrey.transform.parent = preyHolder;

            lastSpawnTime = Time.time;
        }
    }


    // Is any bird currently fading out? Used by DespawnOld so we only free one slot at a time.
    private bool AnyPreyDespawning()
    {
        for ( int i = 0; i < preyHolder.childCount; i++ ) {
            var p = preyHolder.GetChild( i ).GetComponent<PreyController>();

            if ( p != null && p.IsDespawning ) {
                return true;
            }
        }

        return false;
    }

    // Oldest bird that isn't already despawning (children are appended, so index 0 is oldest).
    private PreyController OldestLivePrey()
    {
        for ( int i = 0; i < preyHolder.childCount; i++ ) {
            var p = preyHolder.GetChild( i ).GetComponent<PreyController>();

            if ( p != null && !p.IsDespawning ) {
                return p;
            }
        }

        return null;
    }

    public Vector3 SpawnInsideBox()
    {
        Vector3 min , max;

        if ( regionType == RegionType.Box && boxRegion != null ) {
            var half = boxRegion.lossyScale * 0.5f;
            min = boxRegion.position - half;
            max = boxRegion.position + half;
        } else if ( regionType == RegionType.Collider && regionCollider != null ) {
            min = regionCollider.bounds.min;
            max = regionCollider.bounds.max;
        } else {
            min = max = transform.position; // no region assigned → spawn at the manager origin
        }

        var randomPos = new Vector3(
            Random.Range( min.x , max.x ) ,
            Random.Range( min.y , max.y ) ,
            Random.Range( min.z , max.z ) );

        float closeness = spawnClosenessToBird;

        if ( closeness > 0f ) {
            var wren = GetWrenPosition();

            if ( wren.HasValue ) {
                var clamped = new Vector3(
                    Mathf.Clamp( wren.Value.x , min.x , max.x ) ,
                    Mathf.Clamp( wren.Value.y , min.y , max.y ) ,
                    Mathf.Clamp( wren.Value.z , min.z , max.z ) );
                return Vector3.Lerp( randomPos , clamped , closeness );
            }
        }

        return randomPos;
    }

    public Vector3 SpawnNextToCurve()
    {
        var curve = regionSpline;

        if ( curve == null || curve.Splines.Count == 0 ) {
            Debug.LogWarning( "[PreyManager] SpawnNextToCurve: no spline assigned — set regionSpline" );
            return SpawnInsideBox();
        }

        var sp = curve.Spline;
        var xform = curve.transform;

        var wren = GetWrenPosition();
        Vector3 basePos;

        if ( wren.HasValue ) {
            // always find the nearest point on the spline to the bird, then scatter
            var localWren = (float3)xform.InverseTransformPoint( wren.Value );
            SplineUtility.GetNearestPoint( sp , localWren , out var nearestLocal , out float _ );
            var nearestOnCurve = xform.TransformPoint( (Vector3)nearestLocal );

            if ( spawnClosenessToBird < 1f ) {
                float t = Random.value;
                var randomPoint = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( sp , t ) );
                basePos = Vector3.Lerp( randomPoint , nearestOnCurve , spawnClosenessToBird );
            } else {
                basePos = nearestOnCurve;
            }
        } else {
            float t = Random.value;
            basePos = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( sp , t ) );
        }

        return basePos + Random.insideUnitSphere * spawnRadius;
    }

    // XZ from the region, Y set to ground + the altitude module's desired-altitude range.
    public Vector3 SpawnAtDesiredAltitude()
    {
        var basePos = regionType == RegionType.Spline ? SpawnNextToCurve() : SpawnInsideBox();

        var alt = preyConfig.altitude;
        basePos.y = GroundYAt( basePos ) + Random.Range( alt.desiredAltitudeMin , alt.desiredAltitudeMax );
        return basePos;
    }

    // On a ring around the bird (distanceMin..distanceMax), Y set to the desired-altitude range.
    public Vector3 SpawnInDistance()
    {
        var wren = GetWrenPosition() ?? transform.position;
        float angle = Random.value * Mathf.PI * 2f;
        float dist = Random.Range( spawnDistanceMin , spawnDistanceMax );

        var pos = wren + new Vector3( Mathf.Cos( angle ) , 0f , Mathf.Sin( angle ) ) * dist;

        var alt = preyConfig.altitude;
        pos.y = GroundYAt( pos ) + Random.Range( alt.desiredAltitudeMin , alt.desiredAltitudeMax );
        return pos;
    }

    // World ground height under an XZ position; falls back to the position's own Y if nothing is hit.
    private float GroundYAt( Vector3 pos )
    {
        PreyProfiler.raycastCount++;

        if ( Physics.Raycast( new Vector3( pos.x , 10000f , pos.z ) , Vector3.down , out var hit , 20000f ) ) {
            return hit.point.y;
        }

        return pos.y;
    }

    private Vector3? GetWrenPosition()
    {
        if ( God.wren != null ) {
            return God.wren.transform.position;
        }

        if ( debugWren != null ) {
            return debugWren.position;
        }

        return null;
    }

    // ── Painted spawning ──────────────────────────────────────────────────────
    // Spawn ahead of the bird (in its heading) at the configured distance, but
    // only where the chosen food-map channel is above the threshold. Returns
    // false if no painted spot was found in a few tries (caller skips spawning).
    private bool TryGetPaintedSpawn( out Vector3 pos )
    {
        pos = transform.position;

        var wren = GetWrenPosition();
        if ( !wren.HasValue || !God.hasIslandData ) return false;

        Vector3 wp    = wren.Value;
        Vector3 fwd   = WrenHeadingHorizontal();
        Vector3 right = Vector3.Cross( Vector3.up , fwd );
        var     alt   = preyConfig.altitude;

        const int attempts = 16;
        for ( int i = 0; i < attempts; i++ ) {
            float   dist = Random.Range( spawnDistanceMin , spawnDistanceMax );
            float   side = Random.Range( -spawnRadius , spawnRadius );
            Vector3 cand = wp + fwd * dist + right * side;

            if ( IsPainted( cand ) ) {
                cand.y = GroundYAt( cand ) + Random.Range( alt.desiredAltitudeMin , alt.desiredAltitudeMax );
                pos = cand;
                return true;
            }
        }
        return false;
    }

    // The bird's horizontal heading (forward), used to spawn "ahead of" it.
    private Vector3 WrenHeadingHorizontal()
    {
        Transform t = God.wren != null ? God.wren.transform : debugWren;
        Vector3   f = t != null ? t.forward : Vector3.forward;
        f.y = 0f;
        return f.sqrMagnitude > 1e-4f ? f.normalized : Vector3.forward;
    }

    // The island we sample the food map from. Prefer the explicitly-set island data, but fall
    // back to the controller's current island (what the game itself samples) so painted spawn/
    // despawn works even when God.hasIslandData wasn't set.
    private static IslandData PaintedIsland()
    {
        if ( God.hasIslandData && God.islandData != null ) return God.islandData;
        var ic = God.islandController;
        return ic != null ? ic.currentIsland : null;
    }

    // All 4 food-map channels (RGBA) at a world position, via the game's own UV mapping.
    private Vector4 SampleFood( Vector3 worldPos )
    {
        var data = PaintedIsland();
        if ( data == null || data.foodMap == null ) return Vector4.zero;
        return data.GetFood( God.UVInMap( worldPos ) );
    }

    // True if ANY of the manager's painted channels is above the threshold at this world position.
    private bool IsPainted( Vector3 pos )
    {
        var chans = paintedChannels;
        if ( chans == null || chans.Length == 0 ) return false;
        Vector4 food = SampleFood( pos );
        for ( int i = 0; i < chans.Length; i++ ) {
            int ch = Mathf.Clamp( chans[i] , 0 , 3 );
            if ( food[ch] >= paintedThreshold ) return true;
        }
        return false;
    }


    // ── Painted "islands": connected components (chunks) of the painted area ───
    private int[]   _islandLabels;   // res*res, chunk id per cell (-1 = unpainted)
    private int     _islandRes;
    private int     _islandCount;
    private Vector3 _islandSize, _islandOffset;
    private int     _islandHash = int.MinValue;

    // Which painted chunk a world position is in. -1 = unpainted / no data.
    public int PaintIslandAt( Vector3 worldPos )
    {
        EnsureIslandsBuilt();
        if ( _islandLabels == null || _islandSize.x <= 0f || _islandSize.z <= 0f ) return -1;

        float u = ( worldPos.x - _islandOffset.x ) / _islandSize.x;
        float v = ( worldPos.z - _islandOffset.z ) / _islandSize.z;
        if ( u < 0f || u > 1f || v < 0f || v > 1f ) return -1;

        int i = Mathf.Clamp( (int)( u * _islandRes ) , 0 , _islandRes - 1 );
        int j = Mathf.Clamp( (int)( v * _islandRes ) , 0 , _islandRes - 1 );
        return _islandLabels[ j * _islandRes + i ];
    }

    public int PaintIslandCount { get { EnsureIslandsBuilt(); return _islandCount; } }

    private void EnsureIslandsBuilt()
    {
        int hash = PaintedDebugHash();
        if ( _islandLabels != null && hash == _islandHash ) return;
        BuildPaintIslands();
        _islandHash = hash;
    }

    // Flood-fill the painted grid into connected components. Cached; rebuilt when the
    // channels/threshold/resolution change (or via the context menu after painting).
    [ContextMenu( "Rebake Paint Islands" )]
    public void BuildPaintIslands()
    {
        _islandLabels = null;
        _islandCount  = 0;

        var     data  = ResolvePaintedIsland();
        Terrain ter   = ResolvePaintedTerrain( data );
        var     chans = paintedChannels;
        if ( data == null || data.foodMap == null || ter == null || ter.terrainData == null
             || chans == null || chans.Length == 0 ) return;

        int   res = Mathf.Clamp( paintedDebugResolution , 16 , 256 );
        float thr = paintedThreshold;
        var   tex = data.foodMap;
        _islandSize   = ter.terrainData.size;
        _islandOffset = ter.transform.position;

        // 1) painted / not-painted per cell
        bool[] painted = new bool[ res * res ];
        for ( int j = 0; j < res; j++ )
        for ( int i = 0; i < res; i++ ) {
            Color c = tex.GetPixelBilinear( ( i + 0.5f ) / res , ( j + 0.5f ) / res );
            bool p = false;
            for ( int k = 0; k < chans.Length; k++ )
                if ( Chan( c , Mathf.Clamp( chans[k] , 0 , 3 ) ) >= thr ) { p = true; break; }
            painted[ j * res + i ] = p;
        }

        // 2) label connected components (4-connectivity) via iterative flood fill
        int[] label = new int[ res * res ];
        for ( int n = 0; n < label.Length; n++ ) label[n] = -1;
        var stack = new System.Collections.Generic.Stack<int>();
        int next = 0;
        for ( int start = 0; start < label.Length; start++ ) {
            if ( !painted[start] || label[start] != -1 ) continue;
            stack.Push( start );
            label[start] = next;
            while ( stack.Count > 0 ) {
                int idx = stack.Pop();
                int x = idx % res, y = idx / res;
                if ( x > 0 )       FloodInto( idx - 1   , painted , label , next , stack );
                if ( x < res - 1 ) FloodInto( idx + 1   , painted , label , next , stack );
                if ( y > 0 )       FloodInto( idx - res , painted , label , next , stack );
                if ( y < res - 1 ) FloodInto( idx + res , painted , label , next , stack );
            }
            next++;
        }

        _islandLabels = label;
        _islandRes    = res;
        _islandCount  = next;
    }

    private static void FloodInto( int idx , bool[] painted , int[] label , int id , System.Collections.Generic.Stack<int> stack )
    {
        if ( painted[idx] && label[idx] == -1 ) { label[idx] = id; stack.Push( idx ); }
    }

    // Distinct color per chunk id (golden-ratio hue spacing); red for unpainted.
    public static Color IslandColor( int id )
    {
        if ( id < 0 ) return Color.red;
        float h = ( id * 0.618034f ) % 1f;
        return Color.HSVToRGB( h , 0.65f , 1f );
    }

    // ── On Point Of Interest spawning ─────────────────────────────────────────
    private void SpawnOnPointOfInterest()
    {
        // the whole cluster spawns at ONE shared POI
        if ( !TryPickSpawnPOI( out var poi ) ) return;   // none toggled / in-region → skip this tick

        for ( int i = 0; i < preyPerCluster; i++ ) {
            Vector3 pos  = POISpawnPosition( poi );
            var     bird = Instantiate( preyPrefab , pos , Quaternion.identity ).GetComponent<PreyController>();
            bird.Initialize( preyConfig , this );
            bird.transform.parent = preyHolder;

            ApplyPOISpawnState( bird , poi );
            lastSpawnTime = Time.time;
        }
    }

    // Pick a toggled-on interest point that is inside the region. If an ideal-distance
    // weight is set, bias the pick toward points whose distance from the wren is near the
    // ideal distance; otherwise pick uniformly at random.
    private bool TryPickSpawnPOI( out PreyInterestPoint poi )
    {
        poi = null;
        if ( spawnAtPoints == null || spawnAtPoints.Count == 0 ) return false;

        var wren = GetWrenPosition();
        // For a painted region, only consider POIs in the SAME painted chunk as the wren.
        int wrenChunk = ( regionType == RegionType.Painted && wren.HasValue ) ? PaintIslandAt( wren.Value ) : -1;

        _poiPick.Clear();
        for ( int i = 0; i < spawnAtPoints.Count; i++ ) {
            var p = spawnAtPoints[i];
            if ( p == null ) continue;

            if ( regionType == RegionType.Painted ) {
                if ( wrenChunk < 0 || PaintIslandAt( p.transform.position ) != wrenChunk ) continue;
            } else if ( !IsInsideRegion( p.transform.position ) ) {
                continue;
            }
            _poiPick.Add( p );
        }
        if ( _poiPick.Count == 0 ) return false;

        float weight = Mathf.Clamp01( poiIdealDistanceWeight );
        if ( weight <= 0f || !wren.HasValue ) {
            poi = _poiPick[ Random.Range( 0 , _poiPick.Count ) ];   // 0 → pure random
            return true;
        }

        // Prefer POIs near a point `idealDistance` in front of the wren. `spread` is the tolerance
        // radius; `weight` (0..1) sharpens the preference (1 → strongly favor the nearest-to-ideal).
        Vector3 ideal  = wren.Value + WrenHeadingHorizontal() * poiIdealDistance;
        float   spread = Mathf.Max( 0.01f , poiIdealSpread );
        float   sharp  = weight * 8f;

        _poiWeights.Clear();
        float total = 0f;
        for ( int i = 0; i < _poiPick.Count; i++ ) {
            float e = Vector3.Distance( _poiPick[i].transform.position , ideal ) / spread;
            float score = 1f / ( 1f + e * e );             // 1 at the ideal point, falls off past `spread`
            float w     = Mathf.Pow( score , sharp );
            _poiWeights.Add( w );
            total += w;
        }

        float r = Random.value * total;
        for ( int i = 0; i < _poiPick.Count; i++ ) {
            r -= _poiWeights[i];
            if ( r <= 0f ) { poi = _poiPick[i]; return true; }
        }
        poi = _poiPick[ _poiPick.Count - 1 ];
        return true;
    }

    // Initial spawn position by POI type. Perch is snapped to its surface afterwards by the bird,
    // so its exact value here doesn't matter; Updraft/others scatter within spawnRadius (randomness).
    private Vector3 POISpawnPosition( PreyInterestPoint poi )
    {
        Vector3 c = poi.transform.position;
        switch ( poi.type ) {
            case InterestPointType.Perch:
                return c;
            default:
                return c + Random.insideUnitSphere * spawnRadius;   // sphere around the point
        }
    }

    // Put the freshly-spawned bird into the right state for the POI it spawned at.
    private void ApplyPOISpawnState( PreyController bird , PreyInterestPoint poi )
    {
        switch ( poi.type ) {
            case InterestPointType.Perch:
                bird.SpawnPerchedAt( poi );    // false → stays calm where it spawned (acceptable fallback)
                break;
            case InterestPointType.Updraft:
                bird.SpawnUpdraftingAt( poi );
                break;
            default:
                break;                          // others: stay Calm, already scattered in a sphere
        }
    }

    // ── Painted debug: contour the painted regions of all 4 channels, once ────
    private struct PaintSeg { public Vector3 a; public Vector3 b; public Color c; }
    private System.Collections.Generic.List<PaintSeg> _paintSegs;

    private static readonly Color[] _foodChannelColors =
    {
        new Color( 1f , 0.35f , 0.35f ), // R
        new Color( 0.35f , 1f , 0.40f ), // G
        new Color( 0.40f , 0.60f , 1f ), // B
        new Color( 1f , 0.90f , 0.35f ), // A
    };

    // Marching-squares contour at paintedThreshold for each channel, projected onto
    // the terrain. Computed once and cached (call again to refresh after edits).
    [ContextMenu( "Rebake Painted Debug" )]
    public void BakePaintedDebug()
    {
        // resolve the food map + terrain straight from the scene so this also works in edit mode
        var     data = ResolvePaintedIsland();
        Terrain ter  = ResolvePaintedTerrain( data );
        if ( data == null || data.foodMap == null || ter == null || ter.terrainData == null ) {
            _paintSegs = null;   // data not ready → retry next time rather than caching an empty result
            return;
        }

        _paintTerrain = ter;
        _paintSegs    = new System.Collections.Generic.List<PaintSeg>();

        int     res  = Mathf.Clamp( paintedDebugResolution , 16 , 256 );
        Vector3 size = ter.terrainData.size;
        Vector3 off  = ter.transform.position;
        var     tex  = data.foodMap;
        float   thr  = paintedThreshold;

        // sample the food map on a (res+1)² grid
        var grid = new Color[ ( res + 1 ) * ( res + 1 ) ];
        for ( int j = 0; j <= res; j++ )
        for ( int i = 0; i <= res; i++ )
            grid[ j * ( res + 1 ) + i ] = tex.GetPixelBilinear( (float)i / res , (float)j / res );

        var chans = paintedChannels;
        if ( chans == null || chans.Length == 0 ) return;   // nothing selected → nothing to draw

        for ( int k = 0; k < chans.Length; k++ ) {
            int   ch  = Mathf.Clamp( chans[k] , 0 , 3 );
            Color col = _foodChannelColors[ch];

            for ( int j = 0; j < res; j++ )
            for ( int i = 0; i < res; i++ ) {
                float c00 = Chan( grid[ j * ( res + 1 ) + i ]         , ch );  // BL
                float c10 = Chan( grid[ j * ( res + 1 ) + i + 1 ]     , ch );  // BR
                float c11 = Chan( grid[ ( j + 1 ) * ( res + 1 ) + i + 1 ] , ch ); // TR
                float c01 = Chan( grid[ ( j + 1 ) * ( res + 1 ) + i ] , ch );  // TL

                int id = ( c00 >= thr ? 1 : 0 ) | ( c10 >= thr ? 2 : 0 )
                       | ( c11 >= thr ? 4 : 0 ) | ( c01 >= thr ? 8 : 0 );
                if ( id == 0 || id == 15 ) continue;

                float u0 = (float)i / res, u1 = (float)( i + 1 ) / res;
                float v0 = (float)j / res, v1 = (float)( j + 1 ) / res;

                Vector2 eB = new Vector2( Mathf.Lerp( u0 , u1 , Frac( c00 , c10 , thr ) ) , v0 );
                Vector2 eR = new Vector2( u1 , Mathf.Lerp( v0 , v1 , Frac( c10 , c11 , thr ) ) );
                Vector2 eT = new Vector2( Mathf.Lerp( u0 , u1 , Frac( c01 , c11 , thr ) ) , v1 );
                Vector2 eL = new Vector2( u0 , Mathf.Lerp( v0 , v1 , Frac( c00 , c01 , thr ) ) );

                switch ( id ) {
                    case 1:  AddSeg( eL , eB , size , off , col ); break;
                    case 2:  AddSeg( eB , eR , size , off , col ); break;
                    case 3:  AddSeg( eL , eR , size , off , col ); break;
                    case 4:  AddSeg( eR , eT , size , off , col ); break;
                    case 5:  AddSeg( eL , eB , size , off , col ); AddSeg( eR , eT , size , off , col ); break;
                    case 6:  AddSeg( eB , eT , size , off , col ); break;
                    case 7:  AddSeg( eL , eT , size , off , col ); break;
                    case 8:  AddSeg( eT , eL , size , off , col ); break;
                    case 9:  AddSeg( eT , eB , size , off , col ); break;
                    case 10: AddSeg( eB , eR , size , off , col ); AddSeg( eT , eL , size , off , col ); break;
                    case 11: AddSeg( eT , eR , size , off , col ); break;
                    case 12: AddSeg( eR , eL , size , off , col ); break;
                    case 13: AddSeg( eB , eR , size , off , col ); break;
                    case 14: AddSeg( eL , eB , size , off , col ); break;
                }
            }
        }
    }

    private static float Chan( Color c , int ch ) => ch == 0 ? c.r : ch == 1 ? c.g : ch == 2 ? c.b : c.a;

    // fraction along corner a→b where the value crosses thr
    private static float Frac( float a , float b , float thr )
    {
        float d = b - a;
        return Mathf.Abs( d ) < 1e-6f ? 0.5f : Mathf.Clamp01( ( thr - a ) / d );
    }

    private void AddSeg( Vector2 a , Vector2 b , Vector3 size , Vector3 off , Color col )
        => _paintSegs.Add( new PaintSeg {
            a = UvToTerrain( a , size , off ),
            b = UvToTerrain( b , size , off ),
            c = col } );

    private Vector3 UvToTerrain( Vector2 uv , Vector3 size , Vector3 off )
    {
        float wx = off.x + uv.x * size.x;
        float wz = off.z + uv.y * size.z;
        return new Vector3( wx , TerrainY( wx , wz ) + 0.5f , wz );
    }

    private float TerrainY( float x , float z )
    {
        var ter = _paintTerrain;
        if ( ter != null ) return ter.transform.position.y + ter.SampleHeight( new Vector3( x , 0 , z ) );
        if ( Physics.Raycast( new Vector3( x , 10000f , z ) , Vector3.down , out var hit , 20000f ) ) return hit.point.y;
        return 0f;
    }

    private void OnDrawGizmos()
    {
        DrawPaintedGizmos();
        DrawSpawnPOIGizmos();
#if UNITY_EDITOR
        DrawWrenRegionDebug();
        DrawPOIRegionDebug();
#endif
    }

    private void DrawPaintedGizmos()
    {
        if ( !showPaintedDebug ) return;
        // show whenever this manager uses paint at all — painted spawn OR painted region (edit mode too)
        if ( spawnType != SpawnType.Painted && regionType != RegionType.Painted ) return;

        // Rebake when the cache is empty OR the channels/threshold/resolution changed. Hashing the
        // live values (not OnValidate) is what makes edits on the config SO refresh the overlay —
        // the SO is a separate asset, so editing it never fires this component's OnValidate.
        int hash = PaintedDebugHash();
        if ( _paintSegs == null || hash != _bakeHash ) {
            BakePaintedDebug();
            _bakeHash = hash;
        }
        if ( _paintSegs == null ) return;

        for ( int i = 0; i < _paintSegs.Count; i++ ) {
            Gizmos.color = _paintSegs[i].c;
            Gizmos.DrawLine( _paintSegs[i].a , _paintSegs[i].b );
        }
    }

    private int _bakeHash;
    private int PaintedDebugHash()
    {
        unchecked {
            int h = 17;
            h = h * 31 + paintedThreshold.GetHashCode();
            h = h * 31 + paintedDebugResolution;
            var chans = paintedChannels;
            if ( chans != null )
                for ( int i = 0; i < chans.Length; i++ ) h = h * 31 + chans[i];
            return h;
        }
    }

    // Ring-mark each interest point toggled on for OnPointOfInterest spawning, colored by type.
    private void DrawSpawnPOIGizmos()
    {
        if ( !showSpawnPointDebug || spawnType != SpawnType.OnPointOfInterest || spawnAtPoints == null ) return;

        for ( int i = 0; i < spawnAtPoints.Count; i++ ) {
            var poi = spawnAtPoints[i];
            if ( poi == null ) continue;

            Vector3 c   = poi.transform.position;
            float   r   = poi.type == InterestPointType.Perch ? 1.5f : Mathf.Max( 1.5f , spawnRadius );

            Gizmos.color = POIGizmoColor( poi.type );
            GizmoRing( c , r , 28 );
            Gizmos.DrawLine( c , c + Vector3.up * 2f );
            Gizmos.DrawWireSphere( c , 0.4f );
        }
    }

    private static Color POIGizmoColor( InterestPointType t )
    {
        switch ( t ) {
            case InterestPointType.Perch:   return new Color( 1f , 0.85f , 0.2f );  // yellow
            case InterestPointType.Updraft: return new Color( 0.3f , 1f , 0.5f );   // green
            default:                        return new Color( 0.4f , 0.8f , 1f );   // cyan
        }
    }

    // Horizontal wire ring drawn with plain Gizmos (no editor-only Handles needed).
    private static void GizmoRing( Vector3 c , float r , int seg )
    {
        Vector3 prev = c + new Vector3( r , 0 , 0 );
        for ( int i = 1; i <= seg; i++ ) {
            float a = i / (float)seg * Mathf.PI * 2f;
            Vector3 p = c + new Vector3( Mathf.Cos( a ) * r , 0 , Mathf.Sin( a ) * r );
            Gizmos.DrawLine( prev , p );
            prev = p;
        }
    }

#if UNITY_EDITOR
    // Live "PAINTED: IN / OUT" readout above the wren so you can verify the trigger by flying over
    // painted vs. unpainted ground. Also dumps the raw sampled values to show WHY it triggers.
    private void DrawWrenRegionDebug()
    {
        if ( !showPaintedDebug ) return;
        bool usesPaint = spawnType == SpawnType.Painted || regionType == RegionType.Painted;
        if ( !usesPaint ) return;

        var wren = GetWrenPosition();
        if ( !wren.HasValue ) return;
        Vector3 wp = wren.Value;

        var     data    = PaintedIsland();
        bool    noData  = data == null || data.foodMap == null;
        Vector4 food    = SampleFood( wp );
        bool    painted = !noData && IsPainted( wp );

        string status = noData ? "NO ISLAND DATA" : ( painted ? "PAINTED: IN" : "PAINTED: OUT" );
        Color  col    = noData ? Color.yellow     : ( painted ? Color.green   : Color.red );

        var chans  = paintedChannels;
        string chStr = "";
        if ( chans != null ) for ( int i = 0; i < chans.Length; i++ ) chStr += chans[i] + " ";

        string label =
            $"{name}: {status}\n" +
            $"region: {regionType}   inRegion: {birdInsideRegion}\n" +
            $"despawn: {despawnType} ({despawnSubject})   onExit: {despawnOnWrenExit}   wrenOutsideRegion: {wrenOutsideRegion}\n" +
            $"minAlive: {minimumTimeAlive}s   graceOutside: {timeOutsideBeforeDespawn}s\n" +
            ( regionType == RegionType.Painted ? $"wren chunk: {PaintIslandAt( wp )} / {PaintIslandCount} chunks\n" : "" ) +
            $"food RGBA: {food.x:0.00} {food.y:0.00} {food.z:0.00} {food.w:0.00}\n" +
            $"channels [{chStr.Trim()}]  thr {paintedThreshold:0.00}\n" +
            $"island: {( noData ? "<none>" : data.name )}";

        var style = new GUIStyle( UnityEditor.EditorStyles.boldLabel );
        style.normal.textColor = col;

        Gizmos.color = col;
        Gizmos.DrawWireSphere( wp + Vector3.up * 3f , 0.3f );
        UnityEditor.Handles.Label( wp + Vector3.up * 3.3f , label , style );
    }

    // Label each toggled spawn POI with whether it's IN / OUT of THIS manager's region, so you can
    // see which points this manager will actually spawn at (and why a "foreign" point qualifies).
    private void DrawPOIRegionDebug()
    {
        if ( !showPOIRegionDebug || spawnType != SpawnType.OnPointOfInterest || spawnAtPoints == null ) return;

        var wren = GetWrenPosition();
        int wrenChunk = ( regionType == RegionType.Painted && wren.HasValue ) ? PaintIslandAt( wren.Value ) : -1;

        for ( int i = 0; i < spawnAtPoints.Count; i++ ) {
            var poi = spawnAtPoints[i];
            if ( poi == null ) continue;
            Vector3 c = poi.transform.position;

            Color  col;
            string info;
            if ( regionType == RegionType.Painted ) {
                int  chunk   = PaintIslandAt( c );
                bool isWrens = chunk >= 0 && chunk == wrenChunk;
                col  = chunk < 0 ? Color.red : ( isWrens ? IslandColor( chunk ) : new Color( 1f , 0.6f , 0f ) );
                info = chunk < 0 ? "unpainted (OUT)"
                     : isWrens   ? $"chunk {chunk}  ← wren's (spawns here)"
                     :             $"chunk {chunk}  (other chunk — skipped)";
            } else {
                bool inside = IsInsideRegion( c );
                col  = inside ? Color.green : Color.red;
                info = inside ? $"IN ({regionType})" : $"OUT ({regionType})";
            }

            string label = $"{poi.name}\n{info}";
            var style = new GUIStyle( UnityEditor.EditorStyles.boldLabel );
            style.normal.textColor = col;

            Gizmos.color = col;
            Gizmos.DrawLine( c , c + Vector3.up * 1.5f );
            Gizmos.DrawWireSphere( c , 0.5f );
            UnityEditor.Handles.Label( c + Vector3.up * 1.7f , label , style );
        }
    }
#endif

    private Terrain _paintTerrain;

    // Island to sample for the painted debug — works in edit mode (no runtime God state needed).
    // Live current island if one is active, otherwise the controller's default island.
    private IslandData ResolvePaintedIsland()
    {
        var controllers = FindObjectsByType<IslandController>( FindObjectsSortMode.None );
        if ( controllers == null || controllers.Length == 0 ) return null;
        var ic = controllers[0];
        if ( ic.currentIsland != null ) return ic.currentIsland;
        if ( ic.islands != null && ic.islands.Length > 0 )
            return ic.islands[ Mathf.Clamp( ic.defaultIslandID , 0 , ic.islands.Length - 1 ) ];
        return null;
    }

    private Terrain ResolvePaintedTerrain( IslandData island )
    {
        if ( island != null && island.terrain != null ) return island.terrain;
        return Terrain.activeTerrain;
    }


    public Transform GetClosestThermalCenter( Vector3 pos )
    {
        return null;
        // thermal removed from interest point system
    }

    public Transform GetClosestAnchorPoints( Vector3 pos )
    {
        return null;
        // anchor removed from interest point system
    }


    // Build the per-frame neighbor grid, but only when this manager's prey
    // actually run neighbor queries (flock/social). Cell size = the largest
    // query radius so any query touches at most a 3×3×3 cell neighborhood.
    private void BuildNeighborGrid()
    {
        if ( preyConfig == null || preyHolder == null ) { _gridFrame = -1; return; }

        bool needs = preyConfig.modules.flock || preyConfig.modules.social;
        if ( !needs ) { _gridFrame = -1; return; }

        float cell = 0f;
        if ( preyConfig.modules.flock )  cell = Mathf.Max( cell , preyConfig.flock.detectionRadius );
        if ( preyConfig.modules.social ) cell = Mathf.Max( cell , preyConfig.social.neighborRadius );
        if ( cell <= 0.01f ) { _gridFrame = -1; return; }

        if ( _neighborGrid == null ) _neighborGrid = new PreySpatialGrid();
        _neighborGrid.Rebuild( preyHolder , cell );
        _gridFrame = Time.frameCount;
    }

    // Nearest point + tangent on the region spline, via the baked lookup table.
    // Lazily bakes on first use. Returns false if there's no spline.
    public bool TryGetNearestOnRegionSpline( Vector3 worldPos , out Vector3 nearestWorld , out Vector3 tangentWorld )
    {
        if ( regionSpline == null || regionSpline.Splines.Count == 0 ) {
            nearestWorld = worldPos; tangentWorld = Vector3.forward; return false;
        }
        if ( _splineCache == null ) _splineCache = new PreySplineCache();
        if ( !_splineCache.IsBaked ) _splineCache.Bake( regionSpline , splineCacheSamples );
        return _splineCache.Nearest( worldPos , out nearestWorld , out tangentWorld );
    }

    // Force a re-bake (e.g. after editing the spline in play mode).
    [ContextMenu( "Rebake Spline Cache" )]
    public void RebakeSplineCache()
    {
        if ( _splineCache == null ) _splineCache = new PreySplineCache();
        _splineCache.Bake( regionSpline , splineCacheSamples );
    }

    public void GetNearbyBirds( Vector3 pos , float radius , PreyController exclude , List<PreyController> results )
    {
        // fast path: this frame's spatial hash
        if ( _neighborGrid != null && _gridFrame == Time.frameCount ) {
            _neighborGrid.Query( pos , radius , exclude , results );
            return;
        }

        // fallback: linear scan (grid not built this frame)
        float sqrRadius = radius * radius;

        for ( int i = 0; i < preyHolder.childCount; i++ ) {
            var bird = preyHolder.GetChild( i ).GetComponent<PreyController>();

            if ( bird == null || bird == exclude ) {
                continue;
            }

            if ( (bird.position - pos).sqrMagnitude < sqrRadius ) {
                results.Add( bird );
            }
        }
    }

    public virtual void PreyGotAte( PreyController b )
    {
        if ( God.particleSystems != null ) {
            var ps = God.particleSystems.Get( gotAteParticle );

            if ( ps != null ) {
                ps.transform.position = b.transform.position;
                ps.Play();
            }
        }

        if ( God.audio != null ) {
            God.audio.Play( God.sounds.eatClip );
        }

        if ( God.wren != null ) {
            God.wren.stats.FullnessAdd( preyFullnessIncrease );
            God.wren.shards.CollectShards( b.parameters.crystals.crystalsOnCollect , b.parameters.crystals.crystalType ,
                b.transform.position );
        }
    }


    private void CacheSplineBounds()
    {
        if ( regionSpline == null ) {
            return;
        }

        var s = regionSpline.Spline;
        var xform = regionSpline.transform;
        int samples = Mathf.Max( 32 , s.Count * 4 );

        var center = Vector3.zero;

        for ( int i = 0; i < samples; i++ ) {
            float t = (float)i / (samples - 1);
            center += xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t ) );
        }

        center /= samples;

        float maxSqr = 0f;

        for ( int i = 0; i < samples; i++ ) {
            float t = (float)i / (samples - 1);
            var p = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t ) );
            maxSqr = Mathf.Max( maxSqr , (p - center).sqrMagnitude );
        }

        splineBoundsCenter = center;
        splineBoundingRadius = Mathf.Sqrt( maxSqr );
    }

    private IEnumerator SplineCheckRoutine()
    {
        var wait = new WaitForSeconds( splineCheckInterval );

        while (true) {
            CheckSplineRegion();
            yield return wait;
        }
    }

    private void CheckSplineRegion()
    {
        if ( regionSpline == null ) {
            return;
        }

        var wrenT = God.wren != null ? God.wren.transform
            : debugWren != null ? debugWren
            : null;

        if ( wrenT == null ) {
            return;
        }

        var wrenPos = wrenT.position;
        float threshold = birdInsideRegion ? splineExitDistance : splineEnterDistance;

        float outerLimit = splineBoundingRadius + threshold;

        if ( (wrenPos - splineBoundsCenter).sqrMagnitude > outerLimit * outerLimit ) {
            if ( birdInsideRegion ) {
                OnWrenExit();
            }

            return;
        }

        var localWren = (float3)regionSpline.transform.InverseTransformPoint( wrenPos );
        SplineUtility.GetNearestPoint( regionSpline.Spline , localWren , out var nearestLocal , out float _ );
        float dist = Vector3.Distance( wrenPos , regionSpline.transform.TransformPoint( (Vector3)nearestLocal ) );

        if ( !birdInsideRegion && dist <= splineEnterDistance ) {
            OnWrenEnter();
        } else if ( birdInsideRegion && dist > splineExitDistance ) {
            OnWrenExit();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if ( !showRegionEntrance ) {
            return;
        }

        if ( regionType == RegionType.Box && boxRegion != null ) {
            Gizmos.color = new Color( 0.2f , 1f , 0.3f , 0.35f );
            Gizmos.DrawWireCube( boxRegion.position , boxRegion.lossyScale );
            return;
        }

        if ( regionType != RegionType.Spline || regionSpline == null ) {
            return;
        }

        var s = regionSpline.Spline;
        var xform = regionSpline.transform;
        int samples = Mathf.Max( 64 , s.Count * 8 );

        // pre-compute world positions + per-sample perpendiculars using spline tangents
        var centers = new Vector3[samples + 1];
        var perps = new Vector3[samples + 1];

        for ( int i = 0; i <= samples; i++ ) {
            float t = (float)i / samples;
            centers[i] = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t ) );
            var tan = xform.TransformDirection( (Vector3)SplineUtility.EvaluateTangent( s , t ) );
            perps[i] = Vector3.Cross( tan , Vector3.up ).normalized;
        }

        for ( int i = 0; i < samples; i++ ) {
            Gizmos.color = new Color( 0.2f , 1f , 0.3f , 0.4f );
            Gizmos.DrawLine( centers[i] + perps[i] * splineEnterDistance ,
                centers[i + 1] + perps[i + 1] * splineEnterDistance );
            Gizmos.DrawLine( centers[i] - perps[i] * splineEnterDistance ,
                centers[i + 1] - perps[i + 1] * splineEnterDistance );

            Gizmos.color = new Color( 1f , 0.5f , 0.1f , 0.25f );
            Gizmos.DrawLine( centers[i] + perps[i] * splineExitDistance ,
                centers[i + 1] + perps[i + 1] * splineExitDistance );
            Gizmos.DrawLine( centers[i] - perps[i] * splineExitDistance ,
                centers[i + 1] - perps[i + 1] * splineExitDistance );
        }
    }
#endif
}