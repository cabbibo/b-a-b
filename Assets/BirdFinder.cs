using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFinder : MonoBehaviour
{

    public Biome biome;

    public int numberOfTimesToFind;

    public int timesFound;

    public void OnEnable()
    {
        timesFound = 0;

    }
    public void OnBirdFind()
    {

        print("Bird found");

        timesFound++;
        biome.SetCompletion((float)timesFound / (float)numberOfTimesToFind);

    }



}
