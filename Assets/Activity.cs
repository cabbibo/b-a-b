using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

// Activities are small things to do across that map
// They can be redone over and over again, and you get crystals for doing them
// Each one has a trigger that you fly through to enter it

[ExecuteAlways]
public class Activity : MonoBehaviour
{


    [Header("Input")]

    public string activityName;


    public float amountToComplete = 1;
    public float currentAmountComplete = 0;
    public float percentangeComplete = 0;

    public bool doingActivity;



    public bool discovered;
    public bool started;
    public bool completed;


    public bool canStartMidActivityOnLoad = false;


    public int numCrystalsAward;
    public float crystalType;


    public bool timedActivity;
    public bool stopOnTimeUp;

    public bool restartOnFailure;

    public bool restartOnComplete;

    public bool tieredActivity; // if we have multiple levels of completion

    public int[] crystalRewards; // how many crystals you get for each tier of completion
    public float[] tierValues; // how much you need to get to each tier of completion
    public string[] tierCompleteText; // what you get for each tier of completion

    public float timeAllowedToCompleteActivity;

    public bool exitingActivityArea;

    public float timeAllowedWhileExitedActivityArea = 10; // how long you have to get back in the activity area before you Quit


    public Transform mainPointOfInterest; // place where we will be looking / focusing ( usually the shade position? )


    public Slide[] discoverSlides;
    public Slide[] startSlides;
    public Slide[] startAgainSlides;
    public Slide[] completeSlides;

    public Slide[] redoSlides;

    public Slide[] completedCantRedoSlides;

    public Slide[] onFailureSlides;
    //public Slide[] onSuccessSlides; // dont need these bc they are just the complete slides






    public Transform wrenPosition; // place to move the wren to when starting and finishing the activity



    public UnityEvent onDiscoverEvent;
    public UnityEvent onStartEvent;
    public UnityEvent onCompleteEvent;
    public UnityEvent onRedoEvent;
    public UnityEvent onStartAgainEvent;

    public float activityCooldownTime; // TIME BEFORE WE CAN REDO THE ACTIVITY AGAIN

    public float timeExitedActivityArea;
    public bool inActivityArea;


    public GameObject[] inActivityAreaObjects; // ones we turn on in activity area

    public GameObject[] doingActivityObjects; // ones we turn on while doing the activity







    [Header("Data")]

    public float AmountComplete;
    public float slideChangeTime;
    public bool insideActivityInfoArea;

    public float releaseTime;


    public float timeCompleted;

    public Slide currentSlide;

    public bool inSlide;

    public string fullName;

    public int uniqueID;

    public float activityStartTime;


    public float totalTimeAllowedToBeInActivityArea = 1000;


    // Save them for the future!
    public float bestTime = 10000000;
    public float bestAmountComplete = 0;

    public float timeToFinishActivity;





    public void OnEnable()
    {
        if (uniqueID == 0)
        {
            uniqueID = Random.Range(1, 100000000); // holy shit so spooky and hacky
        }

        fullName = activityName + " " + uniqueID;

        LoadState();

        // reset time completed if we have reset our totalTimeInGame state! ( only really matters in edit mode not in built stuff!)
        if (timeCompleted < God.state.totalTimeInGame)
        {
            timeCompleted = God.state.totalTimeInGame;
            SaveState();

        }



    }


    public void Update()
    {

        // only need to deal with leaving the area if we are actually in the activity?
        if (exitingActivityArea)
        {

            float timeSinceExit = Time.time - timeExitedActivityArea;
            float nTime = timeSinceExit / timeAllowedWhileExitedActivityArea;

            WhileExitingActivityArea(nTime);

            if (nTime > 1)
            {
                FullExitActivityArea();
            }

        }

        // we are currently doing the activity!
        if (started && !completed && doingActivity)
        {

            if (stopOnTimeUp && Time.time - activityStartTime > timeAllowedToCompleteActivity)
            {
                timeToFinishActivity = Time.time - activityStartTime;
                OnTimeUp();
            }

        }


        if (inSlide)
        {
            if (God.input.x)
            {
                // print("NEXT SLIDE");
                if (Time.time - slideChangeTime > .3f)
                {
                    NextSlide();
                }
            }

            if (God.input.circle)
            {

                // only cancel if the slide lets us
                if (currentSlide.canCancel)
                {
                    EndSlide(currentSlide);
                }
            }
        }

    }







