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


public class PreyManager : MonoBehaviour
{
    /*

        Can spawn in clumps for butterflies etc.

    */

    public Transform debugWren;

    [Header( "Debug" )]
    public bool  stepThrough      = false;
    [Range( 0.01f , 1f )]
    public float simulationSpeed  = 1f;

    [Header( "Scene References" )]
    public PreyInterestPoint[] interestPoints; // perch spots, thermals, anchors, updrafts, investigate points

    public bool spawnMaxOnWrenEnter;


    [Header( "Spawn Timing" )]
    public float spawnInterval = 3f;

    public int   bugsPerCluster = 1;
    public float clusterRadius  = 0f;

    [Header( "Config" )]
    public PreyConfigSO preyConfig;

    public GameObject preyPrefab;

    public int maxPray = 100;


    [Header( "On Eat Effects" )]
    public float preyFullnessIncrease;

    public float preyStaminaIncrease;

    public ParticleSystem gotAteParticles;


    public bool birdInsideRegion;

    public Transform preyHolder;

    public Transform[] spawnPoints;

    public bool wrenEnterOnEnabled;

    public List<Transform[]> clusters;

    public float lastSpawnTime;
    public int   currentNumberOfPrey;

    [Header( "Region Detection" )]
    public RegionType      regionType          = RegionType.Box;
    public Transform       boxRegion;
    public Collider        regionCollider;
    public SplineContainer regionSpline;
    public float           splineEnterDistance = 20f;
    public float           splineExitDistance  = 30f;
    public float           splineCheckInterval = 0.1f;

    private Vector3   splineBoundsCenter;
    private float     splineBoundingRadius;
    private Coroutine splineCheckCoroutine;


    public virtual void OnEnable()
    {
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
        currentNumberOfPrey = preyHolder.childCount;
        CheckForNewPrey();

        if      ( regionType == RegionType.Box      ) CheckBoxRegion();
        else if ( regionType == RegionType.Collider ) CheckColliderRegion();
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

    public virtual void CheckForNewPrey()
    {
        if ( preyConfig == null || preyPrefab == null ) {
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

        if ( preyConfig == null || !preyConfig.despawn.onWrenExit ) {
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
        var s = preyConfig.spawn;

        // destroy any over max
        while (preyHolder.childCount >= maxPray) DestroyImmediate( preyHolder.GetChild( 0 ).gameObject );


        var spawnPos = transform.position;

        if ( regionType == RegionType.Spline ) {
            spawnPos = SpawnNextToCurve();
        } else if ( s.spawnType == SpawnType.InsideBox ) {
            spawnPos = SpawnInsideBox();
        } else if ( s.spawnType == SpawnType.NextToCurve ) {
            spawnPos = SpawnNextToCurve();
        } else if ( s.spawnType == SpawnType.BiomePaint ) {
            spawnPos = SpawnBiomePaint();
        }

        if ( s.spawnType != SpawnType.InsideBox ) {
            RaycastHit hit;
            float groundY = spawnPos.y;

            if ( Physics.Raycast( new Vector3( spawnPos.x , 10000f , spawnPos.z ) , Vector3.down , out hit , 20000 ) ) {
                groundY = hit.point.y + s.spawnRadius * 2;
            }

            if ( s.altitudeType == AltitudeType.RandomRange ) {
                spawnPos.y = groundY + preyConfig.altitude.minAltitude +
                             Random.Range( 0 , preyConfig.altitude.maxAltitude - preyConfig.altitude.minAltitude );
            } else if ( s.altitudeType == AltitudeType.DesiredAltitude ) {
                spawnPos.y = groundY + Random.Range( preyConfig.altitude.desiredAltitudeMin , preyConfig.altitude.desiredAltitudeMax );
            } else if ( s.altitudeType == AltitudeType.OnGround ) {
                spawnPos.y = groundY;
            }
        }

        for ( int i = 0; i < bugsPerCluster; i++ ) {
            spawnPos += Random.insideUnitSphere * clusterRadius;

            var newPrey = Instantiate( preyPrefab , spawnPos , Quaternion.identity ).GetComponent<PreyController>();
            newPrey.Initialize( preyConfig , this );
            newPrey.transform.parent = preyHolder;

            lastSpawnTime = Time.time;
        }
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
            min = preyConfig.spawn.boundsMin;
            max = preyConfig.spawn.boundsMax;
        }

        var randomPos = new Vector3(
            Random.Range( min.x , max.x ) ,
            Random.Range( min.y , max.y ) ,
            Random.Range( min.z , max.z ) );

        float closeness = preyConfig.spawn.closenessToBird;

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

        var   s     = preyConfig.spawn;
        var   sp    = curve.Spline;
        var   xform = curve.transform;

        var wren = GetWrenPosition();
        Vector3 basePos;

        if ( wren.HasValue ) {
            // always find the nearest point on the spline to the bird, then scatter
            var localWren = (float3)xform.InverseTransformPoint( wren.Value );
            SplineUtility.GetNearestPoint( sp , localWren , out float3 nearestLocal , out float _ );
            var nearestOnCurve = xform.TransformPoint( (Vector3)nearestLocal );

            if ( s.closenessToBird < 1f ) {
                float t           = Random.value;
                var   randomPoint = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( sp , t ) );
                basePos = Vector3.Lerp( randomPoint , nearestOnCurve , s.closenessToBird );
            } else {
                basePos = nearestOnCurve;
            }
        } else {
            float t = Random.value;
            basePos = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( sp , t ) );
        }

        return basePos + Random.insideUnitSphere * s.spawnRadius;
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
        if ( gotAteParticles != null ) {
            gotAteParticles.transform.position = b.transform.position;
            gotAteParticles.Play();
        }

        if ( God.particleSystems != null && God.particleSystems.eatParticleSystem != null ) {
            var ps = God.particleSystems.eatParticleSystem;
            ps.transform.position = b.transform.position;
            ps.Play();
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

        for ( int i = 0; i < samples; i++ ) {
            float t0 = (float)i / samples;
            float t1 = (float)(i + 1) / samples;
            var p0 = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t0 ) );
            var p1 = xform.TransformPoint( (Vector3)SplineUtility.EvaluatePosition( s , t1 ) );

            var tangent = (p1 - p0).normalized;
            var perp = Vector3.Cross( tangent , Vector3.up ).normalized;

            Gizmos.color = new Color( 0.2f , 1f , 0.3f , 0.4f );
            Gizmos.DrawLine( p0 + perp * splineEnterDistance , p1 + perp * splineEnterDistance );
            Gizmos.DrawLine( p0 - perp * splineEnterDistance , p1 - perp * splineEnterDistance );

            Gizmos.color = new Color( 1f , 0.5f , 0.1f , 0.25f );
            Gizmos.DrawLine( p0 + perp * splineExitDistance , p1 + perp * splineExitDistance );
            Gizmos.DrawLine( p0 - perp * splineExitDistance , p1 - perp * splineExitDistance );
        }
    }
#endif
}