using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WrenUtils;

public class ShardGroup : MonoBehaviour
{
    public List<Shard> shards = new();

    public float distanceToCare;

    public UnityEvent OnAllShardsCollected;

    public bool allCollected;
    public bool allCollectedOnce;

    // Start is called before the first frame update
    private void Start()
    {
        foreach (var shard in shards) shard.enabled = false;

    }

    // Update is called once per frame
    private void Update()
    {
        if ( (God.wren.transform.position - transform.position).magnitude < distanceToCare ) {
            foreach (var shard in shards) shard.enabled = true;
        } else {
            foreach (var shard in shards) shard.enabled = false;
        }

        if ( allCollected ) {
            return;
        }

        allCollected = true;

        foreach (var shard in shards)
            if ( shard.collected == false ) {
                allCollected = false;
                break;
            }

        if ( allCollected && allCollectedOnce == false ) {
            allCollectedOnce = true;
            OnAllShardsCollected.Invoke();
            print( "hiii" );
        }

    }
}