    // Do Time Up tiers 
    public void OnTimeUp()
    {


        print("ON TIME UP");

        // if its a timed activity, we use the amount complete to determine tier rewards
        if (tieredActivity)
        {

            // we didnt get anything!
            if (currentAmountComplete < tierValues[0])
            {
                OnFailure();
                return;
            }

            // see if we got to a tier or not
            for (int i = 0; i < tierValues.Length - 1; i++)
            {

                if (currentAmountComplete >= tierValues[i] && currentAmountComplete < tierValues[i + 1])
                {

                    DoAmountCompleteBasedTier(i);
                    return;

                }



            }

            if (currentAmountComplete >= tierValues[tierValues.Length - 1])
            {
                DoAmountCompleteBasedTier(tierValues.Length - 1);
                return;
            }


        }
        else
        {
            OnFailure();
        }
    }


    public void DoAmountCompleteBasedTier(int tier)
    {

        print("Amount Complete Based Tier : " + tier);

        // we got to this tier!
        completeSlides[0].text = tierCompleteText[tier];
        numCrystalsAward = crystalRewards[tier];


        if (currentAmountComplete > bestAmountComplete)
        {
            bestAmountComplete = currentAmountComplete;
            SaveState();
        }

        OnComplete();

    }




    public void DoTimeBasedTier(int tier)
    {

        print("Time Based Tier : " + tier);
        // we got to this tier!
        completeSlides[0].text = tierCompleteText[tier];
        numCrystalsAward = crystalRewards[tier];

        if (timeToFinishActivity < bestTime)
        {
            bestTime = timeToFinishActivity;
            SaveState();
        }

        OnComplete();

    }

    public void OnFailure()
    {

        print("ON FAILURE");
        doingActivity = false;


        // hack to make sure entering info area doesn get retriggered!
        insideActivityInfoArea = true;

        QuitActivity();
        SetSlide(onFailureSlides[0]); // set the wren position here! 
    }



    public void OnActivityAreaEntered()
    {

        print("Activity Area Entered");
        inActivityArea = true;


        // re entering after exiting!
        if (exitingActivityArea)
        {
            exitingActivityArea = false;
        }


        for (int i = 0; i < inActivityAreaObjects.Length; i++)
        {
            inActivityAreaObjects[i].SetActive(true);
        }

        // turn on inActivityArea stuff 
    }

    public void OnActivityAreaExited()
    {

        print("Activity Area Exited");
        // turn off inActivityArea stuff    
        exitingActivityArea = true;
        timeExitedActivityArea = Time.time;
    }

    public void FullExitActivityArea()
    {
        print("FULL EXIT ACTIVITY AREA");
        // Turn stuff off, end being in activity
        inActivityArea = false;
        exitingActivityArea = false;// we are not exiting anymore
        for (int i = 0; i < inActivityAreaObjects.Length; i++)
        {
            inActivityAreaObjects[i].SetActive(false);
        }

        if (doingActivity)
        {
            QuitActivity();

        }
    }


    public void QuitActivity()
    {
        print("QUIT ACTIVITY");

        doingActivity = false;
        started = false; // get introduced to it again
        for (int i = 0; i < doingActivityObjects.Length; i++)
        {
            doingActivityObjects[i].SetActive(false);
        }
        // Turn off the doingActivity stuff
    }


    public void WhileExitingActivityArea(float nTime)
    {
        // do stuff while exiting the activity area to warn you you are leaving!
        print("ALERT LEAVIGN : " + nTime);

    }


