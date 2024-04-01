using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenShardManager : MonoBehaviour
{

    public int numShards = 0;
    public int maxShards = 100000;

    public Wren wren;

    public FullBird fullBird;


    public int numShardsInBody = 0;

    public int oShards = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        numShardsInBody = fullBird.totalShards;
        numShards = PlayerPrefs.GetInt("Shards", 0);
    }

    public float GetBodyShardPercentage()
    {
        return (float)numShards / (float)numShardsInBody;
    }

    public float GetShardPercentage()
    {
        return (float)numShards / (float)maxShards;
    }



    public float GetShardTrailAmount()
    {
        return numShards - numShardsInBody;
    }

    public float GetMaxShardTrailAmount()
    {
        return maxShards - numShardsInBody;
    }

    public float GetShardTrailPercentage()
    {
        return (float)GetShardTrailAmount() / (float)GetMaxShardTrailAmount();
    }

    // Update is called once per frame
    void Update()
    {

        if (oShards != numShards)
        {
            oShards = numShards;
            UpdateShards();
        }

        //        print(GetBodyShardPercentage());
        fullBird.percentageRendered = GetBodyShardPercentage();
    }


    public void CollectShard()
    {
        numShards++;
        UpdateShards();
    }

    public void CollectShards(int amount)
    {
        print("collected");
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

    public float bodyPercentage;
    public float shardPercentage;
    public float shardTrailPercentage;

    public void UpdateShards()
    {

        numShards = Mathf.Clamp(numShards, 0, maxShards);
        PlayerPrefs.SetInt("Shards", numShards);

        bodyPercentage = GetBodyShardPercentage();
        shardPercentage = GetShardPercentage();
        shardTrailPercentage = GetShardTrailPercentage();

    }



    public void SpendAllShards()
    {
        numShards = 0;
        UpdateShards();
    }

}
