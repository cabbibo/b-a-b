using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BiomePaintPreyManager : PreyManager
{

    public IslandController islandController;

    public int foodType;


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

}
