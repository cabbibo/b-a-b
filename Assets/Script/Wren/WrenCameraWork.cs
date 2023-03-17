using System.Collections;
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
public float lookAtTargetLookSpeedGround = 1 ;

Vector3 fLookTarget = new Vector3();
public void Reset(){
    fLookTarget = transform.position;
    Camera.main.transform.position = transform.position - transform.forward * 10;
    Camera.main.transform.LookAt( transform.position );//SetLookRotation(transform.forward);
}

float oLook;
public void CameraWork(){


  float fov;
  Vector3 lookTarget;

  /*

  Here we are trying to create a dead zone 
  so there isn't gimbal lock when we look down
  I don't get it exactly cuz Dan wrote it :p

*/
oMatchWithVert = matchWithVert;
matchWithVert= Mathf.Abs(Vector3.Dot( transform.forward , Vector3.up ));
if( matchWithVert > deadZoneCutoff  && oMatchWithVert <= deadZoneCutoff ){
  EnterDeadZone();
}


if( matchWithVert <= deadZoneCutoff  && oMatchWithVert > deadZoneCutoff ){
  ExitDeadZone();
}

matchWithVert = Mathf.Abs(Vector3.Dot( transform.forward , Vector3.up ));

Vector3 upVal = Vector3.up;
if( inDeadZone ) upVal = deadZoneUp;


  if( wren.physics.onGround ){

        fov = groundFOV; 
    if( objectTargeted == null ){
        
        camTarget.position =  transform.position +  transform.up * groundUpAmount  * wren._ScaleMultiplier - transform.forward * groundBackAmount * wren._ScaleMultiplier;

        lookTarget = transform.position;
        // Check to make sure we aren't in the terrain
        

        // try and look ahead

        lookTarget += wren.physics.vel * forwardLookVal * wren._ScaleMultiplier;


        fLookTarget = Vector3.Lerp( fLookTarget , lookTarget , .1f);
        // puts our final look target in front of wren based on head transform 
        camTarget.LookAt(  lookTarget ,upVal);

        
    }else{



      lookTarget = objectTargeted.TransformPoint(objectTargetedPosition);
      fLookTarget = Vector3.Lerp( fLookTarget , lookTarget , lookAtTargetLookSpeedGround );
      Vector3 p = transform.position ;
      Vector3 dir = fLookTarget - p;

      camTarget.transform.position  = p - dir.normalized * wren._ScaleMultiplier * backAmount - dir.normalized * wren.physics.vel.magnitude * forwardLookVal * wren._ScaleMultiplier;// +  upVal * upAmount;
      camTarget.LookAt( fLookTarget , upVal );
    
    }



  }else{
    /*
    Tweens out FOV
    if we are going fast enough
    */
    fov =  Mathf.Clamp( wren.physics.vel.magnitude * .7f,slowFOV, fastFOV);
    if( objectTargeted == null ){
    


      float lookEulers = wrenHead.localRotation.eulerAngles.y;

      if( lookEulers > 180 ){ lookEulers = lookEulers - 360; }



      oLook = Mathf.Lerp(oLook, lookEulers , .04f);


        camTarget.position =  transform.position +  transform.up * upAmount - transform.forward * wren._ScaleMultiplier * backAmount - transform.right * lookEulers  * leftAmount * wren._ScaleMultiplier;
        //lookTarget =  camTarget.position + Vector3.Lerp( camTarget.forward, wrenHead.forward.normalized , lerpTowardHeadLook );
      
      
      lookTarget = transform.position;//+  Vector3.Lerp( camTarget.forward, wrenHead.forward.normalized , lerpTowardHeadLook ) * headLookForwardAmount;
    // Check to make sure we aren't in the terrain
      lookTarget += wren.physics.vel * forwardLookVal * wren._ScaleMultiplier;


      // try and look ahead


      fLookTarget = Vector3.Lerp( fLookTarget , lookTarget , .1f);
      // puts our final look target in front of wren based on head transform 
      camTarget.LookAt(  lookTarget ,upVal);

    
    
    }else{

      lookTarget = objectTargeted.TransformPoint(objectTargetedPosition);
      fLookTarget = Vector3.Lerp( fLookTarget , lookTarget , lookAtTargetLookSpeedAir );
      Vector3 p = transform.position ;
      Vector3 dir = fLookTarget - p;

      camTarget.transform.position  = p - dir.normalized * wren._ScaleMultiplier * backAmount - dir.normalized * wren.physics.vel.magnitude * forwardLookVal * wren._ScaleMultiplier;// +  upVal * upAmount;
      camTarget.LookAt( fLookTarget , upVal );
    
    }
    
  
  }

 
  // Makes any change of FOV faster
  God.camera.fieldOfView = Mathf.Lerp( God.camera.fieldOfView , fov , .1f);





  

 






}

public Vector3 deadZoneUp;
public bool inDeadZone;

void EnterDeadZone(){

  inDeadZone = true;
  deadZoneUp = camTarget.transform.up;

}

void ExitDeadZone(){
inDeadZone = false;
}



}