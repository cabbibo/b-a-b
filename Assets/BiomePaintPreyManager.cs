using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BiomePaintPreyManager : PreyManager
{

    public IslandController islandController;

    public int foodType;

    public float spawnDistance;


    public override void CheckForNewPrey()
    {

        float value = islandController.currentIsland.currentFoodValues[foodType];

        if (value != 0)
        {
            print(value);

            if (Time.time - lastSpawnTime > spawnTime)
            {
                SpawnNewBug();
            }
        }

    }

    public override void SpawnNewBug()
    {

        // destroy any over max
        while (preyHolder.childCount >= maxPray)
        {
            DestroyImmediate(preyHolder.GetChild(0).gameObject);
        }


        Vector3 spawnPos = God.wren.transform.position + God.wren.transform.forward * spawnDistance;

        spawnPos.y = 10000;

        RaycastHit hit;
        if (Physics.Raycast(spawnPos, Vector3.down, out hit, 20000))
        {
            spawnPos = hit.point + Vector3.up * spawnRadius * 2;
        }


        Vector3 offset = Random.insideUnitSphere * spawnRadius;

        spawnPos += offset;

        PreyController newPrey = Instantiate(preyPrefab, spawnPos, Quaternion.identity).GetComponent<PreyController>();

        newPrey.Initialize(preyConfig, this);


        newPrey.transform.parent = preyHolder;


        lastSpawnTime = Time.time;

    }

}
