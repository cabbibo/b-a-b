using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BiomeController : MonoBehaviour
{


    public Biome[] biomes;

    public BiomeMusic biomeMusic;


    public float maxBiomeValue;
    public float secondMaxBiomeValue;

    public float oMaxBiomeValue;
    public float oSecondMaxBiomeValue;


    public int maxBiomeID;
    public int secondMaxBiomeID;



    public int oMaxBiomeID;
    public int oSecondMaxBiomeID;


    // Update is called once per frame
    void Update()
    {

        oSecondMaxBiomeValue = secondMaxBiomeValue;
        oMaxBiomeID = maxBiomeID;

        oMaxBiomeValue = maxBiomeValue;
        oSecondMaxBiomeValue = secondMaxBiomeValue;

        maxBiomeValue = 0;
        secondMaxBiomeValue = 0;

        maxBiomeID = -1;
        oSecondMaxBiomeID = -1;

        if (God.islandData != null)
        {

            for (int i = 0; i < God.islandData.currentBiomeValues.Length; i++)
            {
                if (God.islandData.currentBiomeValues[i] > maxBiomeValue)
                {
                    secondMaxBiomeValue = maxBiomeValue;
                    secondMaxBiomeID = maxBiomeID;
                    if (secondMaxBiomeValue == 0)
                    {
                        secondMaxBiomeValue = God.islandData.currentBiomeValues[i];
                        secondMaxBiomeID = i;
                    }
                    maxBiomeValue = God.islandData.currentBiomeValues[i];
                    maxBiomeID = i;
                }
            }





            if (maxBiomeID != oMaxBiomeID)
            {
                OnBiomeChange(oMaxBiomeID, maxBiomeID);
            }

        }

    }



    public Helpers.DoubleIntEvent BiomeChangeEvent;
    public void OnBiomeChange(int oldBiome, int newBiome)
    {
        OnExitBiome(oldBiome);
        OnEnterBiome(newBiome);

        BiomeChangeEvent.Invoke(oldBiome, newBiome);

    }


    public void OnExitBiome(int oldBiome)
    {

        //print("old Biome : " + oldBiome);
        if (oldBiome == -1 || oldBiome == 7)
        {
            LeaveNeutralZone();
        }
        else
        {

            //biomes[oldBiome].OnExitBiome();
        }

    }

    public void OnEnterBiome(int newBiome)
    {
        //        print(" new Biome : " + newBiome);
        if (newBiome == -1 || newBiome == 7)
        {
            EnterNeutralZone();
        }
        else
        {
            //biomes[newBiome].OnEnterBiome();
        }
    }

    public void EnterNeutralZone()
    {

    }

    public void LeaveNeutralZone()
    {

    }


}
