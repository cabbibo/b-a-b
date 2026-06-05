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
    Distance ,   // despawn when far enough from the wren (distance + grace time)
    Collider ,   // despawn when outside a despawn collider
    Cage         // despawn when outside the region/cage (box or region collider)
}

public enum WhenFull
{
    DespawnOld ,       // at max: fade out the oldest bird, then spawn the new one once it's gone
    HoldTilDespawned   // at max: never spawn — wait until existing birds despawn on their own
}


public class PreyManager : MonoBehaviour
{
    /*

        Can spawn in clumps for butterflies etc.

    */

    // ── Debug (lives on the component, not the shared config — per-instance / per-session) ──────
    [Header( "Debug" )]
    public Transform debugWren;
    public bool      stepThrough            = false;
    [Range( 0.01f , 1f )]
    public float     simulationSpeed        = 1f;
    public bool      showRegionEntrance     = true;  // region box / spline enter-exit gizmos
    public bool      showInterestPointDebug = true;  // interest-point markers (via PreyManagerDebug)
    public bool      showDespawnDebug       = true;  // per-prey despawn decision viz (on selected prey)

    // ── Tunable params live on this asset; scene refs + runtime stay on the component ──────────
    [Header( "Manager Config" )]
    public PreyManagerConfigSO managerConfig;

    [Header( "Scene References" )]
    public PreyInterestPoint[] interestPoints; // perch spots, thermals, anchors, updrafts, investigate points

    [Header( "Config" )]
    public PreyConfigSO preyConfig;            // the prey's params
    public GameObject   preyPrefab;            // what to spawn

    [Header( "Scene Wiring" )]
    public Collider        despawnCollider;    // Collider despawn type: outside this → despawn
    public Transform       preyHolder;
    public Transform       boxRegion;          // Box region
    public Collider        regionCollider;     // Collider region
    public SplineContainer regionSpline;       // Spline region

    // ── Runtime state ─────────────────────────────────────────────────────────────────────────
    public bool              birdInsideRegion;
    public List<Transform[]> clusters;
    public float             lastSpawnTime;
    public int               currentNumberOfPrey;
    [HideInInspector] public bool wrenOutsideCage;   // computed once/frame; Cage despawn tests the wren, not each prey

    private Vector3   splineBoundsCenter;
    private float     splineBoundingRadius;
    private Coroutine splineCheckCoroutine;

    // ── Proxy properties: forward to managerConfig, null-safe with the old defaults ────────────
    public float spawnInterval       => managerConfig != null ? managerConfig.spawnInterval       : 3f;
    public int   preyPerCluster      => managerConfig != null ? managerConfig.preyPerCluster      : 1;
    public float clusterRadius       => managerConfig != null ? managerConfig.clusterRadius       : 0f;
    public bool  spawnMaxOnWrenEnter => managerConfig != null ? managerConfig.spawnMaxOnWrenEnter : false;
    public bool  wrenEnterOnEnabled  => managerConfig != null ? managerConfig.wrenEnterOnEnabled  : false;

    public int      maxPray  => managerConfig != null ? managerConfig.maxPray  : 100;
    public WhenFull whenFull => managerConfig != null ? managerConfig.whenFull : WhenFull.DespawnOld;

    public SpawnType spawnType            => managerConfig != null ? managerConfig.spawnType            : SpawnType.InsideBox;
    public float     spawnRadius          => managerConfig != null ? managerConfig.spawnRadius          : 5f;
    public float     spawnDistanceMin     => managerConfig != null ? managerConfig.spawnDistanceMin     : 80f;
    public float     spawnDistanceMax     => managerConfig != null ? managerConfig.spawnDistanceMax     : 150f;
    public float     spawnClosenessToBird => managerConfig != null ? managerConfig.spawnClosenessToBird : 0f;

    public DespawnType despawnType              => managerConfig != null ? managerConfig.despawnType              : DespawnType.Distance;
    public bool        despawnOnWrenExit        => managerConfig != null ? managerConfig.despawnOnWrenExit        : true;
    public float       minimumTimeAlive         => managerConfig != null ? managerConfig.minimumTimeAlive         : 30f;
    public float       timeOutsideBeforeDespawn => managerConfig != null ? managerConfig.timeOutsideBeforeDespawn : 5f;
    public float       distanceBeforeNotCaught  => managerConfig != null ? managerConfig.distanceBeforeNotCaught  : 100f;

