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

    public Transform collectionPosition;


    public float type;



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
            if (Collected != null) Collected.SetActive(true);
            if (Uncollected != null) Uncollected.SetActive(false);
        }
        else
        {
            if (Collected != null) Collected.SetActive(false);
            if (Uncollected != null) Uncollected.SetActive(true);
        }
    }


    public void OnTriggerEnter(Collider c)
    {

        if (God.IsOurWren(c))
        {

            print("LFG");

            Vector3 collectPosition = transform.position;
            if (collectionPosition != null) collectPosition = collectionPosition.position;
            God.wren.shards.CollectShards(ShardsToAdd, type, collectPosition);
            God.particleSystems.Emit(God.particleSystems.shardCollect, collectPosition, ShardsToAdd);
            collected = true;

            if (Collected != null) Collected.SetActive(true);
            if (Uncollected != null) Uncollected.SetActive(false);


            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
        }

    }
}
