using UnityEngine;
using WrenUtils;
using System.Collections.Generic;

public class PreyManager : MonoBehaviour
{
    /*

        Can spawn in clumps for butterflies etc.

    */

    public Transform debugWren;

    [Header( "Debug" )]
    public bool stepThrough = false;

    [Header( "Scene References" )]
    public Transform[] thermalCenters; // orbit points for thermal module (picks closest)

    public Transform[] anchorPoints; // wander centers for anchor/butterfly module (picks closest)
    public Transform[] perchPoints; // explicit perch targets for this manager's birds

    public PreySpline spawnCurve; // scene curve for NextToCurve spawn type

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


    public bool wrenInside;

    public Transform preyHolder;

    public Transform cage;

    public Transform[] spawnPoints;
    public Transform[] objectsOfInterest;

    public bool wrenEnterOnEnabled;

    public List<Transform[]> clusters;

    public float lastSpawnTime;
    public int   currentNumberOfPrey;


    public void OnEnable()
    {
        lastSpawnTime = Time.time - spawnInterval;
        while (preyHolder.childCount > 0) DestroyImmediate( preyHolder.GetChild( 0 ).gameObject );

        if ( wrenEnterOnEnabled ) {
            OnWrenEnter();
        }
    }

    public void OnTriggerEnter( Collider other )
    {
        if ( God.IsOurWren( other ) ) {
            OnWrenEnter();
        }
    }

    public void OnTriggerExit( Collider other )
    {
        if ( God.IsOurWren( other ) ) {
            OnWrenExit();
        }
    }

    private void Update()
    {
        currentNumberOfPrey = preyHolder.childCount;
        CheckForNewPrey();
    }

    public virtual void CheckForNewPrey()
    {
        if ( preyConfig == null || preyPrefab == null ) {
            return;
        }

        if ( Time.time - lastSpawnTime > spawnInterval && wrenInside ) {
            SpawnNewBug();
        }
    }


    public void OnWrenEnter()
    {
        Debug.Log( "OnWrenEnter" );
        wrenInside = true;
    }

    public void OnWrenExit()
    {
        wrenInside = false;

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

        if ( s.spawnType == SpawnType.InsideBox ) {
            
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
        if ( cage != null ) {
            var col = cage.GetComponent<Collider>();
            if ( col != null ) {
                var bounds = col.bounds;
                return new Vector3(
                    Random.Range( bounds.min.x , bounds.max.x ) ,
                    Random.Range( bounds.min.y , bounds.max.y ) ,
                    Random.Range( bounds.min.z , bounds.max.z ) );
            }
        }

        var b = preyConfig.spawn;
        return new Vector3(
            Random.Range( b.boundsMin.x , b.boundsMax.x ) ,
            Random.Range( b.boundsMin.y , b.boundsMax.y ) ,
            Random.Range( b.boundsMin.z , b.boundsMax.z ) );
    }

    public Vector3 SpawnNextToCurve()
    {
        if ( spawnCurve == null ) {
            return SpawnInsideBox();
        }

        var s = preyConfig.spawn;
        float t = Random.value;
        var onCurve = spawnCurve.GetPointAt( t );
        var fwd = spawnCurve.GetForwardAt( t );
        var right = Vector3.Cross( Vector3.up , fwd ).normalized;
        float side = Random.value > 0.5f ? 1f : -1f;
        return onCurve + right * s.curveOffset * side + Random.insideUnitSphere * s.spawnRadius;
    }

    public virtual Vector3 SpawnBiomePaint()
    {
        return SpawnInsideBox();
    }


    public Transform GetClosestThermalCenter( Vector3 pos )
    {
        Transform best = null;
        float bestSqr = float.MaxValue;

        if ( thermalCenters == null ) {
            return null;
        }

        foreach (var t in thermalCenters) {
            if ( t == null ) {
                continue;
            }

            float d = (t.position - pos).sqrMagnitude;

            if ( d < bestSqr ) {
                bestSqr = d;
                best = t;
            }
        }

        return best;
    }

    public Transform GetClosestAnchorPoints( Vector3 pos )
    {
        Transform best = null;
        float bestSqr = float.MaxValue;

        if ( anchorPoints == null ) {
            return null;
        }

        foreach (var a in anchorPoints) {
            if ( a == null ) {
                continue;
            }

            float d = (a.position - pos).sqrMagnitude;

            if ( d < bestSqr ) {
                bestSqr = d;
                best = a;
            }
        }

        return best;
    }


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

        if ( God.audio != null ) God.audio.Play( God.sounds.eatClip );

        if ( God.wren != null ) {
            God.wren.stats.FullnessAdd( preyFullnessIncrease );
            God.wren.shards.CollectShards( b.parameters.crystals.crystalsOnCollect , b.parameters.crystals.crystalType ,
                b.transform.position );
        }
    }
}