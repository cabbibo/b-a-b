using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Quest : MonoBehaviour
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

            print("Quest complete");
            amountComplete = 1;
            CompleteQuest();
        }
    }

    public void SetCompletion(float amount)
    {
        amountComplete = amount;
        if (amountComplete >= 1)
        {
            print("Quest complete");
            amountComplete = 1;
            CompleteQuest();
        }

    }



    public void DiscoverQuest()
    {

        print("HELLOOO111");
        discovered = true;
        God.state.OnQuestDiscovered(id);
        discoveredAnimation.Play();

    }
    public void CompleteQuest()
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

    public void StartQuest()
    {

        if (started == false)
        {
            started = true;
            God.state.OnQuestStarted(id);
            startedAnimation.Play();
        }
        else
        {
            print("already started");
        }
    }


    public void Initialize()
    {

        discovered = God.state.questsDiscovered[id];
        started = God.state.questsStarted[id];
        completed = God.state.questsCompleted[id];


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

    public void OnEnterQuest()
    {

        //print("HELLO I AM ENTERED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(true);
        }

        if (!discovered)
        {
            DiscoverQuest();
        }


    }

    public void OnExitQuest()
    {
        //print("HELLO I AM EXITED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(false);
        }

    }

    public void OnCompletedAnimationFinished()
    {
        completed = true;
        God.state.OnQuestCompleted(id);
    }


}
