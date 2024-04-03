using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteAlways]
public class AllPoser : MonoBehaviour
{

    public Wren wren;

    public Pose output;
    public bool setFromController;
    public bool setFromWren;
    public bool overrideInput;

    public ControllerTest controller;

    public float lerpSpeed;



    [Range(0, 1)] public float leftWingTuck;
    [Range(0, 1)] public float leftWingTurnLeft;
    [Range(0, 1)] public float leftWingTurnRight;
    [Range(0, 1)] public float leftWingTwistUp;
    [Range(0, 1)] public float leftWingTwistDown;
    [Range(0, 1)] public float leftWingStop;

    [Range(0, 1)] public float rightWingTuck;
    [Range(0, 1)] public float rightWingTurnLeft;
    [Range(0, 1)] public float rightWingTurnRight;
    [Range(0, 1)] public float rightWingTwistUp;
    [Range(0, 1)] public float rightWingTwistDown;
    [Range(0, 1)] public float rightWingStop;


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



    private Vector3 wingRot1_L_vel;
    private Vector3 wingRot2_L_vel;
    private Vector3 wingRot3_L_vel;
    private Vector3 wingRot4_L_vel;

    private Vector3 wingRot1_R_vel;
    private Vector3 wingRot2_R_vel;
    private Vector3 wingRot3_R_vel;
    private Vector3 wingRot4_R_vel;

    private Vector3 tailRot_vel;
    private Vector3 hipRot_vel;
    private Vector3 spineRot_vel;
    private Vector3 shoulderRot_vel;
    private Vector3 neckRot_vel;
    private Vector3 headRot_vel;

    private Vector3 tailRot1_vel;
    private Vector3 tailRot2_vel;
    private Vector3 tailRot3_vel;


    private Vector3 wingRot1_L_target;
    private Vector3 wingRot2_L_target;
    private Vector3 wingRot3_L_target;
    private Vector3 wingRot4_L_target;

    private Vector3 wingRot1_R_target;
    private Vector3 wingRot2_R_target;
    private Vector3 wingRot3_R_target;
    private Vector3 wingRot4_R_target;

    private Vector3 tailRot_target;
    private Vector3 hipRot_target;
    private Vector3 spineRot_target;
    private Vector3 shoulderRot_target;
    private Vector3 neckRot_target;
    private Vector3 headRot_target;

    private Vector3 tailRot1_target;
    private Vector3 tailRot2_target;
    private Vector3 tailRot3_target;


    public float towardsTargetForce;
    public float dampening;


    // Start is called before the first frame update
    void OnEnable()
    {

        if (controller == null)
        {
            controller = GameObject.Find("Rewired Input Manager").GetComponent<ControllerTest>();
        }
    }

