using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;
using System.Linq;

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

    public float[] amountToCompleteTimesVisited;

    public bool doingActivity;



    public bool discovered;
    public bool started;
    public bool currentlyComplete;

    public int numTimesCompleted;

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




    /*


        SLIDES

    */


    public Slide[] discoverSlides;
    public Slide[] firstStartSlides; // first time you start


    public Slide[] reenterInfoAreaSlides; // re enter the activity info area while in activity


    public Slide[] completeSlides; // its finished! and some sort of succeess!

    public Slide[] failureSlides; // its finished! and you failed!



    public Slide[] restartSlides; // any time you restart the activity



    public Slide[] completedCantRedoSlides; // came back cant restart yet

    public Slide[] completedCanRedoSlides; // came back can restart!  ( after completion ) ( leads to start again slides )




    //public Slide[] onSuccessSlides; // dont need these bc they are just the complete slides






    public Transform wrenPosition; // place to move the wren to when starting and finishing the activity



    // are these necessary? maybe can add as needed?

    public UnityEvent onDiscoverEvent;
    public UnityEvent onStartEvent;
    public UnityEvent onCompleteEvent;
    public UnityEvent onRedoEvent;
    public UnityEvent onStartAgainEvent;


    public UnityEvent TurnOnActivityEvent;
    public UnityEvent TurnOffActivityEvent;

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


    public AudioSource leavingAudioSource;
    public AudioSource whileInLoopAudioSource;

    public float inActivityLoopVolume = 1;
    public float inActivityLoopPitch = 1;


    public float pitchChangeAmountOverTime = 1;
    public float pitchChangeAmountOverCompletenessAmount = 1;


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

        leavingAudioSource.clip = God.sounds.leavingActivityWhileInActivityLoop;
        leavingAudioSource.volume = 0;
        whileInLoopAudioSource.clip = God.sounds.whileInActivityLoop;
        whileInLoopAudioSource.volume = 0;

        // Set state Correctly depending on whats going on

        if (doingActivity)
        {
            TurnOnActivityObjects();
        }
        else
        {
            TurnOffActivityObjects();
        }

        if (inActivityArea)
        {


            for (int i = 0; i < inActivityAreaObjects.Length; i++)
            {
                inActivityAreaObjects[i].SetActive(true);
            }


        }
        else
        {
            for (int i = 0; i < inActivityAreaObjects.Length; i++)
            {
                inActivityAreaObjects[i].SetActive(false);
            }


        }
    }


    public void Update()
    {

        // only need to deal with leaving the area if we are actually in the activity?
        if (exitingActivityArea)
        {

            print("exitingActivityArea");
            float timeSinceExit = Time.time - timeExitedActivityArea;
            float nTime = timeSinceExit / timeAllowedWhileExitedActivityArea;

            print(nTime);
            WhileExitingActivityArea(nTime);

            if (nTime > 1)
            {

                God.audio.Play(God.sounds.inActivityFullExitedClip, 1, 1);
                FullExitActivityArea();
            }

        }


        // we are currently doing the activity!
        if (started && !currentlyComplete && doingActivity)
        {
            // we need to stop if time is up!
            if (stopOnTimeUp && Time.time - activityStartTime > timeAllowedToCompleteActivity)
            {
                timeToFinishActivity = Time.time - activityStartTime;
                OnTimeUp();
            }

        }


        // input for slides
        if (inSlide)
        {
            if (God.input.xPressed)
            {
                DoX();
            }

            if (God.input.circle)
            {
                DoCircle();
            }

            if (God.input.triangle)
            {
                print("tri pressssss");
                DoTriangle();
            }
        }

        if (inActivityArea && doingActivity)
        {
            whileInLoopAudioSource.volume = Mathf.Lerp(whileInLoopAudioSource.volume, inActivityLoopVolume, .05f);
        }

        if (!doingActivity)
        {
            whileInLoopAudioSource.volume = Mathf.Lerp(whileInLoopAudioSource.volume, 0, .05f);
        }


        if (inActivityArea)
        {
            leavingAudioSource.volume = Mathf.Lerp(leavingAudioSource.volume, 0, .1f);
            leavingAudioSource.pitch = Mathf.Lerp(leavingAudioSource.pitch, 0, .1f);
        }

    }





    /*

        Input Responses

    */

    public int xCount = 0;
    public int nextSlideCount = 0;

    public void DoX()
    {
        print("X Called");
        print(Time.time - slideChangeTime);
        xCount++;
        print(xCount);
        if (Time.time - slideChangeTime > .3f) // this is just to make it from going through them too fast!
        {
            nextSlideCount++;
            print("next slide count : " + nextSlideCount);
            NextSlide();
        }
    }

    public void DoCircle()
    {
        OnCancelButtonPressed();
    }


    //We will need to restart the activity if it is allowed on current slide
    public void DoTriangle()
    {

        print("do tri press");
        OnResetButtonPressed();
    }






    /*


        ENDING / FINISHING THE ACTIVITY


    */


    public void OnAmountCompleteReached()
    {
        print("Amount over amount to complete");

        timeToFinishActivity = Time.time - activityStartTime;

        God.audio.Play(God.sounds.largeSuccessSound, 1, 1);

        // if its a tiered activity, we use the time to determine tier rewards    
        if (tieredActivity)
        {

            print("Tiered");
            if (timeToFinishActivity > tierValues[0])
            {
                print("Time over tier 0");
                OnFailureBegin();
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
                    OnFailureBegin();
                }
                else
                {
                    OnCompleteBegin();
                }


            }
            else
            {
                print("Basic");
                OnCompleteBegin();
            }

        }
    }



    public void StraightFail()
    {

        if (doingActivity)
        {
            OnFailureBegin();
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
                OnFailureBegin();
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
        else // if its not a tiered activity, we just fail if we dont finish in time
        {
            OnFailureBegin();
        }



    }









    /*
    
    
    TIERS 
    
    
    
    */


    public void DoAmountCompleteBasedTier(int tier)
    {

        print("Amount Complete Based Tier : " + tier);

        // we got to this tier!
        SetTierInfo(tier);

        // Save the current best state
        if (currentAmountComplete > bestAmountComplete)
        {
            bestAmountComplete = currentAmountComplete;
            SaveState();
        }

        OnCompleteBegin();

    }




    public void DoTimeBasedTier(int tier)
    {

        print("Time Based Tier : " + tier);

        SetTierInfo(tier);

        if (timeToFinishActivity < bestTime)
        {
            bestTime = timeToFinishActivity;
            SaveState();
        }

        OnCompleteBegin();

    }












    /*

        Activity area exiting and entering;

    */




    public void OnActivityAreaEntered()
    {

        print("Activity Area Entered");
        inActivityArea = true;


        // re entering after exiting! we never actuall quit the activity so thats all good to go :)
        if (exitingActivityArea)
        {
            exitingActivityArea = false;
        }


        for (int i = 0; i < inActivityAreaObjects.Length; i++)
        {
            inActivityAreaObjects[i].SetActive(true);
        }



        if (doingActivity)
        {

            //God.wren.interfaceUtils.ClearPointers();

            God.audio.Play(God.sounds.activityReEnterClip, 1, 1);
        }
        else
        {

            God.audio.Play(God.sounds.activityEnterClip, 1, 1);

            // God.wren.interfaceUtils.AddPointer(mainPointOfInterest);
            God.wren.interfaceUtils.PingPointer(mainPointOfInterest); // just ping it once!

        }
        //God.wren.interfaceUtils.PingAll();


        // turn on inActivityArea stuff 
    }

    public void OnActivityAreaExited()
    {



        if (doingActivity)
        { // only need buffer for exiting if we are already in the activity

            // TODO this should be a loop that is a bit more controlable?
            God.audio.Play(God.sounds.inActivityExitBeginClip, 1, 1);

            print("Activity Area Exited");
            // turn off inActivityArea stuff    
            exitingActivityArea = true;
            timeExitedActivityArea = Time.time;

            leavingAudioSource.Play();
            leavingAudioSource.loop = true;
            leavingAudioSource.volume = 0;
            leavingAudioSource.pitch = 0;



        }
        else
        {

            God.audio.Play(God.sounds.activityAreaExitedClip, 1, 1);
            FullExitActivityArea();
        }
    }

    public void FullExitActivityArea()
    {
        //God.wren.interfaceUtils.RemovePointer(mainPointOfInterest);

        leavingAudioSource.Stop();

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


    public void WhileExitingActivityArea(float nTime)
    {
        // do stuff while exiting the activity area to warn you you are leaving!
        //  print("ALERT LEAVIGN : " + nTime);


        if (doingActivity)
        {


            leavingAudioSource.volume = Mathf.Lerp(leavingAudioSource.volume, nTime * nTime, .1f);
            leavingAudioSource.pitch = Mathf.Lerp(leavingAudioSource.pitch, nTime * 3, .1f);

            //print("should be setting");
            print((Mathf.Sin(nTime * nTime * 10000) + 1) / 3);
            //God.wren.interfaceUtils.AddPointer(mainPointOfInterest); // can call a bunch but shouldnt re add!
            God.wren.interfaceUtils.SetPointerFade(mainPointOfInterest, (Mathf.Sin(nTime * nTime * 10000) + 1) / 3);
        }

    }



    /*

    Activity INFO area entering and exiting

    */


    // get close enough to shade to be starting the activity
    public void OnActivityInfoAreaEntered()

    {
        print("Activity Info Area Entered");
        if (!insideActivityInfoArea) // need to be able to jump into the slides and not reactivate!
        {


            //God.wren.interfaceUtils.ClearPointers();// Get rid of all pointers

            print("entering into activityArea");
            insideActivityInfoArea = true;
            if (!discovered && !started && numTimesCompleted == 0)
            {
                OnDiscoverBegin();
                return;
            }

            if (discovered && !started && numTimesCompleted == 0)
            {

                // this means we have quit or failed the activity or just said we didn't want to do it again
                OnDiscoverBegin(); // TODO a different slide set for this?
                return;

            }

            if (started && !currentlyComplete) // we are in the middle of doing the activity
            {
                OnReenterInfoAreaBegin();
                return;

                //OnRestartBegin();
            }



            if (numTimesCompleted > 0)
            {
                if (God.state.totalTimeInGame - timeCompleted > activityCooldownTime)
                {
                    OnCompletedCanRedoBegin();
                }
                else
                {
                    OnCompletedCantRedoBegin();
                }

                return;
            }


        }


    }


    // 
    public void OnActivityInfoAreaExited()
    {
        print("Activity Info Area Exited");
        insideActivityInfoArea = false;
    }
























    /*


        THE STATE FUNCTION CALLS



        
    //discoverSlides;
    //firstStartSlides; 
    //reenterInfoAreaSlides
   // completeSlides
   // failureSlides
   // restartSlides
    //completedCantRedoSlides
    //completedCanRedoSlides

    */


    public void OnDiscoverBegin()
    {


        God.audio.Play(God.sounds.activityDiscoverClip, 1, 1);

        print("DISCOVER BEGIN");
        discovered = true;
        onDiscoverEvent.Invoke();

        // ResetPercentages(); // shouldnt need to reset?

        SaveState();

        SetSlide(discoverSlides[0]);

    }

    // We can cancel these types of slides!


    public void OnDiscoverEnd()
    {

        print("DISCOVER END");
        EndSlide(currentSlide); // end slide before we begin the next slide
        OnFirstStartBegin();
    }


    // nothing special
    public void OnDiscoverCancel()
    {

        print("DISCOVER CANCEL");
        EndSlide(currentSlide);

    }


    public void AlwaysStartBegin()
    {
        God.audio.Play(God.sounds.activityStartClip, 1, 1);
        TurnOnActivityObjects(); // we chose to do the activity! show the objects!
    }


    public void OnFirstStartBegin()
    {
        print("first START BEGIN");
        AlwaysStartBegin();

        SetSlide(firstStartSlides[0]);


    }

    public void OnFirstStartEnd()
    {

        print("First start end");

        started = true;

        BeginTheActualTask();

        onStartEvent.Invoke();
        SaveState();
        EndSlide(currentSlide);

    }


    public void OnFirstBeginCancel()
    {

        print("first start cancel");
        // turn off activity objects
        TurnOffActivityObjects();
        EndSlide(currentSlide);
    }




    public void OnReenterInfoAreaBegin()
    {

        print("REENTER INFO AREA");
        PauseTimer();
        SetSlide(reenterInfoAreaSlides[0]);
    }

    // can just cancel doing the activity using circle here

    // OR restart the activity if we want to start over


    // meant we chose to continue the activity
    public void OnReenterInfoAreaEnd()
    {
        print("REENTER INFO AREA END");

        UnpauseTimer();
        EndSlide(currentSlide);

    }


    // Quit activity
    public void OnReenterInfoAreaCancel()
    {

        print("REENTER INFO AREA CANCEL");

        // quit the activity completly
        QuitActivity();
        EndSlide(currentSlide);
    }







    public void OnCompleteBegin()
    {
        print("ON COMPLETE");
        insideActivityInfoArea = true;
        currentlyComplete = true;
        onCompleteEvent.Invoke();
        currentlyComplete = true;
        numTimesCompleted++;
        timeCompleted = God.state.totalTimeInGame;

        SetSlide(completeSlides[0]);
    }

    public void OnCompleteEnd()
    {

        print("ON COMPLETE END");
        if (restartOnComplete)
        {
            OnRestartBegin();

        }
        else
        {
            print("ON COMPLETE END");

            QuitActivity();// turn off the doing activity stuff
            SaveState();
            EndSlide(currentSlide);
        }

    }

    // this will happen if we want to make it so they can restart on complete!
    public void OnCompleteCancel()
    {
        print("ON COMPLETE END");

        onCompleteEvent.Invoke();
        currentlyComplete = true;
        numTimesCompleted++;
        timeCompleted = God.state.totalTimeInGame;
        QuitActivity();// turn off the doing activity stuff
        SaveState();
        EndSlide(currentSlide);

    }




    public void OnFailureBegin()
    {

        print("ON FAILURE");
        doingActivity = false;

        // hack to make sure entering info area doesn get retriggered!
        insideActivityInfoArea = true;

        QuitActivity();
        SetSlide(failureSlides[0]); // set the wren position here! 
    }






    public void OnFailureEnd()
    {
        print("ON FAILURE END");

        if (restartOnFailure)
        {
            OnRestartBegin();

        }
        else
        {
            EndSlide(currentSlide);
        }
    }


    // will this ever be possible?
    public void OnFailureCancel()
    {
        print("ON FAILURE CANCEL : SHOULDNT HAPPEN");

        EndSlide(currentSlide);

    }





    public void OnRestartBegin()
    {
        print("ON Restart");
        // onStartAgainEvent.Invoke();
        AlwaysStartBegin();
        SetSlide(restartSlides[0]);

    }

    public void OnRestartEnd()
    {


        print("on restart end");
        started = true;

        BeginTheActualTask();

        onStartEvent.Invoke();
        SaveState();
        EndSlide(currentSlide);


    }

    public void OnRestartCancel()
    {

        print("ON RESTART CANCEL");

        // turn off activity objects
        TurnOffActivityObjects();
        EndSlide(currentSlide);


    }




    public void OnCompletedCanRedoBegin()
    {
        print("On Completed Can Redo Begin");
        ResetPercentages();
        onRedoEvent.Invoke();
        SaveState();
        SetSlide(completedCanRedoSlides[0]);
    }


    // not going from discover -> start again, so need to call what we call on discover end
    public void OnCompletedCanRedoEnd()
    {
        print("ON COMPLETED CAN REDO END");
        OnRestartBegin();
    }

    public void OnCompletedCanRedoCancel() // can cancel it!!!
    {
        print("ON COMPLETED CAN REDO CANCEL");
        EndSlide(currentSlide);
    }


    public void OnCompletedCantRedoBegin()
    {
        print("ON COMPLETED CANT REDO START");
        SetSlide(completedCantRedoSlides[0]);

    }

    public void OnCompletedCantRedoEnd()
    {
        print("ON COMPLETED CANT REDO END");
        EndSlide(currentSlide);
    }


    // will this ever be possible?
    public void OnCompletedCantRedoCancel()
    {
        print("ON COMPLETED CANT REDO CANCEL : SHOULDNT HAPPEN");
        EndSlide(currentSlide);
    }













    /*



        HELPER FUNCTIONS

    */


    public float pauseTimeStart;
    public bool paused;
    public void PauseTimer()
    {
        paused = true;
        pauseTimeStart = Time.time;
    }


    // Add our amount of paused time to the activity start time so it doesnt mess up the time calculations
    public void UnpauseTimer()
    {
        paused = false;
        float timePaused = Time.time - pauseTimeStart;
        activityStartTime += timePaused;
    }




    // This is what we do when we *END* the activity
    public void QuitActivity()
    {
        print("QUIT ACTIVITY");

        doingActivity = false;
        started = false; // get introduced to it again

        TurnOffActivityObjects();
        // Turn off the doingActivity stuff
    }


    public void BeginTheActualTask()
    {


        print("Restarting the actual task");
        if (amountToCompleteTimesVisited != null)
        {
            if (numTimesCompleted < amountToCompleteTimesVisited.Length)
            {
                amountToComplete = amountToCompleteTimesVisited[numTimesCompleted];
            }
        }

        ResetPercentages();
        doingActivity = true;

        activityStartTime = Time.time;
    }




    // We are choosing to do the activity! we are not actually *in* it yet, just want to turn on the components so we can show them!
    public void TurnOnActivityObjects()
    {


        for (int i = 0; i < doingActivityObjects.Length; i++)
        {
            doingActivityObjects[i].SetActive(true);
        }

        print("TURN ON ACTIVITY OBJECTS");

        TurnOnActivityEvent.Invoke();
    }


    public void TurnOffActivityObjects()
    {
        for (int i = 0; i < doingActivityObjects.Length; i++)
        {
            doingActivityObjects[i].SetActive(false);
        }

        TurnOffActivityEvent.Invoke();
    }



    public void GiveReward()
    {
        print("GIVE REWARD");
        God.wren.shards.CollectShards(numCrystalsAward, crystalType, mainPointOfInterest.position);
    }




    public void ResetPercentages()
    {
        print("RESET");
        currentlyComplete = false;
        currentAmountComplete = 0;
        percentangeComplete = 0;
    }



    public void OnResetButtonPressed()
    {
        print("presssed");
        if (inSlide && currentSlide.canReset)
        {
            OnRestartBegin();
        }
    }


    public void OnCancelButtonPressed()
    {

        print("CANCEL BUTTON PRESSED");

        if (inSlide && currentSlide.canCancel)
        {


            if (discoverSlides.Contains(currentSlide))
            {
                OnDiscoverCancel();
            }

            if (firstStartSlides.Contains(currentSlide))
            {
                OnFirstBeginCancel();
            }

            if (reenterInfoAreaSlides.Contains(currentSlide))
            {
                OnReenterInfoAreaCancel();
            }

            if (completeSlides.Contains(currentSlide))
            {
                OnCompleteCancel();
            }

            if (failureSlides.Contains(currentSlide))
            {
                OnFailureCancel();
            }

            if (restartSlides.Contains(currentSlide))
            {
                OnRestartCancel();
            }

            if (completedCanRedoSlides.Contains(currentSlide))
            {
                OnCompletedCanRedoCancel();
            }

            if (completedCantRedoSlides.Contains(currentSlide))
            {
                OnCompletedCantRedoCancel();
            }

        }


    }



    public void NextSlide()
    {


        print("NEXT SLIDE");
        slideChangeTime = Time.time;
        if (currentSlide == null)
        {
            Debug.LogError("No current slide");
            return;
        }


        /*


        
    //discoverSlides;
    //firstStartSlides; 
    reenterInfoAreaSlides
   // completeSlides
   // failureSlides
   // restartSlides
    //completedCantRedoSlides
    //completedCanRedoSlides



*/

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

                    print("discover end called");
                    OnDiscoverEnd();
                    return;
                }
            }
        }


        for (int i = 0; i < firstStartSlides.Length; i++)
        {
            if (firstStartSlides[i] == currentSlide)
            {
                if (i < firstStartSlides.Length - 1)
                {
                    SetSlide(firstStartSlides[i + 1]);
                    return;
                }
                else
                {
                    OnFirstStartEnd();
                    return;
                }
            }
        }

        for (int i = 0; i < reenterInfoAreaSlides.Length; i++)
        {
            if (reenterInfoAreaSlides[i] == currentSlide)
            {
                if (i < reenterInfoAreaSlides.Length - 1)
                {
                    SetSlide(reenterInfoAreaSlides[i + 1]);
                    return;
                }
                else
                {
                    OnReenterInfoAreaEnd();
                    return;
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
                    return;
                }
            }
        }


        for (int i = 0; i < failureSlides.Length; i++)
        {
            if (failureSlides[i] == currentSlide)
            {
                if (i < failureSlides.Length - 1)
                {
                    SetSlide(failureSlides[i + 1]);
                    return;
                }
                else
                {

                    OnFailureEnd();
                    return;
                }
            }
        }



        for (int i = 0; i < restartSlides.Length; i++)
        {
            if (restartSlides[i] == currentSlide)
            {
                if (i < restartSlides.Length - 1)
                {
                    SetSlide(restartSlides[i + 1]);
                    return;
                }
                else
                {
                    OnRestartEnd();

                    return;// Dont want to start over here 
                    //EndSlide(currentSlide);
                }
            }
        }



        for (int i = 0; i < completedCanRedoSlides.Length; i++)
        {
            if (completedCanRedoSlides[i] == currentSlide)
            {
                if (i < completedCanRedoSlides.Length - 1)
                {
                    SetSlide(completedCanRedoSlides[i + 1]);
                    return;
                }
                else
                {
                    OnCompletedCanRedoEnd();
                    return;
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
                    return;
                }
            }
        }



    }




    public void SaveState()
    {
        PlayerPrefs.SetFloat("Activity_" + fullName + "_AmountComplete", currentAmountComplete);
        PlayerPrefs.SetFloat("Activity_" + fullName + "_TimeCompleted", timeCompleted);
        PlayerPrefs.SetInt("Activity_" + fullName + "_NumTimesCompleted", numTimesCompleted);
        PlayerPrefs.SetInt("Activity_" + fullName + "_Discovered", discovered ? 1 : 0);
        PlayerPrefs.SetInt("Activity_" + fullName + "_Started", started ? 1 : 0);
        PlayerPrefs.SetInt("Activity_" + fullName + "_CurrentlyComplete", currentlyComplete ? 1 : 0);
        PlayerPrefs.SetFloat("Activity_" + fullName + "_BestTime", bestTime);
        PlayerPrefs.SetFloat("Activity_" + fullName + "_BestAmountComplete", bestAmountComplete);


    }

    public void LoadState()
    {

        currentAmountComplete = PlayerPrefs.GetFloat("Activity_" + fullName + "_AmountComplete", 0);
        timeCompleted = PlayerPrefs.GetFloat("Activity_" + fullName + "_TimeCompleted", 0);
        discovered = PlayerPrefs.GetInt("Activity_" + fullName + "_Discovered", 0) == 1;

        numTimesCompleted = PlayerPrefs.GetInt("Activity_" + fullName + "_NumTimesCompleted", 0);


        // started should always be false here? we dont want to be starting mid activity, except for really big ones???? 
        if (canStartMidActivityOnLoad)
        {
            started = PlayerPrefs.GetInt("Activity_" + fullName + "_Started", 0) == 1;
        }
        else
        {
            started = false;
        }
        currentlyComplete = PlayerPrefs.GetInt("Activity_" + fullName + "_CurrentlyComplete", 0) == 1;

        bestTime = PlayerPrefs.GetFloat("Activity_" + fullName + "_BestTime", 10000000);
        bestAmountComplete = PlayerPrefs.GetFloat("Activity_" + fullName + "_BestAmountComplete", 0);


    }

    public void FullStateReset()
    {

        currentAmountComplete = 0;
        timeCompleted = 0;
        discovered = false;
        started = false;
        currentlyComplete = false;
        numTimesCompleted = 0;
        bestTime = 10000000;
        bestAmountComplete = 0;

        SaveState();
    }


    // Called from outside this function!
    public void AddToComplete(float v)
    {

        if (!currentlyComplete)
        {
            currentAmountComplete += v;
            percentangeComplete = currentAmountComplete / amountToComplete;

            God.wren.interfaceUtils.SetRingValue(0, percentangeComplete);
            God.wren.interfaceUtils.PingRing(0);

            if (currentAmountComplete >= amountToComplete)
            {


                OnAmountCompleteReached();

            }
        }
    }



    public void SetSlide(Slide s)
    {
        //        print("SET SLIDE");
        //       print("slide text pre replace");
        //      print(s.text);
        s.tmpText = ReplaceSlideText(s.text);
        //    print("slide text post replace");
        //  print(s.text);


        slideChangeTime = Time.time;

        currentSlide = s;
        inSlide = true;
        s.Set();

    }

    public void EndSlide(Slide s)
    {
        slideChangeTime = Time.time;
        inSlide = false;
        s.Release();
        Release();
    }

    public void Release()
    {

        releaseTime = Time.time;

    }


    public void SetTierInfo(int tier)
    {
        completeSlides[0].text = tierCompleteText[tier]; // TODO will this set shit wrong for the future?
        numCrystalsAward = crystalRewards[tier];
    }




    /*

        PARSING TEXT

    */

    public string ReplaceSlideText(string fullString)
    {

        // look for {} in the fullString
        string[] splitArray = fullString.Split('{');

        //print("splitARRRAY");

        if (splitArray.Length == 0)
        {
            //print("0 length");
            return fullString;
        }

        if (splitArray.Length == 1)
        {
            //print("no split needed");
            return fullString;

        }

        if (splitArray.Length > 1)
        {

            //("splitArray0" + splitArray[0]);
            //print("splitArray1" + splitArray[1]);

            string totalString = splitArray[0];

            for (int i = 0; i < splitArray.Length - 1; i++)
            {
                string[] subString = splitArray[i + 1].Split('}'); // get what we need
                //print(subString);
                //print(subString[0]);
                totalString += DoReplace(subString[0]);
                totalString += subString[1];
            }

            //totalString += splitArray[splitArray.Length - 1];

            //print("TOTAL STRING : " + totalString);

            return totalString;

        }

        return fullString;



    }

    /*

        TODOOOOOOOOOOOOO BIG TIME


    */
    public string DoReplace(string subString)
    {

        print("do replace" + subString);

        if (subString == "PC") // percentage complete
        {
            return percentangeComplete.ToString("P0");
        }
        else if (subString == "PL") // percentage left
        {
            return (1 - percentangeComplete).ToString("P0");
        }
        else if (subString == "AC") // amount complete
        {
            return currentAmountComplete.ToString();
        }
        else if (subString == "ATC") // amount to complete
        {
            return amountToComplete.ToString();
        }
        else if (subString == "AL") // amount left
        {
            return (amountToComplete - currentAmountComplete).ToString();
        }
        else if (subString == "BT") // best time
        {
            return bestTime.ToString("F2");
        }
        else if (subString == "BAC") // best amount complete
        {
            return bestAmountComplete.ToString();
        }
        else if (subString == "CT") // current time
        {
            return (pauseTimeStart - activityStartTime).ToString("F2");
        }
        else if (subString == "TTF") // time to finish
        {
            return timeToFinishActivity.ToString("F2");
        }
        else if (subString == "TAC") // time allowed to complete
        {
            return timeAllowedToCompleteActivity.ToString("F2");
        }
        else if (subString == "NTC") // numtimes completed
        {
            return numTimesCompleted.ToString();
        }
        else if (subString == "NCA") // num crystals award
        {
            return numCrystalsAward.ToString();
        }
        else if (subString == "CT") // crystal type
        {
            return crystalType.ToString();
        }
        else if (subString == "AN") // activity name
        {
            return fullName;
        }
        else if (subString == "NL") // number left
        {
            return amountToComplete - currentAmountComplete + "";
        }
        else if (subString == "TTR") // time til can redo TODO MIGHT BE WRONG
        {
            return (timeCompleted + activityCooldownTime - God.state.totalTimeInGame).ToString("F2");

        }



        return "REPALACS";
    }





}
