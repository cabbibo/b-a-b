using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class PointOfInterestCameraController : BaseCameraManager
{
    public Transform pointOfInterest;

    public float distanceFromBird = 10;
    public float requestSpeed;
    public bool  lockToBird;

    // Update is called once per frame
    private void Update()
    {

        if ( pointOfInterest != null ) {
            if ( overallManager.currentPriority != this ) {
                print( "REQUESTTTING" );
                overallManager.RequestPriority( this , requestSpeed );
            }
        } else {
            if ( overallManager.currentPriority == this ) {

                print( "RELEASIGNs" );
                overallManager.ReleasePriority( this , requestSpeed );
            }
        }


    }


    public override void WhileInUse()
    {


        if ( pointOfInterest != null ) {

            //            print("Point of Interest: " + pointOfInterest.name);

            var targetPos = pointOfInterest.position;
            var birdPos = God.wren.transform.position;

            var direction = targetPos - birdPos;
            float distance = direction.magnitude;

            if ( lockToBird ) {

                transform.position = birdPos - direction.normalized * distanceFromBird;
                transform.LookAt( birdPos );
            } else {
                transform.position = targetPos - direction.normalized * distanceFromBird;
                transform.LookAt( targetPos );

            }


        }
    }

    public void SetPointOfInterest( Transform newPointOfInterest , float rs = .1f , bool ltb = true )
    {
        pointOfInterest = newPointOfInterest;
        requestSpeed = rs;
        lockToBird = ltb;
    }

    public void ClearPointOfInterest()
    {
        print( "clearing point of interest" );
        pointOfInterest = null;
    }

    public void SetPointOfInterestForTime( Transform newPointOfInterest , float time , float dfb , float fov , float rs ,
        bool ltb = true )
    {

        print( "SETTTING it all!" );
        print( newPointOfInterest );
        pointOfInterest = newPointOfInterest;
        FOV = fov;
        distanceFromBird = dfb;
        requestSpeed = rs;

        lockToBird = ltb;


        StartCoroutine( SetForTimePeriodCoroutine( time ) );
    }


    private IEnumerator SetForTimePeriodCoroutine( float time )
    {

        yield return new WaitForSeconds( time );
        ClearPointOfInterest();
    }
}