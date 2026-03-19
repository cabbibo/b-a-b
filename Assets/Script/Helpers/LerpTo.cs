using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


public class LerpTo : BaseCameraManager
{
    public Transform target;
    public float     lerpSpeed;
    public float     slerpSpeed;

    public float resetLerpSpeed;
    public float resetSlerpSpeed;

    public Transform lookTarget;

    public Transform resetTarget;

    public float wrenVelocityOffseter = .02f;

    // Start is called before the first frame update
    private void OnEnable()
    {
#if UNITY_EDITOR
        if ( !Application.isPlaying || UnityEditor.BuildPipeline.isBuildingPlayer ) {
            return;
        }
#endif
        lerpSpeed = resetLerpSpeed;
        slerpSpeed = resetSlerpSpeed;
        resetTarget = God.instance.transform;

        if ( God.wren != null ) {
            resetTarget = God.wren.cameraWork.camTarget;
        }

    }

    public void ResetTargets()
    {
        target = resetTarget;
        lerpSpeed = resetLerpSpeed;
        slerpSpeed = resetSlerpSpeed;

    }

    private void Update()
    {
        if ( God.wren != null ) {
            resetTarget = God.wren.cameraWork.camTarget;
        }
    }

    public override void WhileInUse()
    {


        if ( God.wren != null ) {
            FOV = God.wren.cameraWork.FOV;
            resetTarget = God.wren.cameraWork.camTarget;
        } else {
            resetTarget = God.instance.transform;
            FOV = 60;
        }


        if ( wantsToRelease && Time.time - startLookTime > releaseTime ) {
            lookTarget = null;

        }

        if ( target != null ) {


            transform.position = Vector3.Lerp( transform.position , target.position , lerpSpeed );

            // do camera offset


            if ( God.wren != null ) {
                var localVelocity = transform.InverseTransformDirection( God.wren.physics.vel );
                localVelocity = Vector3.Scale( localVelocity , new Vector3( 1 , 1 , 0 ) );
                localVelocity = transform.TransformDirection( localVelocity );


                transform.position += localVelocity * wrenVelocityOffseter;
            }

            if ( lookTarget != null ) {

                transform.rotation = Quaternion.Slerp( transform.rotation ,
                    Quaternion.LookRotation( lookTarget.position - transform.position , Vector3.up ) , slerpSpeed );
            } else {
                transform.rotation = Quaternion.Slerp( transform.rotation , target.rotation , slerpSpeed );
            }
        } else {
            // gives us a target if we dont have one!
            /*if( God.wren != null ){
                target = God.wren.cameraWork.camTarget;
            }*/

        }
    }

    // Update is called once per frame
    /*void FixedUpdate()
    {




        if (God.wren != null)
        {
            FOV = God.wren.cameraWork.FOV;
            resetTarget = God.wren.cameraWork.camTarget;
        }
        else
        {
            resetTarget = God.instance.transform;
            FOV = 60;
        }



        if (wantsToRelease && Time.time - startLookTime > releaseTime)
        {
            lookTarget = null;

        }
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, lerpSpeed);

            if (lookTarget != null)
            {

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget.position - transform.position, Vector3.up), slerpSpeed);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, slerpSpeed);
            }
        }
        else
        {
            // gives us a target if we dont have one!


        }
    }*/


    public void OnDisable()
    {
        //        print("disabled");
    }

    public void SetLookTarget( Transform t )
    {
        print( "look target set" );
        lookTarget = t;
    }

    public void RemoveLookTarget()
    {
        lookTarget = null;
    }


    public float startLookTime;
    public bool  wantsToRelease;
    public float releaseTime;

    public void SetLookReleaseTime( float time )
    {

        wantsToRelease = true;
        startLookTime = Time.time;
        releaseTime = time;
    }
}