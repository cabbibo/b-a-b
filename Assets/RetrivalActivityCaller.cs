using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Activity activity;
    public Transform retrivalPoint;
    public Transform startLocation;

    public float dotMatchForLeaving = 0.5f;


    public void OnActivityStart()
    {
        // switch main point of interest to thing to retrive

        activity.mainPointOfInterest = retrivalPoint;


    }

    public void Update()
    {


        if (activity.doingActivity)
        {

            // check if we are flying in the wrong direction


        }


    }

    public Carryable carryable;

    public void OnCarryablePickUp()
    {
        // set back to original location
        activity.mainPointOfInterest = startLocation;

        // now we check to see if we are close enough to the crystal to decide if we need to leave the activity



    }

}
