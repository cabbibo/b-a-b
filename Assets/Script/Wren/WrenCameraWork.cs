﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Normal.Realtime;
using UnityEngine.UI;
using WrenUtils;


public class WrenCameraWork : MonoBehaviour
{



  public Wren wren;


  public Transform camTarget;

  public Transform wrenHead;

  public Transform objectTargeted;
  public Vector3 objectTargetedPosition;


  public float slowFOV;
  public float fastFOV;
  public float groundFOV;
  public float headLookForwardAmount;
  public float headLookLerpSpeed;

  public Vector3 lookTarget;


  public float backAmount;
  public float upAmount;

  public float leftAmount;

  public float groundUpAmount;
  public float groundBackAmount;

  public float oMatchWithVert;
  public float matchWithVert;


  public float lerpTowardHeadLook;

  public float forwardLookVal;

  public float deadZoneCutoff = .8f;

  public float lookAtTargetLookSpeedAir = 1;
  public float lookAtTargetLookSpeedGround = 1;


  public float lookUpAmount;

  Vector3 fLookTarget = new Vector3();
  public void Reset()
  {
    fLookTarget = transform.position;
    Camera.main.transform.position = transform.position - transform.forward * 10;
    Camera.main.transform.LookAt(transform.position);//SetLookRotation(transform.forward);
  }


  // Jumping the camera to a different position
  public void Offset(Vector3 v)
  {


    //fLookTarget = transform.position;
    lookTarget += v;
    fLookTarget += v;

    //transform.position += v;
    camTarget.position += v;
    Camera.main.transform.position += v;
    CameraWork();


  }

  public void Offset(Transform startingTransform, Transform endingTransform)
  {

    Vector3 localPos = startingTransform.InverseTransformPoint(camTarget.position);
    Vector3 localForward = startingTransform.InverseTransformDirection(camTarget.forward);
    Vector3 localUp = startingTransform.InverseTransformDirection(camTarget.up);
    Vector3 localRight = startingTransform.InverseTransformDirection(camTarget.right);

    Vector3 endPos = endingTransform.TransformPoint(localPos);
    Vector3 endForward = endingTransform.TransformDirection(localForward);
    Vector3 endUp = endingTransform.TransformDirection(localUp);
    Vector3 endRight = endingTransform.TransformDirection(localRight);


    Vector3 delta = endingTransform.position - startingTransform.position;


    camTarget.position = endPos;
    Camera.main.transform.position = endPos;

    transform.forward = endForward;
    transform.up = endUp;
    transform.right = endRight;

    transform.position = endingTransform.position;

    lookTarget = endingTransform.position;
    SnapLookTarget();


    CameraWork();
    //SnapLookTarget();
    //CameraWork();

  }


  public void RotateOffset(Quaternion q)
  {
    Quaternion delta = q * Quaternion.Inverse(transform.rotation);
    // Vector3


    transform.rotation = q;
    CameraWork();
  }



  public void Offset(Transform t)
  {

  }



  public Vector3 tmpUp;
  public Vector3 tmpForward;
  public Vector3 tmpRight;
  public Vector3 wrenTmpUp;


