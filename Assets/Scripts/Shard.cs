using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

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

    public Vector3 collisionSpeed;

    public void OnTriggerEnter( Collider c )
    {

        if ( God.IsOurWren( c ) ) {

            print( "LFG" );

            var collectPosition = transform.position;

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


        }

    }
}