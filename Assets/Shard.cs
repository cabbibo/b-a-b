using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Shard : MonoBehaviour
{

    public int ShardsToAdd;
    public bool destroyOnCollect = true;

    public bool collected = false;

    public GameObject Uncollected;
    public GameObject Collected;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        if (collected)
        {
            Collected.SetActive(true);
            Uncollected.SetActive(false);
        }
        else
        {
            Collected.SetActive(false);
            Uncollected.SetActive(true);
        }
    }


    public void OnTriggerEnter(Collider c)
    {

        if (God.IsOurWren(c))
        {

            print("LFG");
            God.wren.shards.CollectShards(ShardsToAdd);
            God.particleSystems.Emit(God.particleSystems.shardCollect, transform.position, ShardsToAdd);
            collected = true;
            Collected.SetActive(true);
            Uncollected.SetActive(false);


            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
        }

    }
}
