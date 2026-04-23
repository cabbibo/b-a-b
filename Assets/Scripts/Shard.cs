using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif


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


    public bool canUncollect = false;

    [Header( "Persistence" )]
    public bool saveCollectionStatus = false;

    [SerializeField]
    private string uniqueShardID;

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

    private string SaveKeyCollected => $"shard_collected_{uniqueShardID}";
    private string SaveKeyFirstCollect => $"shard_firstCollect_{uniqueShardID}";


    private void Awake()
    {


        LoadState();
        ApplyStateVisuals();
    }

    private void Start()
    {
    }

    private void Update()
    {
        float timeSinceHit = God.state.totalTimeInGame - timeHit;

        if ( timeSinceHit > respawnTime && collected && respawnAfterTime ) {
            Respawn();
        }

        if ( magnetizable && !collected ) {

            oDistanceToWren = distanceToWren;
            distanceToWren = (God.wren.transform.position - centerTransform.position).magnitude;

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
        if ( magnetizableTransform != null && centerTransform != null ) {
            magnetizableTransform.position = centerTransform.position;
        }

        beingMangetized = false;
        canMagnetize = false;

        if ( canMagnetizeIdicator != null ) {
            canMagnetizeIdicator.enabled = false;
        }

        velocity = Vector3.zero;
        DoNotBeingMangetized();
    }

    public void StopMangetizing()
    {
        beingMangetized = false;
        canMagnetize = false;

        if ( canMagnetizeIdicator != null ) {
            canMagnetizeIdicator.enabled = false;
        }
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
        if ( magnetizableTransform == null || centerTransform == null ) {
            return;
        }

        var dir = (magnetizableTransform.position - centerTransform.position).normalized;
        velocity += dir * returnForce * Time.deltaTime;
        magnetizableTransform.position += velocity * Time.deltaTime;
        velocity *= 1 - dampening * Time.deltaTime;

        if ( isMagnetizedLine != null ) {
            isMagnetizedLine.positionCount = 0;
        }
    }

    public void Respawn()
    {
        SetCollectedState( false , false );

        if ( respawnEvent != null ) {
            respawnEvent.Invoke( gameObject );
        }
    }

    private void OnEnable()
    {
        LoadState();

        if ( respawnAfterTime ) {
            timeHit = God.state.totalTimeInGame;
            Respawn();
        } else {
            ApplyStateVisuals();
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
        if ( collected && !canUncollect ) {
            return;
        }

        print( "LFG" );
        
        

        SetCollectedState( !collected , saveCollectionStatus );

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


        timeHit = God.state.totalTimeInGame;

        if ( magnetizable ) {
            OnMagnetizableCollect();
        }

        if ( destroyOnCollect ) {
            if ( saveCollectionStatus ) {
                gameObject.SetActive( false );
            } else {
                Destroy( gameObject );
            }
        }
    }

    private void SetCollectedState( bool newCollectedState , bool saveState )
    {
        collected = newCollectedState;
        ApplyStateVisuals();

        if ( saveState ) {
            SaveState();
        }
    }

    private void ApplyStateVisuals()
    {
        if ( Collected != null ) {
            Collected.SetActive( collected );
        }

        if ( Uncollected != null ) {
            Uncollected.SetActive( !collected );
        }

        if ( magnetizable ) {
            beingMangetized = false;
            canMagnetize = false;
            velocity = Vector3.zero;

            if ( canMagnetizeIdicator != null ) {
                canMagnetizeIdicator.enabled = false;
            }

            if ( isMagnetizedLine != null ) {
                isMagnetizedLine.positionCount = 0;
            }

            if ( collected && magnetizableTransform != null && centerTransform != null ) {
                magnetizableTransform.position = centerTransform.position;
            }
        }

        if ( saveCollectionStatus && destroyOnCollect ) {
            gameObject.SetActive( !collected );
        }
    }

    private void SaveState()
    {
        if ( !saveCollectionStatus ) {
            return;
        }

        if ( string.IsNullOrEmpty( uniqueShardID ) ) {
            return;
        }

        PlayerPrefs.SetInt( SaveKeyCollected , collected ? 1 : 0 );
        PlayerPrefs.SetInt( SaveKeyFirstCollect , firstCollect ? 1 : 0 );
        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        if ( !saveCollectionStatus ) {
            return;
        }

        if ( string.IsNullOrEmpty( uniqueShardID ) ) {
            return;
        }

        if ( PlayerPrefs.HasKey( SaveKeyCollected ) ) {
            collected = PlayerPrefs.GetInt( SaveKeyCollected , 0 ) == 1;
        }

        if ( PlayerPrefs.HasKey( SaveKeyFirstCollect ) ) {
            firstCollect = PlayerPrefs.GetInt( SaveKeyFirstCollect , 0 ) == 1;
        }

        print( "shard correct " + collected );
    }


    private string GetHierarchyPath( Transform current )
    {
        string path = current.name;

        while (current.parent != null) {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureUniqueID();

        EditorUtility.SetDirty( this );
    }

    private void EnsureUniqueID()
    {
        if ( Application.isPlaying ) {
            return;
        }

        bool needsNewId = string.IsNullOrEmpty( uniqueShardID ) || HasDuplicateID( uniqueShardID );

        if ( needsNewId ) {
            uniqueShardID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty( this );
        }
    }

    private bool HasDuplicateID( string id )
    {
        if ( string.IsNullOrEmpty( id ) ) {
            return false;
        }

        var allShards = Resources.FindObjectsOfTypeAll<Shard>();
        int count = 0;

        foreach (var shard in allShards) {
            if ( EditorUtility.IsPersistent( shard ) ) {
                continue;
            } // skip prefab assets

            if ( shard.uniqueShardID == id ) {
                count++;

                if ( count > 1 ) {
                    return true;
                }
            }
        }

        return false;
    }
#endif

    public void ClearSavedState()
    {
        if ( string.IsNullOrEmpty( uniqueShardID ) ) {
            return;
        }

        PlayerPrefs.DeleteKey( SaveKeyCollected );
        PlayerPrefs.DeleteKey( SaveKeyFirstCollect );
        PlayerPrefs.Save();
    }
}