using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class WrenShardManager : MonoBehaviour
{

    public int numShards = 0;
    public int maxShards = 100000;

    public Wren wren;

    public FullBird fullBird;


    public int numShardsInBody = 0;


    public int oNumShards = 0;
    public int tmpNumShards = 0;

    public Vector3 collectPosition;
    public float collectType;


    public float lostPerCrash;
    public float gainedPerSkim;
    public float gainedWhileClose;


    public float boostNumLost;
    public float reverseNumLost;
    public float callNumLost;


    public void DoBoost()
    {
        print("SPEND");
        SpendShards((int)boostNumLost);
    }

    public void DoDisintegrate()
    {

    }

    public void DoReverse()
    {

        SpendShards((int)reverseNumLost);

    }

    public void DoCall()
    {

    }

    public void DoWater()
    {

    }

    public void DoDodgeRight()
    {

    }

    public void DoDodgeLeft()
    {

    }

    // Claw?
    // Charge?


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

        if (oNumShards == numShards)
        {
            tmpNumShards = numShards;
        }

        if (oNumShards != numShards)
        {
            tmpNumShards = oNumShards;
            oNumShards = numShards;
            UpdateShards();
        }


        //        print(GetBodyShardPercentage());
        // fullBird.percentageRendered = GetBodyShardPercentage();

    }

    public void CollectShard()
    {
        numShards++;
        collectType = -1;
        collectPosition = wren.transform.position;
        UpdateShards();
    }


    public void CollectShard(float type)
    {
        numShards++;
        collectType = type;
        collectPosition = wren.transform.position;
        UpdateShards();
    }

    public void CollectShards(int amount, float type)
    {
        print("collected");
        numShards += amount;
        collectType = type;
        collectPosition = wren.transform.position;
        UpdateShards();
    }

    public void CollectShards(int amount, float type, Vector3 position)
    {
        print("collected");
        numShards += amount;
        collectType = type;
        collectPosition = position;
        UpdateShards();
    }


    public void CollectShards(int amount, float type, Transform position)
    {
        print("collected");
        numShards += amount;
        collectType = type;
        collectPosition = transform.position;
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
