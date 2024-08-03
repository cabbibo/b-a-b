using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Biome : MonoBehaviour
{

    public AudioClip[] loops;
    public AudioClip[] ambiance;

    public Color color;

    public List<GameObject> localObjects;

    public GameObject fog;

    //TODO:

    // Make it so that every object fades in and fades out somehow
    // Make it so fog fades in and fades out









    /*
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

        public IslandData islandData;

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

            if (completed == false)
            {
                print("HELLOOO");

                portal.OpenPortal();
                completedAnimation.Play();
            }
            else
            {
                print("already completed");

            }

        }

        public void StartBiome()
        {

            if (started == false)
            {
                started = true;
                God.state.OnBiomeStarted(id);
                startedAnimation.Play();
            }
            else
            {
                print("already started");
            }
        }


        public void Initialize()
        {

            discovered = God.state.biomesDiscovered[id];
            started = God.state.biomesStarted[id];
            completed = God.state.biomesCompleted[id];


            completedAnimation.SetStartValues();
            startedAnimation.SetStartValues();
            discoveredAnimation.SetStartValues();


            // Setting state from animations!
            if (!discovered)
            {
                print("HELLO I AM NOT DISCOVERED");
                discoveredAnimation.SetStartValues();
            }

            if (discovered && !started)
            {
                print("discovered not started");
                discoveredAnimation.SetEndValues();
                startedAnimation.SetStartValues();
            }

            if (discovered && started && !completed)
            {
                print("discovered started not completed");
                discoveredAnimation.SetEndValues();
                startedAnimation.SetEndValues();
                completedAnimation.SetStartValues();
            }

            if (discovered && started && completed)
            {
                print("Setting All End Values");
                discoveredAnimation.SetEndValues();
                startedAnimation.SetEndValues();
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

            // print("HELLO I AM ENABLED");
            //print(gameObject.name);
            //print(discovered);
            //print(started);
            //print(completed);


        }

   

        public void OnCompletedAnimationFinished()
        {

            completed = true;
            God.state.OnBiomeCompleted(id);
        }
    */


    public void OnEnterBiome()
    {

        //print("HELLO I AM ENTERED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(true);
        }

        /*  if (!discovered)
          {
              DiscoverBiome();
          }*/


    }

    public void OnExitBiome()
    {
        //print("HELLO I AM EXITED");

        /*for (int i = 0; i < localObjects.Count; i++)
         {
             localObjects[i].SetActive(false);
         }*/

    }
}
