using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BiomePaintPreyManager : PreyManager
{
    public IslandController islandController;

    public int foodType;
    public int biomeType;


    public override void CheckForNewPrey()
    {

        float value = islandController.currentIsland.currentFoodValues[foodType];

        // biome
        float currentBiomeValue = islandController.currentIsland.currentBiomeValues[biomeType];

        print( value );

        if ( value >= 0 && currentBiomeValue >= 0 ) {
            if ( Time.time - lastSpawnTime > spawnTime ) {
                SpawnNewBug();
            }
        }

    }
}