    public RegionType regionType          => managerConfig != null ? managerConfig.regionType          : RegionType.Box;
    public float      splineEnterDistance => managerConfig != null ? managerConfig.splineEnterDistance : 20f;
    public float      splineExitDistance  => managerConfig != null ? managerConfig.splineExitDistance  : 30f;
    public float      splineCheckInterval => managerConfig != null ? managerConfig.splineCheckInterval : 0.1f;

    public float preyFullnessIncrease => managerConfig != null ? managerConfig.preyFullnessIncrease : 0f;
    public float preyStaminaIncrease  => managerConfig != null ? managerConfig.preyStaminaIncrease  : 0f;
    public GodParticleType gotAteParticle => managerConfig != null ? managerConfig.gotAteParticle : GodParticleType.Eat;


    public virtual void OnEnable()
    {
        if ( managerConfig == null ) return;   // no params assigned → manager is inert

        lastSpawnTime = Time.time - spawnInterval;
        while (preyHolder.childCount > 0) DestroyImmediate( preyHolder.GetChild( 0 ).gameObject );

        if ( wrenEnterOnEnabled ) {
            OnWrenEnter();
        }

        if ( regionType == RegionType.Spline ) {
            CacheSplineBounds();
            splineCheckCoroutine = StartCoroutine( SplineCheckRoutine() );
        }
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
        if ( managerConfig == null ) return;   // no params assigned → manager is inert

        currentNumberOfPrey = preyHolder.childCount;
        CheckForNewPrey();

        if      ( regionType == RegionType.Box      ) CheckBoxRegion();
        else if ( regionType == RegionType.Collider ) CheckColliderRegion();

        // Cage despawn is wren-based: compute the wren-outside-cage test ONCE here; every prey reads it.
        var wp = GetWrenPosition();
        wrenOutsideCage = wp.HasValue && IsOutsideCage( wp.Value );
    }

