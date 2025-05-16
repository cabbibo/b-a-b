using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

[System.Serializable]
public class ShardEvent : UnityEvent<Shard>
{
}

public class Shard : MonoBehaviour
{
    public int  ShardsToAdd;
    public bool destroyOnCollect = true;

    public bool collected    = false;
    public bool firstCollect = false;


    public GameObject Uncollected;
    public GameObject Collected;

    public Transform collectionPosition;


    public float type;

    public float timeCollected;
    public float respawnTime = 10;
    public float timeHit;

    public bool respawnAfterTime;

    public Helpers.GameObjectEvent onCollectEvent;
    public Helpers.GameObjectEvent onFirstCollectEvent;
    public Helpers.GameObjectEvent respawnEvent;


    // SUCKABLE

    public bool         magnetizable = false;
    public Transform    centerTransform;
    public Transform    magnetizableTransform;
    public bool         beingMangetized = false;
    public Transform    magnetizer;
    public Vector3      velocity;
    public float        magnetizeForce;
    public float        returnForce;
    public float        dampening;
    public float        magnetizableDistance = 10f;
    public MeshRenderer canMagnetizeIdicator;
    public bool         canMagnetize;
    public float        distanceToWren;
    public float        oDistanceToWren;
    public LineRenderer isMagnetizedLine;
    public float        magnetizableCollectionDistance = 1f;

    // Start is called before the first frame update
    private void Start()
    {


    }

    // Update is called once per frame
    private void Update()
    {

        float timeSinceHit = God.state.totalTimeInGame - timeHit;

        if ( timeSinceHit > respawnTime && collected && respawnAfterTime ) {
            Respawn();
        }


        if ( magnetizable && !collected ) {

            oDistanceToWren = distanceToWren;
            distanceToWren = (God.wren.transform.position - centerTransform.position).magnitude;

            // we only want to turn off and on magnetize if we are not being magnetized so the bird cant fly away too fast!
            if ( beingMangetized == false ) {

                if ( distanceToWren < magnetizableDistance && oDistanceToWren > magnetizableDistance ) {
                    canMagnetize = true;
                    canMagnetizeIdicator.enabled = true;
                } else if ( distanceToWren > magnetizableDistance && oDistanceToWren < magnetizableDistance ) {
                    canMagnetize = false;
                    canMagnetizeIdicator.enabled = false;
                }


            }

            if ( canMagnetize && !beingMangetized ) {
                if ( God.wren.magnetize.isMagnetized == true ) {
                    StartMagnetizing();
                }

            }

            if ( beingMangetized ) {
                /*if ( God.wren.input.square < .5f ) {
                    StopMangetizing();
                }*/
            }

            if ( beingMangetized ) {
                DoBeingMangetized();
            } else {
                DoNotBeingMangetized();
            }
        }

    }

    public void StartMagnetizing()
    {
        print( "Start magnetization" );
        beingMangetized = true;
        velocity = Vector3.zero;
        magnetizer = God.wren.transform;
        canMagnetize = false;
        canMagnetizeIdicator.enabled = false;
    }

    public void OnMagnetizableCollect()
    {

        magnetizableTransform.position = centerTransform.position;
        beingMangetized = false;
        canMagnetize = false;
        canMagnetizeIdicator.enabled = false;
        velocity = Vector3.zero;
        DoNotBeingMangetized();

    }

    public void StopMangetizing()
    {
        beingMangetized = false;
        canMagnetize = false;
        canMagnetizeIdicator.enabled = false;
    }

    public void DoBeingMangetized()
    {
        var dir = (magnetizableTransform.position - magnetizer.position).normalized;
        velocity += dir * magnetizeForce * Time.deltaTime;
        magnetizableTransform.position += velocity * Time.deltaTime;
        velocity *= 1 - dampening * Time.deltaTime;
        isMagnetizedLine.positionCount = 2;

        var pos = magnetizableTransform.position;

        if ( collectionPosition != null ) {
            pos = collectionPosition.position;
        }

        isMagnetizedLine.SetPosition( 0 , pos );
        isMagnetizedLine.SetPosition( 1 , magnetizer.position );

        if ( (magnetizableTransform.position - magnetizer.position).magnitude < magnetizableCollectionDistance ) {
            DoCollect();
        }

    }

    public void DoNotBeingMangetized()
    {
        var dir = (magnetizableTransform.position - centerTransform.position).normalized;
        velocity += dir * returnForce * Time.deltaTime;
        magnetizableTransform.position += velocity * Time.deltaTime;
        velocity *= 1 - dampening * Time.deltaTime;
        isMagnetizedLine.positionCount = 0;
    }


    public void Respawn()
    {

        collected = false;

        if ( Collected != null ) {
            Collected.SetActive( false );
        }

        if ( Uncollected != null ) {
            Uncollected.SetActive( true );
        }

        if ( respawnEvent != null ) {
            respawnEvent.Invoke( gameObject );
        }


    }

    private void OnEnable()
    {

        if ( respawnAfterTime ) {
            timeHit = God.state.totalTimeInGame;
            Respawn();
        } else {

            if ( collected ) {
                if ( Collected != null ) {
                    Collected.SetActive( true );
                }

                if ( Uncollected != null ) {
                    Uncollected.SetActive( false );
                }
            } else {
                if ( Collected != null ) {
                    Collected.SetActive( false );
                }

                if ( Uncollected != null ) {
                    Uncollected.SetActive( true );
                }
            }
        }
    }

    public void OnTriggerEnter( Collider c )
    {

        if ( God.IsOurWren( c ) ) {

            DoCollect();

        }

    }


    public void DoCollect()
    {
        print( "LFG" );

        var collectPosition = God.wren.transform.position;

        if ( collectionPosition != null ) {
            collectPosition = collectionPosition.position;
        }

        God.wren.shards.CollectShards( ShardsToAdd , type , collectPosition );
        God.particleSystems.Emit( God.particleSystems.shardCollect , collectPosition , ShardsToAdd );

        if ( firstCollect == false ) {
            firstCollect = true;

            if ( onFirstCollectEvent != null ) {
                onFirstCollectEvent.Invoke( gameObject );
            }
        }

        if ( onCollectEvent != null ) {
            onCollectEvent.Invoke( gameObject );
        }

        collected = true;

        if ( Collected != null ) {
            Collected.SetActive( true );
        }

        if ( Uncollected != null ) {
            Uncollected.SetActive( false );
        }

        timeHit = God.state.totalTimeInGame;

        if ( destroyOnCollect ) {
            Destroy( gameObject );
        }

        if ( magnetizable ) {
            OnMagnetizableCollect();
        }
    }
}