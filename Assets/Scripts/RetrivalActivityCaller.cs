using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class RetrivalActivityCaller : MonoBehaviour
{


    // switch  main point of interest to thing to retrive when activity starts
    // Activity area entered on area near start
    // start leaving activity if we have been flying in the wrong direction for too long
    // full activity leave if we continue to fly in wrong direction
    // reenter if we start flying again in right direction ( towards thing to retreive )
    // once we have picked up thing to retrieve then the exiting comes from radius around thing to retrive
    // main point of interests switches back to start location
    // activity is complete when we have returned to start location with thing to retrieve

    public float startRadius;
    public float leaveCrystalRadius;

    public float finishRadius;

    public Activity activity;
    public Transform retrivalPoint;
    public Transform startLocation;


    public Carryable carryable;

    public float dotMatchForLeaving = 0.5f;

    public bool isCarrying = false;
    public bool hasPickedUpCrystal = false;


    public float radius;
    public float oRadius;

    public float currentDot;



    public void Start()
    {



        carryable.gameObject.SetActive(true);
        //activity.OnActivityStart += OnActivityStart;
        carryable.OnPickup.AddListener(OnCarryablePickUp);
        carryable.OnDrop.AddListener(OnCarryableDrop);

        radius = (God.wren.transform.position - activity.mainPointOfInterest.position).magnitude;
        oRadius = radius;


        if (radius < startRadius)
        {
            activity.OnActivityAreaEntered();
        }


    }

    public int framesTilStartLeaving = 1000;
    public int framesOutside = 0;

    public void Update()
    {


        if (!activity.doingActivity)
        {


            oRadius = radius;

            radius = (God.wren.transform.position - activity.mainPointOfInterest.position).magnitude;
            if (radius < startRadius && oRadius > startRadius)
            {
                // do we need to check if in slide?

                if (activity.inActivityArea == false)
                {
                    activity.OnActivityAreaEntered();
                }



            }

            if (radius > startRadius && oRadius < startRadius)
            {
                // do we need to check if in slide?
                if (activity.inActivityArea == true)
                {
                    activity.OnActivityAreaExited();
                }
            }


        }
        else // we are doing the activity
        {

            // check to see if we have picked up crystal or not, if we have can do a radius to crystal check!
            if (hasPickedUpCrystal == false)
            {

                Vector3 targetDir = activity.mainPointOfInterest.position - God.wren.transform.position;
                Vector3 forward = God.wren.transform.forward;

                float dot = Vector3.Dot(targetDir.normalized, forward.normalized);

                currentDot = dot;

                if (currentDot < dotMatchForLeaving)
                {
                    framesOutside++;

                    if (framesOutside > framesTilStartLeaving)
                    {
                        if (activity.exitingActivityArea == false)
                        { // TODO have to go wrong way for a bit before we say we are going the wrong way!

                            activity.OnActivityAreaExited();
                        }
                        //  AreaExited();
                    }
                }
                else
                {

                    framesOutside = 0;

                    if (activity.exitingActivityArea == true)
                    {
                        activity.OnActivityAreaEntered();
                        AreaEntered();
                    }
                }


            }
            else
            {

                // if we are currently carrying the crystal, pings should send back to the start location
                if (isCarrying == true)
                {



                    // check to see if we can end the activity!
                    oRadius = radius;
                    radius = (God.wren.transform.position - startLocation.position).magnitude;


                    // we have picked up the crystal
                    activity.mainPointOfInterest.position = startLocation.position;

                    if (radius <= finishRadius && oRadius > finishRadius)
                    {
                        print("ACTIVITY COMPLETE");
                        activity.AddToComplete(1000);
                    }

                }
                else
                {

                    print("crystal dropped but in activity");
                    activity.mainPointOfInterest.position = carryable.transform.position;

                    oRadius = radius;

                    radius = (God.wren.transform.position - carryable.transform.position).magnitude;

                    if (radius <= leaveCrystalRadius && oRadius > leaveCrystalRadius)
                    {
                        print("ENTERED ACTIVITY AREA cyrstal dropped");
                        activity.OnActivityAreaEntered();
                    }
                    else if (radius > leaveCrystalRadius && oRadius <= leaveCrystalRadius)
                    {
                        print("EXITED ACTIVITY AREA cyrstal dropped");
                        activity.OnActivityAreaExited();

                    }


                }
            }


        }
    }



    public void OnActivityStart()
    {

        // switch main point of interest to thing to retrive

        activity.mainPointOfInterest.position = retrivalPoint.position;
        isCarrying = false;

    }

    public void OnCarryablePickUp(Carryable c)
    {
        // set back to original location
        activity.mainPointOfInterest.position = startLocation.position;
        isCarrying = true;
        hasPickedUpCrystal = true;

        // now we check to see if we are close enough to the crystal to decide if we need to leave the activity

    }


    // point us back to
    public void OnCarryableDrop(Carryable c)
    {
        isCarrying = false;

        // Have to set this on update when not carrying
        activity.mainPointOfInterest.position = carryable.transform.position;

    }


    public void TurnOnActivityEvent()
    {

        print("TURN ON ACTIVITY EVENT 1");
        activity.mainPointOfInterest.position = retrivalPoint.position;
        carryable.TryToResetPosition(God.wren.carrying, retrivalPoint.position);
    }

    public void TurnOffActivityEvent()
    {
        print("TURN OFF ACTIVITY EVENT1");
        activity.mainPointOfInterest.position = startLocation.position;
        carryable.TryToResetPosition(God.wren.carrying, retrivalPoint.position);
    }

    public void AreaEntered()
    {
        print("Area Entered");
        // Dont reset if we are already in it
        if (activity.doingActivity == false)
        {
            activity.mainPointOfInterest.position = startLocation.position;
            carryable.TryToResetPosition(God.wren.carrying, retrivalPoint.position);
        }

    }

    public void AreaExited()
    {
        print("Area Exited");
        carryable.TryToResetPosition(God.wren.carrying, retrivalPoint.position);
    }


    public void FullExitActivityArea()
    {

        print("FULL EXIT ACTIVITY AREA");
        activity.mainPointOfInterest.position = startLocation.position;
        carryable.TryToResetPosition(God.wren.carrying, retrivalPoint.position);

    }


}
