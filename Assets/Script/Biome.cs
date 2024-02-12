using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Biome : MonoBehaviour
{

    public bool discovered;
    public bool started;
    public bool completed;

    public int id;

    public Portal portal;
    public float amountComplete;


    public void AddToCompletion(float amount)
    {
        amountComplete += amount;

        if (amountComplete >= 1)
        {
            amountComplete = 1;
            CompleteBiome();
        }
    }



    public void DiscoverBiome()
    {

        discovered = true;
        God.state.OnBiomeDiscovered(id);

    }
    public void CompleteBiome()
    {

        completed = true;
        God.state.OnBiomeCompleted(id);
        portal.OpenPortal();

    }

    public void StartBiome()
    {

        started = true;
        God.state.OnBiomeStarted(id);
    }


    public void OnEnable()
    {

        discovered = God.state.biomesDiscovered[id];
        started = God.state.biomesStarted[id];
        completed = God.state.biomesCompleted[id];


        if (!completed)
        {
            portal.SetPortalOff();
        }

    }


}
