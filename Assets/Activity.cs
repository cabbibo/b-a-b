using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

// Activities are small things to do across that map
// They can be redone over and over again, and you get crystals for doing them
// Each one has a trigger that you fly through to enter it
public class Activity : MonoBehaviour
{

    public int id;


    public float amountToComplete = 1;
    public float currentAmountComplete = 0;
    public float percentangeComplete = 0;

    public bool discovered;
    public bool started;
    public bool completed;



    public int numCrystalsAward;
    public float crystalType;

    public Transform mainPointOfInterest;

    public string activityName;

    public Slide[] discoverSlides;
    public Slide[] startSlides;
    public Slide[] startAgainSlides;
    public Slide[] completeSlides;

    public Slide[] redoSlides;

    public Slide currentSlide;

    public bool inSlide;

    public Transform wrenPosition;

    public float AmountComplete;

    public float slideChangeTime;




    public void AddToComplete(float v)
    {
        currentAmountComplete += v;
        percentangeComplete = currentAmountComplete / amountToComplete;
        if (currentAmountComplete >= amountToComplete)
        {
            OnComplete();
        }
    }







    public void Update()
    {
        if (inSlide)
        {
            if (God.input.x)
            {
                print("NEXT SLIDE");
                if (Time.time - slideChangeTime > .3f)
                {
                    NextSlide();
                }
            }

            if (God.input.circle)
            {
                EndSlide(currentSlide);
            }
        }

    }

    public bool insideActivity;
    // Different Cameras for explaining the task
    // public Slide[] slides;

    public float releaseTime;


    public void OnActivityInfoAreaEntered()
    {
        if (!insideActivity) // need to be able to jump into the slides and not reactivate!
        {
            insideActivity = true;
            if (!started && !completed)
            {
                DoDiscover();
            }

            if (started && !completed)
            {
                OnStartAgain();
            }

            if (completed)
            {
                OnRedo();
            }
        }


    }

    public void OnActivityInfoAreaExited()
    {

        insideActivity = false;
    }






    public void DoDiscover()
    {

        print("DO DISCOVER");

        discovered = true;



        SetSlide(discoverSlides[0]);


    }

    public void SetSlide(Slide s)
    {

        God.wren.Crash(wrenPosition.position);
        God.wren.physics.rb.isKinematic = true;
        God.wren.physics.rb.position = wrenPosition.position;
        God.wren.physics.rb.rotation = wrenPosition.rotation;
        God.wren.state.canTakeOff = false;

        slideChangeTime = Time.time;
        currentSlide = s;
        inSlide = true;
        s.Set();



    }

    public void EndSlide(Slide s)
    {
        inSlide = false;
        s.Release();
        God.wren.state.canTakeOff = true;
        Release();
    }

    public void Release()
    {
        print("RELEASE");

        releaseTime = Time.time;
        God.wren.physics.rb.isKinematic = false;

    }



    public void Reset()
    {
        currentAmountComplete = 0;
        percentangeComplete = 0;
    }

    public void DiscoverEnd()
    {
        SetSlide(startSlides[0]);
    }

    public void StartEnd()
    {

        started = true;
        EndSlide(currentSlide);
    }


    public void OnComplete()
    {
        insideActivity = true;
        SetSlide(completeSlides[0]);
    }

    public void OnCompleteEnd()
    {
        GiveReward();
        completed = true;
        EndSlide(currentSlide);
    }

    public void GiveReward()
    {
        God.wren.shards.CollectShards(numCrystalsAward, crystalType, mainPointOfInterest.position);
    }


    public void OnStartAgain()
    {
        SetSlide(startAgainSlides[0]);
    }


    public void OnRedo()
    {
        Reset();
        completed = false;// reshow the information!
        SetSlide(redoSlides[0]);
    }



    public void NextSlide()
    {

        slideChangeTime = Time.time;
        if (currentSlide == null)
        {
            Debug.LogError("No current slide");
            return;
        }

        for (int i = 0; i < discoverSlides.Length; i++)
        {
            if (discoverSlides[i] == currentSlide)
            {
                if (i < discoverSlides.Length - 1)
                {
                    SetSlide(discoverSlides[i + 1]);
                    return;
                }
                else
                {
                    DiscoverEnd();
                    //EndSlide(currentSlide);
                }
            }
        }


        for (int i = 0; i < startSlides.Length; i++)
        {
            if (startSlides[i] == currentSlide)
            {
                if (i < startSlides.Length - 1)
                {
                    SetSlide(startSlides[i + 1]);
                    return;
                }
                else
                {
                    StartEnd();
                    //EndSlide(currentSlide);
                }
            }
        }


        for (int i = 0; i < startAgainSlides.Length; i++)
        {
            if (startAgainSlides[i] == currentSlide)
            {
                if (i < startAgainSlides.Length - 1)
                {
                    SetSlide(startAgainSlides[i + 1]);
                    return;
                }
                else
                {
                    StartEnd();
                    //EndSlide(currentSlide);
                }
            }
        }


        for (int i = 0; i < completeSlides.Length; i++)
        {
            if (completeSlides[i] == currentSlide)
            {
                if (i < completeSlides.Length - 1)
                {
                    SetSlide(completeSlides[i + 1]);
                    return;
                }
                else
                {

                    OnCompleteEnd();
                }
            }
        }


        for (int i = 0; i < redoSlides.Length; i++)
        {
            if (redoSlides[i] == currentSlide)
            {
                if (i < redoSlides.Length - 1)
                {
                    SetSlide(redoSlides[i + 1]);
                    return;
                }
                else
                {

                    StartEnd();
                }
            }
        }

    }






}