    // get close enough to shade to be starting the activity
    public void OnActivityInfoAreaEntered()
    {
        print("Activity Info Area Entered");
        if (!insideActivityInfoArea) // need to be able to jump into the slides and not reactivate!
        {
            insideActivityInfoArea = true;
            if (!discovered && !started && !completed)
            {
                DoDiscover();
            }

            if (discovered && !started && !completed)
            {

                // this means we have quit or failed the activity or just said we didn't want to do it again
                DoDiscover(); // TODO a different slide set for this?

            }

            if (started && !completed) // we are in the middle of doing the activity
            {
                OnStartAgain();
            }

            if (completed)
            {
                if (God.state.totalTimeInGame - timeCompleted > activityCooldownTime)
                {
                    OnRedo();
                }
                else
                {
                    OnCompletedCantRedoStart();
                }
            }
        }


    }


    // 
    public void OnActivityInfoAreaExited()
    {
        print("Activity Info Area Exited");
        insideActivityInfoArea = false;
    }



    public void Reset()
    {
        print("RESET");
        currentAmountComplete = 0;
        percentangeComplete = 0;
    }



    public void DoDiscover()
    {

        print("DO DISCOVER");

        discovered = true;
        onDiscoverEvent.Invoke();
        Reset();

        SaveState();
        SetSlide(discoverSlides[0]);

    }


    public void DiscoverEnd()
    {
        print("DISCOVER END");
        Reset();
        DoDoingActivityStart(); // we chose to do the activity!
        SetSlide(startSlides[0]);
    }

    public void StartEnd()
    {
        print("START END");

        started = true;
        doingActivity = true; // now we are actuallly doing the activity!
        //currentAmountComplete = 0; // reset the amount complete
        activityStartTime = Time.time;
        onStartEvent.Invoke();
        SaveState();
        EndSlide(currentSlide);

    }


    public void OnComplete()
    {
        print("ON COMPLETE");
        insideActivityInfoArea = true;

        SetSlide(completeSlides[0]);
    }

    public void OnCompleteEnd()
    {

        print("ON COMPLETE END");
        if (restartOnComplete)
        {
            OnStartAgain();

        }
        else
        {
            print("ON COMPLETE END");
            onCompleteEvent.Invoke();
            completed = true;
            timeCompleted = God.state.totalTimeInGame;
            QuitActivity();// turn off the doing activity stuff
            SaveState();
            EndSlide(currentSlide);
        }





    }

    public void GiveReward()
    {
        print("GIVE REWARD");
        God.wren.shards.CollectShards(numCrystalsAward, crystalType, mainPointOfInterest.position);
    }


    public void OnStartAgain()
    {
        print("ON START AGAIN");
        onStartAgainEvent.Invoke();
        DoDoingActivityStart();
        SetSlide(startAgainSlides[0]);
    }


    public void DoDoingActivityStart()
    {
        for (int i = 0; i < doingActivityObjects.Length; i++)
        {
            doingActivityObjects[i].SetActive(true);
        }
    }



    public void OnRedo()
    {
        print("ON REDO");
        Reset();
        onRedoEvent.Invoke();
        completed = false;// reshow the information!
        SaveState();
        SetSlide(redoSlides[0]);
    }


    // not going from discover -> start again, so need to call what we call on discover end
    public void OnRedoEnd()
    {


        OnStartAgain();


    }

    public void OnCompletedCantRedoStart()
    {
        print("ON COMPLETED CANT REDO START");
        SetSlide(completedCantRedoSlides[0]);

    }

    public void OnCompletedCantRedoEnd()
    {
        print("ON COMPLETED CANT REDO END");
        EndSlide(currentSlide);
    }



    public void OnFailureEnd()
    {
        print("ON FAILURE END");

        if (restartOnFailure)
        {
            OnStartAgain();

        }
        else
        {
            EndSlide(currentSlide);
        }
    }












    /*



        HELPER FUNCTIONS

    */
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
                    StartEnd();// Dont want to start over here 
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