  float oLook;
  public void CameraWork()
  {


    float fov;
    Vector3 lookTarget;

    /*

    Here we are trying to create a dead zone 
    so there isn't gimbal lock when we look down
    I don't get it exactly cuz Dan wrote it :p

  */
    oMatchWithVert = matchWithVert;
    matchWithVert = Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up));
    if (matchWithVert > deadZoneCutoff && oMatchWithVert <= deadZoneCutoff)
    {
      EnterDeadZone();
    }


    if (matchWithVert <= deadZoneCutoff && oMatchWithVert > deadZoneCutoff)
    {
      ExitDeadZone();
    }

    matchWithVert = Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up));

    Vector3 upVal = Vector3.up;
    if (inDeadZone) upVal = deadZoneUp;

    tmpUp = Vector3.Lerp(tmpUp, transform.up, .1f);
    tmpRight = Vector3.Lerp(tmpRight, transform.right, .1f);
    tmpForward = Vector3.Lerp(tmpForward, transform.forward, .1f);
    wrenTmpUp = Vector3.Lerp(wrenTmpUp, wren.transform.up, .1f);

    if (wren.physics.onGround)
    {

      fov = groundFOV;
      if (objectTargeted == null)
      {

        camTarget.position = transform.position + transform.up * groundUpAmount * wren._ScaleMultiplier - transform.forward * groundBackAmount * wren._ScaleMultiplier;

        lookTarget = transform.position;
        // Check to make sure we aren't in the terrain


        // try and look ahead

        // lookTarget += wren.physics.vel * forwardLookVal * wren._ScaleMultiplier;


        fLookTarget = Vector3.Lerp(fLookTarget, lookTarget, .1f);
        // puts our final look target in front of wren based on head transform 
        camTarget.LookAt(lookTarget, upVal);


      }
      else
      {



        lookTarget = objectTargeted.TransformPoint(objectTargetedPosition);
        fLookTarget = Vector3.Lerp(fLookTarget, lookTarget, lookAtTargetLookSpeedGround);
        Vector3 p = transform.position;
        Vector3 dir = fLookTarget - p;

        camTarget.transform.position = p - dir.normalized * wren._ScaleMultiplier * backAmount - dir.normalized * wren.physics.vel.magnitude * forwardLookVal * wren._ScaleMultiplier;// +  upVal * upAmount;
        camTarget.LookAt(fLookTarget, upVal);

      }



    }
    else
    {




      /*
      Tweens out FOV
      if we are going fast enough
      */
      fov = Mathf.Clamp(wren.physics.vel.magnitude * .7f, slowFOV, fastFOV);
      if (objectTargeted == null)
      {





        float lookEulers = wrenHead.localRotation.eulerAngles.y;

        if (lookEulers > 180) { lookEulers = lookEulers - 360; }



        oLook = Mathf.Lerp(oLook, lookEulers, .04f);
        //        print(oLook);


        camTarget.position = transform.position + tmpUp * upAmount - tmpForward * wren._ScaleMultiplier * backAmount - tmpRight * lookEulers * leftAmount * wren._ScaleMultiplier;
        //lookTarget =  camTarget.position + Vector3.Lerp( camTarget.forward, wrenHead.forward.normalized , lerpTowardHeadLook );


        Vector3 lookForwardDirection = Vector3.Lerp(camTarget.forward, wrenHead.forward.normalized, lerpTowardHeadLook);
        lookTarget = transform.position;//+  Vector3.Lerp( camTarget.forward, wrenHead.forward.normalized , lerpTowardHeadLook ) * headLookForwardAmount;
                                        // Check to make sure we aren't in the terrain


        lookTarget += lookForwardDirection * forwardLookVal * wren._ScaleMultiplier;


        // try and look ahead

        lookTarget += wrenTmpUp * lookUpAmount;


        fLookTarget = Vector3.Lerp(fLookTarget, lookTarget, .1f);
        // puts our final look target in front of wren based on head transform 
        camTarget.LookAt(lookTarget, upVal);



      }
      else
      {

        lookTarget = objectTargeted.TransformPoint(objectTargetedPosition);
        fLookTarget = Vector3.Lerp(fLookTarget, lookTarget, lookAtTargetLookSpeedAir);
        Vector3 p = transform.position;
        Vector3 dir = fLookTarget - p;

        camTarget.transform.position = p - dir.normalized * wren._ScaleMultiplier * backAmount - dir.normalized * wren.physics.vel.magnitude * forwardLookVal * wren._ScaleMultiplier;// +  upVal * upAmount;
        camTarget.LookAt(fLookTarget, upVal);

      }


    }


    // Makes any change of FOV faster
    God.camera.fieldOfView = Mathf.Lerp(God.camera.fieldOfView, fov, .1f);














  }

  public Vector3 deadZoneUp;
  public bool inDeadZone;

  void EnterDeadZone()
  {

    inDeadZone = true;
    deadZoneUp = camTarget.transform.up;

  }

  void ExitDeadZone()
  {
    inDeadZone = false;
  }


  public void SnapLookTarget()
  {
    fLookTarget = lookTarget;
  }


}