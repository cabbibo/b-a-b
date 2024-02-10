using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenShardManager : MonoBehaviour
{

    public int numShards = 0;
    public int maxShards = 100000;

    // Start is called before the first frame update
    void OnEnable()
    {
        numShards = PlayerPrefs.GetInt("Shards", 0);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void CollectShard()
    {
        numShards++;
        UpdateShards();
    }

    public void CollectShards(int amount)
    {
        numShards += amount;
        UpdateShards();
    }

    public void SpendShards(int amount)
    {
        numShards -= amount;
        UpdateShards();
    }


    public void SpendShard(int amount)
    {
        numShards--;
        UpdateShards();
    }

    public void UpdateShards()
    {

        numShards = Mathf.Clamp(numShards, 0, maxShards);
        PlayerPrefs.SetInt("Shards", numShards);
    }

}