    // Update is called once per frame
    void Update()
    {



        if (!overrideInput)
        {
            int invertY = 1;
            invertY = wren.physics.invert ? -1 : 1;
            // invertY = 0;
            if (setFromController)
            {
                leftWingTuck = Mathf.Lerp(leftWingTuck, controller.l2, lerpSpeed);
                leftWingTurnLeft = Mathf.Lerp(leftWingTurnLeft, Mathf.Clamp(-controller.left.x, 0, 1), lerpSpeed);
                leftWingTurnRight = Mathf.Lerp(leftWingTurnRight, Mathf.Clamp(controller.left.x, 0, 1), lerpSpeed);
                leftWingTwistUp = Mathf.Lerp(leftWingTwistUp, Mathf.Clamp(-controller.left.y * invertY, 0, 1), lerpSpeed);
                leftWingTwistDown = Mathf.Lerp(leftWingTwistDown, Mathf.Clamp(controller.left.y * invertY, 0, 1), lerpSpeed);
                rightWingTuck = Mathf.Lerp(rightWingTuck, controller.r2, lerpSpeed);
                rightWingTurnLeft = Mathf.Lerp(rightWingTurnLeft, Mathf.Clamp(-controller.right.x, 0, 1), lerpSpeed);
                rightWingTurnRight = Mathf.Lerp(rightWingTurnRight, Mathf.Clamp(controller.right.x, 0, 1), lerpSpeed);
                rightWingTwistUp = Mathf.Lerp(rightWingTwistUp, Mathf.Clamp(-controller.right.y * invertY, 0, 1), lerpSpeed);
                rightWingTwistDown = Mathf.Lerp(rightWingTwistDown, Mathf.Clamp(controller.right.y * invertY, 0, 1), lerpSpeed);
                rightWingStop = Mathf.Lerp(rightWingStop, Mathf.Clamp(controller.r3 ? 1 : 0, 0, 1), lerpSpeed);
                leftWingStop = Mathf.Lerp(leftWingStop, Mathf.Clamp(controller.l3 ? 1 : 0, 0, 1), lerpSpeed);


            }


            if (setFromWren)
            {
                leftWingTuck = Mathf.Lerp(leftWingTuck, wren.input.left2, lerpSpeed);
                leftWingTurnLeft = Mathf.Lerp(leftWingTurnLeft, Mathf.Clamp(-wren.input.leftX, 0, 1), lerpSpeed);
                leftWingTurnRight = Mathf.Lerp(leftWingTurnRight, Mathf.Clamp(wren.input.leftX, 0, 1), lerpSpeed);
                leftWingTwistUp = Mathf.Lerp(leftWingTwistUp, Mathf.Clamp(-wren.input.leftY * invertY, 0, 1), lerpSpeed);
                leftWingTwistDown = Mathf.Lerp(leftWingTwistDown, Mathf.Clamp(wren.input.leftY * invertY, 0, 1), lerpSpeed);
                rightWingTuck = Mathf.Lerp(rightWingTuck, wren.input.right2, lerpSpeed);
                rightWingTurnLeft = Mathf.Lerp(rightWingTurnLeft, Mathf.Clamp(-wren.input.rightX, 0, 1), lerpSpeed);
                rightWingTurnRight = Mathf.Lerp(rightWingTurnRight, Mathf.Clamp(wren.input.rightX, 0, 1), lerpSpeed);
                rightWingTwistUp = Mathf.Lerp(rightWingTwistUp, Mathf.Clamp(-wren.input.rightY * invertY, 0, 1), lerpSpeed);
                rightWingTwistDown = Mathf.Lerp(rightWingTwistDown, Mathf.Clamp(wren.input.rightY * invertY, 0, 1), lerpSpeed);
                rightWingStop = Mathf.Lerp(rightWingStop, Mathf.Clamp(wren.input.right3, 0, 1), lerpSpeed);
                leftWingStop = Mathf.Lerp(leftWingStop, Mathf.Clamp(wren.input.left3, 0, 1), lerpSpeed);

            }
        }



        wingRot1_L_target = p_Base.wingRot1_L;
        wingRot2_L_target = p_Base.wingRot2_L;
        wingRot3_L_target = p_Base.wingRot3_L;
        wingRot4_L_target = p_Base.wingRot4_L;

        wingRot1_R_target = p_Base.wingRot1_R;
        wingRot2_R_target = p_Base.wingRot2_R;
        wingRot3_R_target = p_Base.wingRot3_R;
        wingRot4_R_target = p_Base.wingRot4_R;

        tailRot_target = p_Base.tailRot;
        hipRot_target = p_Base.hipRot;
        spineRot_target = p_Base.spineRot;
        shoulderRot_target = p_Base.shoulderRot;
        neckRot_target = p_Base.neckRot;
        headRot_target = p_Base.headRot;

        tailRot1_target = p_Base.tailRot1;
        tailRot2_target = p_Base.tailRot2;
        tailRot3_target = p_Base.tailRot3;




        wingRot1_L = Vector3.zero;
        wingRot2_L = Vector3.zero;
        wingRot3_L = Vector3.zero;
        wingRot4_L = Vector3.zero;
        wingRot1_R = Vector3.zero;
        wingRot2_R = Vector3.zero;
        wingRot3_R = Vector3.zero;
        wingRot4_R = Vector3.zero;
        tailRot = Vector3.zero;
        hipRot = Vector3.zero;
        spineRot = Vector3.zero;
        shoulderRot = Vector3.zero;
        neckRot = Vector3.zero;
        headRot = Vector3.zero;
        tailRot1 = Vector3.zero;
        tailRot2 = Vector3.zero;
        tailRot3 = Vector3.zero;

        AddPose(leftWingTuck, p_leftWingTuck);
        AddPose(leftWingTurnLeft, p_leftWingTurnLeft);
        AddPose(leftWingTurnRight, p_leftWingTurnRight);
        AddPose(leftWingTwistUp, p_leftWingTwistUp);
        AddPose(leftWingTwistDown, p_leftWingTwistDown);
        AddPose(leftWingStop, p_leftWingStop);

        AddPose(rightWingTuck, p_rightWingTuck);
        AddPose(rightWingTurnLeft, p_rightWingTurnLeft);
        AddPose(rightWingTurnRight, p_rightWingTurnRight);
        AddPose(rightWingTwistUp, p_rightWingTwistUp);
        AddPose(rightWingTwistDown, p_rightWingTwistDown);


        AddPose(rightWingStop, p_rightWingStop);


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

        wingRot1_L_target += wingRot1_L;
        wingRot2_L_target += wingRot2_L;
        wingRot3_L_target += wingRot3_L;
        wingRot4_L_target += wingRot4_L;
        wingRot1_R_target += wingRot1_R;
        wingRot2_R_target += wingRot2_R;
        wingRot3_R_target += wingRot3_R;
        wingRot4_R_target += wingRot4_R;
        tailRot_target += tailRot;
        hipRot_target += hipRot;
        spineRot_target += spineRot;
        shoulderRot_target += shoulderRot;
        neckRot_target += neckRot;
        headRot_target += headRot;
        tailRot1_target += tailRot1;
        tailRot2_target += tailRot2;
        tailRot3_target += tailRot3;


        wingRot1_L_vel += (wingRot1_L_target - output.wingRot1_L) * towardsTargetForce;
        output.wingRot1_L += wingRot1_L_vel;
        wingRot1_L_vel *= dampening;

        wingRot2_L_vel += (wingRot2_L_target - output.wingRot2_L) * towardsTargetForce;
        output.wingRot2_L += wingRot2_L_vel;
        wingRot2_L_vel *= dampening;

        wingRot3_L_vel += (wingRot3_L_target - output.wingRot3_L) * towardsTargetForce;
        output.wingRot3_L += wingRot3_L_vel;
        wingRot3_L_vel *= dampening;

        wingRot4_L_vel += (wingRot4_L_target - output.wingRot4_L) * towardsTargetForce;
        output.wingRot4_L += wingRot4_L_vel;
        wingRot4_L_vel *= dampening;

        wingRot1_R_vel += (wingRot1_R_target - output.wingRot1_R) * towardsTargetForce;
        output.wingRot1_R += wingRot1_R_vel;
        wingRot1_R_vel *= dampening;

        wingRot2_R_vel += (wingRot2_R_target - output.wingRot2_R) * towardsTargetForce;
        output.wingRot2_R += wingRot2_R_vel;
        wingRot2_R_vel *= dampening;

        wingRot3_R_vel += (wingRot3_R_target - output.wingRot3_R) * towardsTargetForce;
        output.wingRot3_R += wingRot3_R_vel;
        wingRot3_R_vel *= dampening;

        wingRot4_R_vel += (wingRot4_R_target - output.wingRot4_R) * towardsTargetForce;
        output.wingRot4_R += wingRot4_R_vel;
        wingRot4_R_vel *= dampening;

        tailRot_vel += (tailRot_target - output.tailRot) * towardsTargetForce;
        output.tailRot += tailRot_vel;
        tailRot_vel *= dampening;

        hipRot_vel += (hipRot_target - output.hipRot) * towardsTargetForce;
        output.hipRot += hipRot_vel;
        hipRot_vel *= dampening;

        spineRot_vel += (spineRot_target - output.spineRot) * towardsTargetForce;
        output.spineRot += spineRot_vel;
        spineRot_vel *= dampening;

        shoulderRot_vel += (shoulderRot_target - output.shoulderRot) * towardsTargetForce;
        output.shoulderRot += shoulderRot_vel;
        shoulderRot_vel *= dampening;

        neckRot_vel += (neckRot_target - output.neckRot) * towardsTargetForce;
        output.neckRot += neckRot_vel;
        neckRot_vel *= dampening;

        headRot_vel += (headRot_target - output.headRot) * towardsTargetForce;
        output.headRot += headRot_vel;
        headRot_vel *= dampening;

        tailRot1_vel += (tailRot1_target - output.tailRot1) * towardsTargetForce;
        output.tailRot1 += tailRot1_vel;
        tailRot1_vel *= dampening;

        tailRot2_vel += (tailRot2_target - output.tailRot2) * towardsTargetForce;
        output.tailRot2 += tailRot2_vel;
        tailRot2_vel *= dampening;

        tailRot3_vel += (tailRot3_target - output.tailRot3) * towardsTargetForce;
        output.tailRot3 += tailRot3_vel;
        tailRot3_vel *= dampening;





    }


    void AddPose(float strength, Pose pose)
    {

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


    public ComputeBuffer _TransformBuffer;

    //
    public int totalBones = 16;

    public void Debug()
    {

        if (_TransformBuffer == null)
        {
            _TransformBuffer = new ComputeBuffer(1, 12 * 4);
        }


    }



}
