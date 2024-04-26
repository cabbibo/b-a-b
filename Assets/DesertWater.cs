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

    public GameObject flamePrefab;
    public GameObject wrenWithWater;
    public GameObject wrenGatheringWater;

    public ParticleSystem onWaterParticles;


    public float carryingScaleMultiplier;
    public float waterPickUpSpeed;
    public float waterKillSpeed;
    public float waterValue;

    public float maxWaterValue = 1;


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


    // Update is called once per frame
    void Update()
    {


        if (God.wren == null) { testTransform = debugWren; } else { testTransform = God.wren.transform; }


        bool isGathering = false;

        for (int i = 0; i < waterLocations.Count; i++)
        {
            if (!wrenCarryingWater)
            {

                if (Vector3.Distance(waterLocations[i].position, testTransform.position) < pickUpRadius)
                {
                    isGathering = true;
                    WhileWrenGatheringWater();
                }

            }
        }


        if (waterValue > 0)
        {
            wrenCarryingWater = true;
        }

        if (wrenCarryingWater)
        {
            WhileWrenCarryingWater();
        }

        wrenGatheringWater.SetActive(isGathering);
        wrenWithWater.SetActive(wrenCarryingWater);

    }

    public void WhileWrenGatheringWater()
    {

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
        wrenWithWater.transform.localScale = Vector3.one * (.001f * carryingScaleMultiplier * waterValue);
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


}
