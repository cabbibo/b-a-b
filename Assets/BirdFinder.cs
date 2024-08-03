using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFinder : MonoBehaviour
{

    public Quest quest;

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
        quest.SetCompletion((float)timesFound / (float)numberOfTimesToFind);

    }



}
