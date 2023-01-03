using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AllPoser : MonoBehaviour
{

    public Wren wren;

    public Pose output;
    public bool setFromController;
    public bool setFromWren;
    public ControllerTest controller;

    public float lerpSpeed;
    
    [Range(0,1)] public float leftWingTuck;
    [Range(0,1)] public float leftWingTurnLeft;
    [Range(0,1)] public float leftWingTurnRight;
    [Range(0,1)] public float leftWingTwistUp;
    [Range(0,1)] public float leftWingTwistDown;
    [Range(0,1)] public float leftWingStop;

    [Range(0,1)] public float rightWingTuck;
    [Range(0,1)] public float rightWingTurnLeft;
    [Range(0,1)] public float rightWingTurnRight;
    [Range(0,1)] public float rightWingTwistUp;
    [Range(0,1)] public float rightWingTwistDown;
    [Range(0,1)] public float rightWingStop;

    public Pose p_Base;


    public Pose p_leftWingTuck;
    public Pose p_leftWingTurnLeft;
    public Pose p_leftWingTurnRight;
    public Pose p_leftWingTwistUp;
    public Pose p_leftWingTwistDown;
    public Pose p_leftWingStop;


    public Pose p_rightWingTuck;
    public Pose p_rightWingTurnLeft;
    public Pose p_rightWingTurnRight;
    public Pose p_rightWingTwistUp;
    public Pose p_rightWingTwistDown;
    public Pose p_rightWingStop;

   private Vector3 wingRot1_L;
   private Vector3 wingRot2_L;
   private Vector3 wingRot3_L;
   private Vector3 wingRot4_L;

   private Vector3 wingRot1_R;
   private Vector3 wingRot2_R;
   private Vector3 wingRot3_R;
   private Vector3 wingRot4_R;

   private Vector3 tailRot;
   private Vector3 hipRot;
   private Vector3 spineRot;
   private Vector3 shoulderRot;
   private Vector3 neckRot;
   private Vector3 headRot;

   private Vector3 tailRot1;
   private Vector3 tailRot2;
   private Vector3 tailRot3;

    // Start is called before the first frame update
    void OnEnable()
    {

        if( controller == null ){
            controller = GameObject.Find("Rewired Input Manager").GetComponent<ControllerTest>();
        }        
    }

    // Update is called once per frame
    void Update()
    {


if( setFromController ){
leftWingTuck        = Mathf.Lerp(leftWingTuck, controller.l2 , lerpSpeed);
leftWingTurnLeft    = Mathf.Lerp(leftWingTurnLeft, Mathf.Clamp( -controller.left.x ,0,1) , lerpSpeed);
leftWingTurnRight   = Mathf.Lerp(leftWingTurnRight, Mathf.Clamp( controller.left.x ,0,1) , lerpSpeed);
leftWingTwistUp     = Mathf.Lerp(leftWingTwistUp, Mathf.Clamp( -controller.left.y ,0,1) , lerpSpeed);
leftWingTwistDown   = Mathf.Lerp(leftWingTwistDown, Mathf.Clamp(controller.left.y ,0,1) , lerpSpeed);
rightWingTuck       = Mathf.Lerp(rightWingTuck, controller.r2 , lerpSpeed);
rightWingTurnLeft   = Mathf.Lerp(rightWingTurnLeft, Mathf.Clamp( -controller.right.x ,0,1) , lerpSpeed);
rightWingTurnRight  = Mathf.Lerp(rightWingTurnRight, Mathf.Clamp( controller.right.x ,0,1) , lerpSpeed);
rightWingTwistUp    = Mathf.Lerp(rightWingTwistUp, Mathf.Clamp( -controller.right.y ,0,1) , lerpSpeed);
rightWingTwistDown  = Mathf.Lerp(rightWingTwistDown, Mathf.Clamp( controller.right.y ,0,1) , lerpSpeed);
rightWingStop       = Mathf.Lerp(rightWingStop, Mathf.Clamp( controller.r3 ? 1:0 ,0,1) , lerpSpeed);
leftWingStop        = Mathf.Lerp(leftWingStop, Mathf.Clamp( controller.l3 ? 1:0 ,0,1) , lerpSpeed);


}


if( setFromWren ){
leftWingTuck        = Mathf.Lerp(leftWingTuck, wren.input.left2 , lerpSpeed);
leftWingTurnLeft    = Mathf.Lerp(leftWingTurnLeft, Mathf.Clamp( -wren.input.leftX ,0,1) , lerpSpeed);
leftWingTurnRight   = Mathf.Lerp(leftWingTurnRight, Mathf.Clamp( wren.input.leftX ,0,1) , lerpSpeed);
leftWingTwistUp     = Mathf.Lerp(leftWingTwistUp, Mathf.Clamp( -wren.input.leftY ,0,1) , lerpSpeed);
leftWingTwistDown   = Mathf.Lerp(leftWingTwistDown, Mathf.Clamp( wren.input.leftY ,0,1) , lerpSpeed);
rightWingTuck       = Mathf.Lerp(rightWingTuck, wren.input.right2 , lerpSpeed);
rightWingTurnLeft   = Mathf.Lerp(rightWingTurnLeft, Mathf.Clamp( -wren.input.rightX ,0,1) , lerpSpeed);
rightWingTurnRight  = Mathf.Lerp(rightWingTurnRight, Mathf.Clamp( wren.input.rightX ,0,1) , lerpSpeed);
rightWingTwistUp    = Mathf.Lerp(rightWingTwistUp, Mathf.Clamp( -wren.input.rightY ,0,1) , lerpSpeed);
rightWingTwistDown  = Mathf.Lerp(rightWingTwistDown, Mathf.Clamp( wren.input.rightY ,0,1) , lerpSpeed);
rightWingStop       = Mathf.Lerp(rightWingStop, Mathf.Clamp( wren.input.right3 ,0,1) , lerpSpeed);
leftWingStop        = Mathf.Lerp(leftWingStop, Mathf.Clamp( wren.input.left3 ,0,1) , lerpSpeed);

}





    output.wingRot1_L = p_Base.wingRot1_L;
    output.wingRot2_L = p_Base.wingRot2_L;
    output.wingRot3_L = p_Base.wingRot3_L;
    output.wingRot4_L = p_Base.wingRot4_L;

    output.wingRot1_R = p_Base.wingRot1_R;
    output.wingRot2_R = p_Base.wingRot2_R;
    output.wingRot3_R = p_Base.wingRot3_R;
    output.wingRot4_R = p_Base.wingRot4_R;

    output.tailRot = p_Base.tailRot;
    output.hipRot = p_Base.hipRot;
    output.spineRot = p_Base.spineRot;
    output.shoulderRot = p_Base.shoulderRot;
    output.neckRot = p_Base.neckRot;
    output.headRot = p_Base.headRot;

    output.tailRot1 = p_Base.tailRot1;
    output.tailRot2 = p_Base.tailRot2;
    output.tailRot3 = p_Base.tailRot3;

wingRot1_L= Vector3.zero;
wingRot2_L= Vector3.zero;
wingRot3_L= Vector3.zero;
wingRot4_L= Vector3.zero;
wingRot1_R= Vector3.zero;
wingRot2_R= Vector3.zero;
wingRot3_R= Vector3.zero;
wingRot4_R= Vector3.zero;
tailRot= Vector3.zero;
hipRot= Vector3.zero;
spineRot= Vector3.zero;
shoulderRot= Vector3.zero;
neckRot= Vector3.zero;
headRot= Vector3.zero;
tailRot1= Vector3.zero;
tailRot2= Vector3.zero;
tailRot3= Vector3.zero;

    AddPose( leftWingTuck , p_leftWingTuck);
    AddPose( leftWingTurnLeft , p_leftWingTurnLeft);
    AddPose( leftWingTurnRight , p_leftWingTurnRight);
    AddPose( leftWingTwistUp , p_leftWingTwistUp);
    AddPose( leftWingTwistDown , p_leftWingTwistDown);
    AddPose( leftWingStop , p_leftWingStop);

    AddPose( rightWingTuck , p_rightWingTuck);
    AddPose( rightWingTurnLeft , p_rightWingTurnLeft);
    AddPose( rightWingTurnRight , p_rightWingTurnRight);
    AddPose( rightWingTwistUp , p_rightWingTwistUp);
    AddPose( rightWingTwistDown , p_rightWingTwistDown);
    
   
    AddPose( rightWingStop , p_rightWingStop);


/*
wingRot1_L /= 10;
wingRot2_L /= 10;
wingRot3_L /= 10;
wingRot4_L /= 10;
wingRot1_R /= 10;
wingRot2_R /= 10;
wingRot3_R /= 10;
wingRot4_R /= 10;
tailRot /= 10;
hipRot /= 10;
spineRot /= 10;
shoulderRot /= 10;
neckRot /= 10;
headRot /= 10;
tailRot1 /= 10;
tailRot2 /= 10;
tailRot3 /= 10;
*/

output.wingRot1_L += wingRot1_L;
output.wingRot2_L += wingRot2_L;
output.wingRot3_L += wingRot3_L;
output.wingRot4_L += wingRot4_L;
output.wingRot1_R += wingRot1_R;
output.wingRot2_R += wingRot2_R;
output.wingRot3_R += wingRot3_R;
output.wingRot4_R += wingRot4_R;
output.tailRot += tailRot;
output.hipRot += hipRot;
output.spineRot += spineRot;
output.shoulderRot += shoulderRot;
output.neckRot += neckRot;
output.headRot += headRot;
output.tailRot1 += tailRot1;
output.tailRot2 += tailRot2;
output.tailRot3 += tailRot3;
        
    }


    void AddPose( float strength , Pose pose ){

    wingRot1_L += strength * (pose.wingRot1_L - p_Base.wingRot1_L);
    wingRot2_L += strength * (pose.wingRot2_L - p_Base.wingRot2_L);
    wingRot3_L += strength * (pose.wingRot3_L - p_Base.wingRot3_L);
    wingRot4_L += strength * (pose.wingRot4_L - p_Base.wingRot4_L);

    wingRot1_R += strength * (pose.wingRot1_R - p_Base.wingRot1_R);
    wingRot2_R += strength * (pose.wingRot2_R - p_Base.wingRot2_R);
    wingRot3_R += strength * (pose.wingRot3_R - p_Base.wingRot3_R);
    wingRot4_R += strength * (pose.wingRot4_R - p_Base.wingRot4_R);

    tailRot += strength * (pose.tailRot - p_Base.tailRot);
    hipRot += strength * (pose.hipRot - p_Base.hipRot);
    spineRot += strength * (pose.spineRot - p_Base.spineRot);
    shoulderRot += strength * (pose.shoulderRot - p_Base.shoulderRot);
    neckRot += strength * (pose.neckRot - p_Base.neckRot);
    headRot += strength * (pose.headRot - p_Base.headRot);

    tailRot1 += strength * (pose.tailRot1 - p_Base.tailRot1);
    tailRot2 += strength * (pose.tailRot2 - p_Base.tailRot2);
    tailRot3 += strength * (pose.tailRot3 - p_Base.tailRot3);
        

    }



}
