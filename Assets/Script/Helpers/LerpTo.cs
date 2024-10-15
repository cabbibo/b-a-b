using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class LerpTo : MonoBehaviour
{

    public Transform target;
    public float lerpSpeed;
    public float slerpSpeed;

    public float resetLerpSpeed;
    public float resetSlerpSpeed;

    public Transform lookTarget;

    public Transform resetTarget;

    public float FOV;

    public float weight;

    // Start is called before the first frame update
    void OnEnable()
    {

        lerpSpeed = resetLerpSpeed;
        slerpSpeed = resetSlerpSpeed;
        resetTarget = God.instance.transform;
        if (God.wren != null)
        {
            resetTarget = God.wren.cameraWork.camTarget;
        }

    }

    public void ResetTargets()
    {
        target = resetTarget;
        lerpSpeed = resetLerpSpeed;
        slerpSpeed = resetSlerpSpeed;

    }

    void Update()
    {
        if (God.wren != null)
        {
            resetTarget = God.wren.cameraWork.camTarget;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
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
            /*if( God.wren != null ){
                target = God.wren.cameraWork.camTarget;
            }*/

        }
    }


    public void OnDisable()
    {
        //        print("disabled");
    }

    public void SetLookTarget(Transform t)
    {
        print("look target set");
        lookTarget = t;
    }

    public void RemoveLookTarget()
    {
        lookTarget = null;
    }


    public float startLookTime;
    public bool wantsToRelease;
    public float releaseTime;

    public void SetLookReleaseTime(float time)
    {

        wantsToRelease = true;
        startLookTime = Time.time;
        releaseTime = time;
    }


}