    private void CheckBoxRegion()
    {
        if ( boxRegion == null ) return;

        var wrenT = God.wren != null ? God.wren.transform
            : debugWren != null ? debugWren
            : null;
        if ( wrenT == null ) return;

        var   wrenPos = wrenT.position;
        var   half    = boxRegion.lossyScale * 0.5f;
        var   center  = boxRegion.position;
        bool  inside  = wrenPos.x >= center.x - half.x && wrenPos.x <= center.x + half.x
                     && wrenPos.y >= center.y - half.y && wrenPos.y <= center.y + half.y
                     && wrenPos.z >= center.z - half.z && wrenPos.z <= center.z + half.z;

        if ( !birdInsideRegion && inside  ) OnWrenEnter();
        else if ( birdInsideRegion && !inside ) OnWrenExit();
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
    // Is a world position outside the region/cage (box or region collider)? Used by Cage despawn.
    public bool IsOutsideCage( Vector3 pos )
    {
        if ( regionType == RegionType.Box && boxRegion != null ) {
            var half = boxRegion.lossyScale * 0.5f;
            var c    = boxRegion.position;
            return pos.x < c.x - half.x || pos.x > c.x + half.x
                || pos.y < c.y - half.y || pos.y > c.y + half.y
                || pos.z < c.z - half.z || pos.z > c.z + half.z;
        }
        if ( regionType == RegionType.Collider && regionCollider != null )
            return (regionCollider.ClosestPoint( pos ) - pos).sqrMagnitude > 0.0001f;

        return false;
    }

    // Is a world position outside the dedicated despawn collider? Used by Collider despawn.
    public bool IsOutsideDespawnCollider( Vector3 pos )
    {
        if ( despawnCollider == null ) return true;
        return (despawnCollider.ClosestPoint( pos ) - pos).sqrMagnitude > 0.0001f;
    }

    public virtual void CheckForNewPrey()
    {
        if ( managerConfig == null || preyConfig == null || preyPrefab == null ) {
            return;
        }

        if ( Time.time - lastSpawnTime > spawnInterval && birdInsideRegion ) {
            SpawnNewBug();
        }
    }


    public void OnWrenEnter()
    {
        Debug.Log( "OnWrenEnter" );
        birdInsideRegion = true;

        if ( spawnMaxOnWrenEnter ) {
            while ( preyHolder.childCount < maxPray ) {
                SpawnNewBug();
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
                if ( oldest != null ) oldest.ForceDespawn();
            }
            return;
        }


        Vector3 spawnPos;

        switch ( spawnType ) {
            case SpawnType.NextToCurve:     spawnPos = SpawnNextToCurve();       break;
            case SpawnType.BiomePaint:      spawnPos = SpawnBiomePaint();        break;
            case SpawnType.DesiredAltitude: spawnPos = SpawnAtDesiredAltitude(); break;
            case SpawnType.InDistance:      spawnPos = SpawnInDistance();        break;
            case SpawnType.InsideBox:
            default:                        spawnPos = SpawnInsideBox();         break;
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
            if ( p != null && p.IsDespawning ) return true;
        }
        return false;
    }

    // Oldest bird that isn't already despawning (children are appended, so index 0 is oldest).
    private PreyController OldestLivePrey()
    {
        for ( int i = 0; i < preyHolder.childCount; i++ ) {
            var p = preyHolder.GetChild( i ).GetComponent<PreyController>();
            if ( p != null && !p.IsDespawning ) return p;
        }
        return null;
    }

    public Vector3 SpawnInsideBox()
    {
        Vector3 min, max;

        if ( regionType == RegionType.Box && boxRegion != null ) {
            var half = boxRegion.lossyScale * 0.5f;
            min = boxRegion.position - half;
            max = boxRegion.position + half;
        } else if ( regionType == RegionType.Collider && regionCollider != null ) {
            min = regionCollider.bounds.min;
            max = regionCollider.bounds.max;
        } else {
            min = max = transform.position;   // no region assigned → spawn at the manager origin
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

        var   sp    = curve.Spline;
        var   xform = curve.transform;

        var wren = GetWrenPosition();
        Vector3 basePos;

        if ( wren.HasValue ) {
            // always find the nearest point on the spline to the bird, then scatter
            var localWren = (float3)xform.InverseTransformPoint( wren.Value );
            SplineUtility.GetNearestPoint( sp , localWren , out float3 nearestLocal , out float _ );
            var nearestOnCurve = xform.TransformPoint( (Vector3)nearestLocal );

            if ( spawnClosenessToBird < 1f ) {
                float t           = Random.value;
                var   randomPoint = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( sp , t ) );
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
        var   wren  = GetWrenPosition() ?? transform.position;
        float angle = Random.value * Mathf.PI * 2f;
        float dist  = Random.Range( spawnDistanceMin , spawnDistanceMax );

        var pos = wren + new Vector3( Mathf.Cos( angle ) , 0f , Mathf.Sin( angle ) ) * dist;

        var alt = preyConfig.altitude;
        pos.y = GroundYAt( pos ) + Random.Range( alt.desiredAltitudeMin , alt.desiredAltitudeMax );
        return pos;
    }

    // World ground height under an XZ position; falls back to the position's own Y if nothing is hit.
    private float GroundYAt( Vector3 pos )
    {
        if ( Physics.Raycast( new Vector3( pos.x , 10000f , pos.z ) , Vector3.down , out var hit , 20000f ) ) {
            return hit.point.y;
        }

        return pos.y;
    }

    private Vector3? GetWrenPosition()
    {
        if ( God.wren != null )  return God.wren.transform.position;
        if ( debugWren != null ) return debugWren.position;
        return null;
    }

    public virtual Vector3 SpawnBiomePaint()
    {
        return SpawnInsideBox();
    }


    public Transform GetClosestThermalCenter( Vector3 pos ) => null; // thermal removed from interest point system
    public Transform GetClosestAnchorPoints( Vector3 pos )  => null; // anchor removed from interest point system


    public void GetNearbyBirds( Vector3 pos , float radius , PreyController exclude , List<PreyController> results )
    {
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
        if ( !showRegionEntrance ) return;

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
        var perps   = new Vector3[samples + 1];
        for ( int i = 0; i <= samples; i++ ) {
            float t   = (float)i / samples;
            centers[i] = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t ) );
            var   tan  = xform.TransformDirection( (Vector3)SplineUtility.EvaluateTangent( s , t ) );
            perps[i]   = Vector3.Cross( tan , Vector3.up ).normalized;
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