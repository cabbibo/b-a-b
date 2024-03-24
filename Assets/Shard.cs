using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Shard : MonoBehaviour
{

    public int ShardsToAdd;
    public bool destroyOnCollect = true;

    public bool collected = false;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnTriggerEnter(Collider c)
    {

        if (God.IsOurWren(c))
        {

            print("LFG");
            God.wren.shards.CollectShards(ShardsToAdd);
            God.particleSystems.Emit(God.particleSystems.shardCollect, transform.position, ShardsToAdd);
            collected = true;
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
        }

    }
}
