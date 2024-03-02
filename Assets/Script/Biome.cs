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


    public PlayCutScene discoveredAnimation;
    public PlayCutScene startedAnimation;
    public PlayCutScene completedAnimation;


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
        discoveredAnimation.Play();

    }
    public void CompleteBiome()
    {

        completed = true;
        God.state.OnBiomeCompleted(id);
        portal.OpenPortal();
        completedAnimation.Play();

    }

    public void StartBiome()
    {

        started = true;
        God.state.OnBiomeStarted(id);
        startedAnimation.Play();
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

    public void OnEnterBiome()
    {
        print("Biome entered");
        if (!discovered)
        {
            DiscoverBiome();
        }


    }

    public void OnExitBiome()
    {
        print("Biome exited");

    }


}
