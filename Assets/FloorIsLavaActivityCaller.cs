using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class FloorIsLavaActivityCaller : MonoBehaviour
{



    public float startRadius;

    public float finishRadius;

    public Activity activity;
    public Transform endLocation;
    public Transform startLocation;



    public float dotMatchForLeaving = 0.5f;

    public int framesTilStartLeaving = 5000; // might be a bit more wonky to get to the right place?
    public int framesOutside = 0;

    public float radius;
    public float oRadius;

    public float currentDot;





    public void Start()
    {



        radius = (God.wren.transform.position - activity.mainPointOfInterest.position).magnitude;
        oRadius = radius;


        if (radius < startRadius)
        {
            activity.OnActivityAreaEntered();
        }


    }



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


            // flash our ring to show time left
            float v = (Time.time - activity.activityStartTime) / activity.timeAllowedToCompleteActivity;
            God.wren.interfaceUtils.SetRingValue(1, 1 - v);
            God.wren.interfaceUtils.SetRingFade(1, Mathf.Sin(v * 100) + 1);


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


            // check to see if we can end the activity!
            oRadius = radius;
            radius = (God.wren.transform.position - endLocation.position).magnitude;

            /// GO GO GO 
            activity.mainPointOfInterest.position = endLocation.position;

            if (radius <= finishRadius && oRadius > finishRadius)
            {
                print("ACTIVITY COMPLETE");
                activity.AddToComplete(1000);
            }


        }


    }




    public void OnActivityStart()
    {

        // switch main point of interest to thing to retrive

        activity.mainPointOfInterest.position = endLocation.position;

        God.wren.interfaceUtils.SetRingValue(1, 0);
        God.wren.interfaceUtils.SetRingFade(1, 0);

    }






    public void TurnOnActivityEvent()
    {

        print("TURN ON ACTIVITY EVENT 1");
        activity.mainPointOfInterest.position = endLocation.position;

        God.wren.interfaceUtils.SetRingValue(1, 0);
        God.wren.interfaceUtils.SetRingFade(1, 0);

    }

    public void TurnOffActivityEvent()
    {
        print("TURN OFF ACTIVITY EVENT1");
        activity.mainPointOfInterest.position = startLocation.position;

        God.wren.interfaceUtils.SetRingValue(1, 0);
        God.wren.interfaceUtils.SetRingFade(1, 0);
    }

    public void AreaEntered()
    {
        print("Area Entered");
        // Dont reset if we are already in it
        if (activity.doingActivity == false)
        {
            activity.mainPointOfInterest.position = startLocation.position;

            God.wren.interfaceUtils.SetRingValue(1, 0);
            God.wren.interfaceUtils.SetRingFade(1, 0);
        }

    }

    public void AreaExited()
    {
    }


    public void FullExitActivityArea()
    {

        activity.mainPointOfInterest.position = startLocation.position;

        God.wren.interfaceUtils.SetRingValue(1, 0);
        God.wren.interfaceUtils.SetRingFade(1, 0);

    }

    public void OnLavaHit()
    {
        print("HIT LAVA");

    }



}
