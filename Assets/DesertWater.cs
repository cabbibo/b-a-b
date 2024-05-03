using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;

[ExecuteAlways]
public class DesertWater : MonoBehaviour
{

    public Biome biome;

    public List<Transform> waterLocations;
    public Transform locationToWater;

    public float amountWatered;

    public float pickUpRadius;
    public float dropRadius;

    public bool wrenCarryingWater;
    public GameObject wrenWithWater;
    public GameObject wrenGatheringWater;

    public ParticleSystem onWaterParticles;


    public float carryingScaleMultiplier;
    public float waterPickUpSpeed;
    public float waterKillSpeed;
    public float waterValue;

    public float maxWaterValue = 1;

    public LineRenderer whileGatheringLine;


    // Start is called before the first frame update
    void OnEnable()
    {




        if (biome.completed == false)
        {
            amountWatered = 0;
        }


        if (biome.completed == true)
        {
            amountWatered = 1;
        }

        // wren shouldn't be on fire to start!
        wrenWithWater.SetActive(false);


    }

    public Transform debugWren;

    public Transform testTransform;

    public bool isGathering;


    // Update is called once per frame
    void Update()
    {


        if (God.wren == null) { testTransform = debugWren; } else { testTransform = God.wren.transform; }

        isGathering = false;


        for (int i = 0; i < waterLocations.Count; i++)
        {
            if (!wrenCarryingWater)
            {

                float d = Vector3.Distance(waterLocations[i].position, testTransform.position);
                if (d < pickUpRadius)
                {
                    print("is gathering");
                    isGathering = true;
                    WhileWrenGatheringWater(i);
                }

            }
        }

        if (isGathering == false)
        {
            whileGatheringLine.positionCount = 0;
        }



        if (waterValue > 0 && isGathering == false)
        {
            wrenCarryingWater = true;
        }

        if (isGathering == true && waterValue >= maxWaterValue)
        {
            MaxWaterCollected();
        }

        if (wrenCarryingWater)
        {
            WhileWrenCarryingWater();
        }

        wrenGatheringWater.SetActive(isGathering);
        wrenWithWater.SetActive(wrenCarryingWater);

    }

    public void WhileWrenGatheringWater(int i)
    {
        whileGatheringLine.positionCount = 2;
        whileGatheringLine.SetPosition(0, testTransform.position);
        whileGatheringLine.SetPosition(1, waterLocations[i].position);


        wrenGatheringWater.transform.position = testTransform.position;
        waterValue += waterPickUpSpeed;

        if (waterValue > maxWaterValue)
        {
            waterValue = maxWaterValue;
        }

    }

    public void WhileWrenCarryingWater()
    {
        if (Vector3.Distance(locationToWater.position, testTransform.position) < dropRadius)
        {
            DropOffWater();
        }

        wrenWithWater.transform.position = testTransform.position;
        wrenWithWater.transform.localScale = Vector3.one * (carryingScaleMultiplier * waterValue);
        waterValue -= waterKillSpeed;


        if (waterValue < 0)
        {
            waterValue = 0;
            wrenCarryingWater = false;

        }





    }

    public void DropOffWater()
    {
        amountWatered += waterValue;
        waterValue = 0;
        wrenCarryingWater = false;
        biome.SetCompletion(amountWatered);

        onWaterParticles.Play();
    }


    // Do Something fun for collection here
    public void MaxWaterCollected()
    {
        wrenCarryingWater = true;

    }

}
