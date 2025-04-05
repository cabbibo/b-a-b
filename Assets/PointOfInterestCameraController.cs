using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class PointOfInterestCameraController : BaseCameraManager
{

    public Transform pointOfInterest;

    public float distanceFromBird = 10;
    public float requestSpeed;


    // Update is called once per frame
    void Update()
    {

        if (pointOfInterest != null)
        {
            if (overallManager.currentPriority != this)
            {
                overallManager.RequestPriority(this, requestSpeed);
            }
        }
        else
        {
            if (overallManager.currentPriority == this)
            {
                overallManager.ReleasePriority(this, requestSpeed);
            }
        }


    }


    public override void WhileInUse()
    {




        if (pointOfInterest != null)
        {

            //            print("Point of Interest: " + pointOfInterest.name);


            Vector3 targetPos = pointOfInterest.position;
            Vector3 birdPos = God.wren.transform.position;

            Vector3 direction = targetPos - birdPos;
            float distance = direction.magnitude;


            transform.position = birdPos - direction.normalized * distanceFromBird;
            transform.LookAt(birdPos);


        }


    }

    public void SetPointOfInterest(Transform newPointOfInterest)
    {
        pointOfInterest = newPointOfInterest;
    }

    public void ClearPointOfInterest()
    {
        pointOfInterest = null;
    }

    public void SetPointOfInterestForTime(Transform newPointOfInterest, float time, float dfb, float fov, float rs)
    {

        print("SETTTING");
        pointOfInterest = newPointOfInterest;
        FOV = fov;
        distanceFromBird = dfb;
        requestSpeed = rs;
        StartCoroutine(SetForTimePeriodCoroutine(time));
    }


    private IEnumerator SetForTimePeriodCoroutine(float time)
    {

        yield return new WaitForSeconds(time);
        ClearPointOfInterest();
    }


}
