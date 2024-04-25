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

    public List<GameObject> localObjects;

    public void AddToCompletion(float amount)
    {
        amountComplete += amount;

        if (amountComplete >= 1)
        {

            print("Biome complete");
            amountComplete = 1;
            CompleteBiome();
        }
    }

    public void SetCompletion(float amount)
    {
        amountComplete = amount;
        if (amountComplete >= 1)
        {
            print("Biome complete");
            amountComplete = 1;
            CompleteBiome();
        }

    }



    public void DiscoverBiome()
    {

        print("HELLOOO111");
        discovered = true;
        God.state.OnBiomeDiscovered(id);
        discoveredAnimation.Play();

    }
    public void CompleteBiome()
    {

        print("HELLOOO");

        completed = true;
        portal.OpenPortal();
        God.state.OnBiomeCompleted(id);
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


        completedAnimation.SetStartValues();
        startedAnimation.SetStartValues();
        discoveredAnimation.SetStartValues();

        if (discovered)
        {
            discoveredAnimation.SetEndValues();
        }
        if (started)
        {
            startedAnimation.SetEndValues();
        }
        if (completed)
        {
            completedAnimation.SetEndValues();
        }


        if (!completed)
        {
            portal.SetPortalOff();
        }
        else
        {
            portal.SetPortalFull();
        }

        print("HELLO I AM ENABLED");
        print(gameObject.name);
        print(discovered);
        print(started);
        print(completed);


    }

    public void OnEnterBiome()
    {

        print("HELLO I AM ENTERED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(true);
        }

        if (!discovered)
        {
            DiscoverBiome();
        }


    }

    public void OnExitBiome()
    {
        print("HELLO I AM EXITED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(false);
        }

    }


}
