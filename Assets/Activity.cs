using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

public class Activity : MonoBehaviour
{

    public int id;

    public bool discovered;
    public bool started;
    public bool completed;


    public UnityEvent onActivityEnter;
    public UnityEvent onActivityExit;

    public UnityEvent onDiscoverEvent;
    public UnityEvent onStartEvent;
    public UnityEvent onCompleteEvent;
    public Helpers.FloatEvent onAddToCompletionEvent;


    public float radiusToDiscover = 10;
    public float radiusToStart = 10;

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
            CompleteActivity();
        }
    }

    public void SetCompletion(float amount)
    {
        amountComplete = amount;
        if (amountComplete >= 1)
        {
            amountComplete = 1;
            CompleteActivity();
        }

    }



    public void DiscoverActivity()
    {


        if (discovered == false)
        {
            discovered = true;
            God.state.OnActivityDiscovered(id);
            if (discoveredAnimation)
            {
                discoveredAnimation.Play();
            }
        }

    }
    public void CompleteActivity()
    {

        if (completed == false)
        {
            completed = true;
            God.state.OnActivityCompleted(id);
            if (completedAnimation != null)
            {
                completedAnimation.Play();
            }
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
            God.state.OnActivityStarted(id);
            if (startedAnimation != null)
            {
                startedAnimation.Play();
            }
        }
        else
        {
            print("already started");
        }
    }


    public void Initialize()
    {


        discovered = God.state.activitiesDiscovered[id];
        started = God.state.activitiesStarted[id];
        completed = God.state.activitiesCompleted[id];




        if (discoveredAnimation)
        {
            if (!discovered)
            {
                discoveredAnimation.SetStartValues();
            }
            else
            {
                discoveredAnimation.SetEndValues();
            }

        }


        if (startedAnimation)
        {
            if (!started)
            {
                startedAnimation.SetStartValues();
            }
            else
            {
                startedAnimation.SetEndValues();
            }
        }

        if (completedAnimation)
        {
            if (!completed)
            {
                completedAnimation.SetStartValues();
            }
            else
            {
                completedAnimation.SetEndValues();
            }
        }


        // print("HELLO I AM ENABLED");
        //print(gameObject.name);
        //print(discovered);
        //print(started);
        //print(completed);


    }

    public void OnEnterActivity()
    {

        //print("HELLO I AM ENTERED");

        for (int i = 0; i < localObjects.Count; i++)
        {
            localObjects[i].SetActive(true);
        }

        if (!discovered)
        {
            DiscoverActivity();
        }


    }

    public void OnExitActivity()
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
        God.state.OnActivityCompleted(id);
    }

    public void Reset()
    {
        discovered = false;
        started = false;
        completed = false;
        God.state.ResetActivity(id);

    }

}