                    OnRedoEnd();
                }
            }
        }


        for (int i = 0; i < completedCantRedoSlides.Length; i++)
        {
            if (completedCantRedoSlides[i] == currentSlide)
            {
                if (i < completedCantRedoSlides.Length - 1)
                {
                    SetSlide(completedCantRedoSlides[i + 1]);
                    return;
                }
                else
                {

                    OnCompletedCantRedoEnd();
                }
            }
        }

        for (int i = 0; i < onFailureSlides.Length; i++)
        {
            if (onFailureSlides[i] == currentSlide)
            {
                if (i < onFailureSlides.Length - 1)
                {
                    SetSlide(onFailureSlides[i + 1]);
                    return;
                }
                else
                {

                    OnFailureEnd();
                }
            }
        }

    }




    public void SaveState()
    {
        PlayerPrefs.SetFloat("Activity_" + fullName + "_AmountComplete", currentAmountComplete);
        PlayerPrefs.SetFloat("Activity_" + fullName + "_TimeCompleted", timeCompleted);
        PlayerPrefs.SetInt("Activity_" + fullName + "_Discovered", discovered ? 1 : 0);
        PlayerPrefs.SetInt("Activity_" + fullName + "_Started", started ? 1 : 0);
        PlayerPrefs.SetInt("Activity_" + fullName + "_Completed", completed ? 1 : 0);
        PlayerPrefs.SetFloat("Activity_" + fullName + "_BestTime", bestTime);
        PlayerPrefs.SetFloat("Activity_" + fullName + "_BestAmountComplete", bestAmountComplete);


    }

    public void LoadState()
    {

        currentAmountComplete = PlayerPrefs.GetFloat("Activity_" + fullName + "_AmountComplete", 0);
        timeCompleted = PlayerPrefs.GetFloat("Activity_" + fullName + "_TimeCompleted", 0);
        discovered = PlayerPrefs.GetInt("Activity_" + fullName + "_Discovered", 0) == 1;

        // started should always be false here? we dont want to be starting mid activity, except for really big ones???? 
        if (canStartMidActivityOnLoad)
        {
            started = PlayerPrefs.GetInt("Activity_" + fullName + "_Started", 0) == 1;
        }
        else
        {
            started = false;
        }
        completed = PlayerPrefs.GetInt("Activity_" + fullName + "_Completed", 0) == 1;




        bestTime = PlayerPrefs.GetFloat("Activity_" + fullName + "_BestTime", 10000000);
        bestAmountComplete = PlayerPrefs.GetFloat("Activity_" + fullName + "_BestAmountComplete", 0);


    }


    // Called from outside this function!
    public void AddToComplete(float v)
    {

        if (!completed)
        {
            currentAmountComplete += v;
            percentangeComplete = currentAmountComplete / amountToComplete;
            if (currentAmountComplete >= amountToComplete)
            {

                print("Amount over amount to complete");

                timeToFinishActivity = Time.time - activityStartTime;


                // if its a tiered activity, we use the time to determine tier rewards    
                if (tieredActivity)
                {

                    print("Tiered");
                    if (timeToFinishActivity > tierValues[0])
                    {
                        print("Time over tier 0");
                        OnFailure();
                    }
                    else
                    {
                        print("Time under tier 0");
                        for (int i = 0; i < tierValues.Length - 1; i++)
                        {

                            print("Checking tier " + i);
                            if (timeToFinishActivity > tierValues[i + 1] && timeToFinishActivity <= tierValues[i])
                            {
                                print("is tier " + i);
                                DoTimeBasedTier(i);
                            }
                        }

                        if (timeToFinishActivity < tierValues[tierValues.Length - 1])
                        {
                            print("is tier " + (tierValues.Length - 1));
                            DoTimeBasedTier(tierValues.Length - 1);
                        }

                    }

                }
                else
                {

                    if (timedActivity)
                    {
                        print("Timed");
                        // see if we finished it in time or not                    
                        if (timeToFinishActivity > timeAllowedToCompleteActivity)
                        {
                            OnFailure();
                        }
                        else
                        {
                            OnComplete();
                        }


                    }
                    else
                    {
                        print("Basic");
                        OnComplete();
                    }

                }

            }
        }
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

        releaseTime = Time.time;
        God.wren.physics.rb.isKinematic = false;

    }






}
