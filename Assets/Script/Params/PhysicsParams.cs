using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( fileName = "PhysicsParams" , menuName = "WREN/PhysicsParams" , order = 1 )]
public class PhysicsParams : ScriptableObject
{
    public string name;

    public bool swapLR;
    public bool invert;
    public bool lockX;
    public bool lockY;

    public float reduceFlapOnStaminaStart;
    public float reduceFlapOnStaminaMax;

    public float maxAngleForY;
    public float maxAngleForYMax;
    public float maxAngleForYMaxReduction;

    public float gravityForce;

    public float slowestTwistAngle;
    public float fastestTwistAngle;

    public float slowestBendAngle;
    public float fastestBendAngle;

    public float twistLerpSpeed;
    public float bendLerpSpeed;

    public float slowestAmountToSide;
    public float fastestAmountToSide;

    public float twistForceVal;


    public float allFeathersMaxSpeed;
    public float noFeathersMaxSpeed;
    public float maxSpeed;

    public float maxSpeedDamper;

    public float baseSpeed;
    public float baseSpeedDamper;


    public float closeForwardBoostVal;
    public float thrustForceMultiplier;

    public float strafeVal;
    public float straightLiftForce;

    public float velMatchMultiplier;


    public float tuckAddToGravityVal;

    public float tuckReduceLiftVal;

    public float tuckedAngularDrag;
    public float untuckedAngularDrag;


    public float tuckedDrag;
    public float untuckedDrag;

    public float tuckLerpSpeed;

    public float forwardExtraBoostOnTuck;
    public float tuckDampeningReduction;

    public float tuckReduceUpdraftVal;


    public float horizonRightingForceVal;
    public float rightingForce;
    public float rightingDependentOnNotTouchingVal;


    public float maxUpAngle;
    public float maxUpAngleForceRightingMultiplier;


    public float pushingBackThrustForceCorrector;


    public float closestHeight;
    public float furthestHeight;
    public float closestForce;
    public float furthestForce;
    public float groundForceTweenVal;
    public float windAmountToTheSide;


    public float groundPower;
    public float groundOut;
    public float groundDampening;

    public float rotateTowardsTargetOnGround;


    public float groundUpForce;
    public float groundUpVal;


    public float flapToSide;
    public float flapPowerUp;
    public float flapPowerForward;

    public float bumperForce;
    public float bumperTorqueForce;


    public float takeOffForwardForce;
    public float takeOffUpForce;


    public float carryingForceMultiplier;
    public float carryingDragMultiplier;


    public float paintedWindForceMultiplier;


    public float skimForceUp;
    public float skimForceForward;
    public float skimImpulseMulitplier;


    public float boostMultiplier;

    public float oceanForceMultiplier;
    public float oceanVelocityForceMaxHeight;
    public float oceanVelocityForceMultiplier;
    public float oceanMomentumForceMaxHeight;
    public float oceanMomentumForceMultiplier;
    public float oceanNormalForceMaxHeight;
    public float oceanNormalForceMultiplier;
    public float oceanNormalFlattener;
    public float oceanBoyancyForceMaxHeight;
    public float oceanBoyancyForceMultiplier;
    public float waveLiftForceMaxHeight;
    public float waveLiftForceMultiplier;
    public float oceanToFlatOnExit;
    public float velocityReductionOnEnterWater;

    public void CopyParamsFromWrenPamPam()
    {

    }
}