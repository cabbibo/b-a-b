using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


[ExecuteAlways]
public class WrenParams : MonoBehaviour
{


    public Wren wren;
    public string[] paramFiles;
    int paramID;

    public string paramSetName;


    public void OnEnable()
    {
        print("enabled");
        paramFiles = allNames();
        for (int i = 0; i < paramFiles.Length; i++)
        {
            if (paramFiles[i] == paramSetName)
            {
                paramID = i;
            }
        }

    }
    public void Reset()
    {

        paramFiles = allNames();

        for (int i = 0; i < paramFiles.Length; i++)
        {
            if (paramFiles[i] == paramSetName)
            {
                paramID = i;
            }
        }

        loadParams(paramID);

    }


    public void NextParam()
    {
        paramID += 1;
        if (paramID >= paramFiles.Length) { paramID = 0; }
        paramSetName = paramFiles[paramID];
        loadParams(paramID);
    }

    public void PrevParam()
    {
        paramID -= 1;
        if (paramID < 0) { paramID = paramFiles.Length - 1; }
        paramSetName = paramFiles[paramID];
        loadParams(paramID);
    }

    public virtual void loadParams(int id)
    {
        Load(paramFiles[id]);
    }






    public void SaveNewParamSet()
    {
        paramID = paramFiles.Length;
        for (int i = 0; i < paramFiles.Length; i++)
        {
            if (paramFiles[i] == paramSetName)
            {
                Debug.Log("NAME ALREADY EXISTS");
                return;
            }
        }

        print("SAVING NEW PARAM SET");
        Save(paramSetName);
        paramFiles = allNames();
    }


    public void Load()
    {
        for (int i = 0; i < paramFiles.Length; i++)
        {
            if (paramFiles[i] == paramSetName)
            {
                paramID = i;
            }
        }

        Load(paramFiles[paramID]);
    }

    public void Save()
    {


        /* bool alreadySaved = false;
         for (int i = 0; i < paramFiles.Length; i++)
         {
             if (paramFiles[i] == paramSetName)
             {
                 alreadySaved = true;
             }
         }*/

        Save(paramSetName);


    }



    /*

    swapLR
    invert
    gravityForce
    tuckAddToGravityVal
    slowestTwistAngle
    fastestTwistAngle
    slowestBendAngle
    fastestBendAngle
    twistLerpSpeed
    bendLerpSpeed
    allFeathersMaxSpeed
    noFeathersMaxSpeed
    maxSpeedDamper
    baseSpeed
    baseSpeedDamper
    twistForceVal
    slowestAmountToSide
    fastestAmountToSide
    tuckReduceLiftVal
    closeForwardBoostVal
    thrustForceMultiplier
    strafeVal
    tuckedAngularDrag
    untuckedAngularDrag
    tuckedDrag
    untuckedDrag
    tuckLerpSpeed
    flapToSide
    flapPowerUp
    flapPowerForward
    horizonRightingForceVal
    rightingForce
    rightingDependentOnNotTouchingVal
    closestHeight
    furthestHeight
    closestForce
    furthestForce
    tuckReduceUpdraftVal
    windAmountToTheSide
    groundForceTweenVal
    forwardExtraBoostOnTuck
    tuckDampeningReduction
    velMatchMultiplier
    bumperForce
    bumperTorqueForce
    oceanForceMultiplier
    oceanVelocityForceMaxHeight
    oceanVelocityForceMultiplier
    oceanMomentumForceMaxHeight
    oceanMomentumForceMultiplier
    oceanNormalForceMaxHeight
    oceanNormalForceMultiplier
    oceanNormalFlattener
    oceanBoyancyForceMaxHeight
    oceanBoyancyForceMultiplier
    waveLiftForceMaxHeight
    waveLiftForceMultiplier
    carryingForceMultiplier
    carryingDragMultiplier
    groundPower
    groundOut
    groundDampening
    rotateTowardsTargetOnGround
    groundUpForce
    groundUpVal
    takeOffForwardForce
    takeOffUpForce
    paintedWindForceMultiplier

    */
    public MechanicParams GetWrenMechanics()
    {

        MechanicParams PamPam;
        PamPam = new MechanicParams();
        PamPam.swapLR = wren.physics.swapLR;
        PamPam.invert = wren.physics.invert;
        PamPam.gravityForce = wren.physics.gravityForce;
        PamPam.tuckAddToGravityVal = wren.physics.tuckAddToGravityVal;
        PamPam.slowestTwistAngle = wren.physics.slowestTwistAngle;
        PamPam.fastestTwistAngle = wren.physics.fastestTwistAngle;
        PamPam.slowestBendAngle = wren.physics.slowestBendAngle;
        PamPam.fastestBendAngle = wren.physics.fastestBendAngle;
        PamPam.twistLerpSpeed = wren.physics.twistLerpSpeed;
        PamPam.bendLerpSpeed = wren.physics.bendLerpSpeed;
        PamPam.allFeathersMaxSpeed = wren.physics.allFeathersMaxSpeed;
        PamPam.noFeathersMaxSpeed = wren.physics.noFeathersMaxSpeed;
        PamPam.maxSpeedDamper = wren.physics.maxSpeedDamper;
        PamPam.baseSpeed = wren.physics.baseSpeed;
        PamPam.baseSpeedDamper = wren.physics.baseSpeedDamper;
        PamPam.twistForceVal = wren.physics.twistForceVal;
        PamPam.slowestAmountToSide = wren.physics.slowestAmountToSide;
        PamPam.fastestAmountToSide = wren.physics.fastestAmountToSide;
        PamPam.tuckReduceLiftVal = wren.physics.tuckReduceLiftVal;
        PamPam.closeForwardBoostVal = wren.physics.closeForwardBoostVal;
        PamPam.thrustForceMultiplier = wren.physics.thrustForceMultiplier;
        PamPam.strafeVal = wren.physics.strafeVal;
        PamPam.tuckedAngularDrag = wren.physics.tuckedAngularDrag;
        PamPam.untuckedAngularDrag = wren.physics.untuckedAngularDrag;
        PamPam.tuckedDrag = wren.physics.tuckedDrag;
        PamPam.untuckedDrag = wren.physics.untuckedDrag;
        PamPam.tuckLerpSpeed = wren.physics.tuckLerpSpeed;
        PamPam.flapToSide = wren.physics.flapToSide;
        PamPam.flapPowerUp = wren.physics.flapPowerUp;
        PamPam.flapPowerForward = wren.physics.flapPowerForward;
        PamPam.horizonRightingForceVal = wren.physics.horizonRightingForceVal;
        PamPam.rightingForce = wren.physics.rightingForce;
        PamPam.rightingDependentOnNotTouchingVal = wren.physics.rightingDependentOnNotTouchingVal;
        PamPam.closestHeight = wren.physics.closestHeight;
        PamPam.furthestHeight = wren.physics.furthestHeight;
        PamPam.closestForce = wren.physics.closestForce;
        PamPam.furthestForce = wren.physics.furthestForce;
        PamPam.tuckReduceUpdraftVal = wren.physics.tuckReduceUpdraftVal;
        PamPam.windAmountToTheSide = wren.physics.windAmountToTheSide;
        PamPam.groundForceTweenVal = wren.physics.groundForceTweenVal;
        PamPam.forwardExtraBoostOnTuck = wren.physics.forwardExtraBoostOnTuck;
        PamPam.tuckDampeningReduction = wren.physics.tuckDampeningReduction;
        PamPam.velMatchMultiplier = wren.physics.velMatchMultiplier;
        PamPam.bumperForce = wren.physics.bumperForce;
        PamPam.bumperTorqueForce = wren.physics.bumperTorqueForce;
        PamPam.oceanForceMultiplier = wren.physics.oceanForceMultiplier;
        PamPam.oceanVelocityForceMaxHeight = wren.physics.oceanVelocityForceMaxHeight;
        PamPam.oceanVelocityForceMultiplier = wren.physics.oceanVelocityForceMultiplier;
        PamPam.oceanMomentumForceMaxHeight = wren.physics.oceanMomentumForceMaxHeight;
        PamPam.oceanMomentumForceMultiplier = wren.physics.oceanMomentumForceMultiplier;
        PamPam.oceanNormalForceMaxHeight = wren.physics.oceanNormalForceMaxHeight;
        PamPam.oceanNormalForceMultiplier = wren.physics.oceanNormalForceMultiplier;
        PamPam.oceanNormalFlattener = wren.physics.oceanNormalFlattener;
        PamPam.oceanBoyancyForceMaxHeight = wren.physics.oceanBoyancyForceMaxHeight;
        PamPam.oceanBoyancyForceMultiplier = wren.physics.oceanBoyancyForceMultiplier;
        PamPam.waveLiftForceMaxHeight = wren.physics.waveLiftForceMaxHeight;
        PamPam.waveLiftForceMultiplier = wren.physics.waveLiftForceMultiplier;
        PamPam.carryingForceMultiplier = wren.physics.carryingForceMultiplier;
        PamPam.carryingDragMultiplier = wren.physics.carryingDragMultiplier;
        PamPam.groundPower = wren.physics.groundPower;
        PamPam.groundOut = wren.physics.groundOut;
        PamPam.groundDampening = wren.physics.groundDampening;
        PamPam.rotateTowardsTargetOnGround = wren.physics.rotateTowardsTargetOnGround;
        PamPam.groundUpForce = wren.physics.groundUpForce;
        PamPam.groundUpVal = wren.physics.groundUpVal;
        PamPam.takeOffForwardForce = wren.physics.takeOffForwardForce;
        PamPam.takeOffUpForce = wren.physics.takeOffUpForce;
        PamPam.paintedWindForceMultiplier = wren.physics.paintedWindForceMultiplier;

        PamPam.skimForceUp = wren.physics.skimForceUp;
        PamPam.skimForceForward = wren.physics.skimForceForward;
        PamPam.skimImpulseMulitplier = wren.physics.skimImpulseMulitplier;
        PamPam.boostMultiplier = wren.physics.boostMultiplier;
        PamPam.oceanToFlatOnExit = wren.physics.oceanToFlatOnExit;
        PamPam.velocityReductionOnEnterWater = wren.physics.velocityReductionOnEnterWater;

        return PamPam;

    }



    public void SetWrenMechanics(MechanicParams PamPam)
    {


        print("settintg");
        print(PamPam.skimForceUp);


        wren.physics.swapLR = PamPam.swapLR;
        wren.physics.invert = PamPam.invert;
        wren.physics.gravityForce = PamPam.gravityForce;
        wren.physics.tuckAddToGravityVal = PamPam.tuckAddToGravityVal;
        wren.physics.slowestTwistAngle = PamPam.slowestTwistAngle;
        wren.physics.fastestTwistAngle = PamPam.fastestTwistAngle;
        wren.physics.slowestBendAngle = PamPam.slowestBendAngle;
        wren.physics.fastestBendAngle = PamPam.fastestBendAngle;
        wren.physics.twistLerpSpeed = PamPam.twistLerpSpeed;
        wren.physics.bendLerpSpeed = PamPam.bendLerpSpeed;
        wren.physics.allFeathersMaxSpeed = PamPam.allFeathersMaxSpeed;
        wren.physics.noFeathersMaxSpeed = PamPam.noFeathersMaxSpeed;
        wren.physics.maxSpeedDamper = PamPam.maxSpeedDamper;
        wren.physics.baseSpeed = PamPam.baseSpeed;
        wren.physics.baseSpeedDamper = PamPam.baseSpeedDamper;
        wren.physics.twistForceVal = PamPam.twistForceVal;
        wren.physics.slowestAmountToSide = PamPam.slowestAmountToSide;
        wren.physics.fastestAmountToSide = PamPam.fastestAmountToSide;
        wren.physics.tuckReduceLiftVal = PamPam.tuckReduceLiftVal;
        wren.physics.closeForwardBoostVal = PamPam.closeForwardBoostVal;
        wren.physics.thrustForceMultiplier = PamPam.thrustForceMultiplier;
        wren.physics.strafeVal = PamPam.strafeVal;
        wren.physics.tuckedAngularDrag = PamPam.tuckedAngularDrag;
        wren.physics.untuckedAngularDrag = PamPam.untuckedAngularDrag;
        wren.physics.tuckedDrag = PamPam.tuckedDrag;
        wren.physics.untuckedDrag = PamPam.untuckedDrag;
        wren.physics.tuckLerpSpeed = PamPam.tuckLerpSpeed;
        wren.physics.flapToSide = PamPam.flapToSide;
        wren.physics.flapPowerUp = PamPam.flapPowerUp;
        wren.physics.flapPowerForward = PamPam.flapPowerForward;
        wren.physics.horizonRightingForceVal = PamPam.horizonRightingForceVal;
        wren.physics.rightingForce = PamPam.rightingForce;
        wren.physics.rightingDependentOnNotTouchingVal = PamPam.rightingDependentOnNotTouchingVal;
        wren.physics.closestHeight = PamPam.closestHeight;
        wren.physics.furthestHeight = PamPam.furthestHeight;
        wren.physics.closestForce = PamPam.closestForce;
        wren.physics.furthestForce = PamPam.furthestForce;
        wren.physics.tuckReduceUpdraftVal = PamPam.tuckReduceUpdraftVal;
        wren.physics.windAmountToTheSide = PamPam.windAmountToTheSide;
        wren.physics.groundForceTweenVal = PamPam.groundForceTweenVal;
        wren.physics.forwardExtraBoostOnTuck = PamPam.forwardExtraBoostOnTuck;
        wren.physics.tuckDampeningReduction = PamPam.tuckDampeningReduction;
        wren.physics.velMatchMultiplier = PamPam.velMatchMultiplier;
        wren.physics.bumperForce = PamPam.bumperForce;
        wren.physics.bumperTorqueForce = PamPam.bumperTorqueForce;
        wren.physics.oceanForceMultiplier = PamPam.oceanForceMultiplier;
        wren.physics.oceanVelocityForceMaxHeight = PamPam.oceanVelocityForceMaxHeight;
        wren.physics.oceanVelocityForceMultiplier = PamPam.oceanVelocityForceMultiplier;
        wren.physics.oceanMomentumForceMaxHeight = PamPam.oceanMomentumForceMaxHeight;
        wren.physics.oceanMomentumForceMultiplier = PamPam.oceanMomentumForceMultiplier;
        wren.physics.oceanNormalForceMaxHeight = PamPam.oceanNormalForceMaxHeight;
        wren.physics.oceanNormalForceMultiplier = PamPam.oceanNormalForceMultiplier;
        wren.physics.oceanNormalFlattener = PamPam.oceanNormalFlattener;
        wren.physics.oceanBoyancyForceMaxHeight = PamPam.oceanBoyancyForceMaxHeight;
        wren.physics.oceanBoyancyForceMultiplier = PamPam.oceanBoyancyForceMultiplier;
        wren.physics.waveLiftForceMaxHeight = PamPam.waveLiftForceMaxHeight;
        wren.physics.waveLiftForceMultiplier = PamPam.waveLiftForceMultiplier;
        wren.physics.carryingForceMultiplier = PamPam.carryingForceMultiplier;
        wren.physics.carryingDragMultiplier = PamPam.carryingDragMultiplier;
        wren.physics.groundPower = PamPam.groundPower;
        wren.physics.groundOut = PamPam.groundOut;
        wren.physics.groundDampening = PamPam.groundDampening;
        wren.physics.rotateTowardsTargetOnGround = PamPam.rotateTowardsTargetOnGround;
        wren.physics.groundUpForce = PamPam.groundUpForce;
        wren.physics.groundUpVal = PamPam.groundUpVal;
        wren.physics.takeOffForwardForce = PamPam.takeOffForwardForce;
        wren.physics.takeOffUpForce = PamPam.takeOffUpForce;
        wren.physics.paintedWindForceMultiplier = PamPam.paintedWindForceMultiplier;




        wren.physics.skimForceUp = PamPam.skimForceUp;
        wren.physics.skimForceForward = PamPam.skimForceForward;
        wren.physics.skimImpulseMulitplier = PamPam.skimImpulseMulitplier;
        wren.physics.boostMultiplier = PamPam.boostMultiplier;

        wren.physics.oceanToFlatOnExit = PamPam.oceanToFlatOnExit;
        wren.physics.velocityReductionOnEnterWater = PamPam.velocityReductionOnEnterWater;


    }

    public void Save(string name)
    {

        print("saving");
        print(name);
        MechanicParams PamPam = GetWrenMechanics();


        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(fullName(name));
        bf.Serialize(file, PamPam);
        file.Close();

    }

    public void Load(string name)
    {
        if (File.Exists(fullName(name)))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fullName(name), FileMode.Open);
            MechanicParams PamPam = (MechanicParams)bf.Deserialize(file);
            file.Close();
            SetWrenMechanics(PamPam);


            paramSetName = name;

            paramFiles = allNames();
            bool found = false;
            for (int i = 0; i < paramFiles.Length; i++)
            {
                if (paramFiles[i] == name)
                {
                    //                    print("FOUND");
                    paramID = i;
                    found = true;
                }
            }

            if (!found)
            {
                print("NOT FOUND");
                print(name);
                Debug.LogError("paramID not found   ");
            }
        }
        else
        {

            print("NO FILE");

        }

    }

    public string baseName()
    {
        return Application.streamingAssetsPath + "/mechanics/";
    }

    public string fullName(string n)
    {
        return baseName() + n + ".wren";
    }

    public string[] allNames()
    {
        //        print("LOADING111");
        DirectoryInfo dir = new DirectoryInfo(baseName());
        FileInfo[] info = dir.GetFiles("*.*");
        List<string> paramNames = new List<string>();///sting paramNames = new string[ info.Length ];

        //      print("LOADING");


        foreach (FileInfo f in info)
        {

            string[] s = f.Name.Split(new string[] { ".wren" }, System.StringSplitOptions.None);//);//, StringSplitOptions.None));
            string[] s2 = f.Name.Split(new string[] { ".meta" }, System.StringSplitOptions.None);//);//, StringSplitOptions.None));
                                                                                                 //print( f.Name );
                                                                                                 //print( s.Length );
                                                                                                 //print(s2.Length);
                                                                                                 //print( s[0]);



            if (s2.Length == 1)
            {
                paramNames.Add(s[0]);
            }
        }

        print(paramNames.Count);

        return paramNames.ToArray();//new string[ info.Length ];


    }
}




[System.Serializable]
public class MechanicParams
{

    public bool swapLR;
    public bool invert;
    public float gravityForce;
    public float tuckAddToGravityVal;
    public float slowestTwistAngle;
    public float fastestTwistAngle;
    public float slowestBendAngle;
    public float fastestBendAngle;
    public float twistLerpSpeed;
    public float bendLerpSpeed;
    public float allFeathersMaxSpeed;
    public float noFeathersMaxSpeed;
    public float maxSpeedDamper;
    public float baseSpeed;
    public float baseSpeedDamper;
    public float twistForceVal;
    public float slowestAmountToSide;
    public float fastestAmountToSide;
    public float tuckReduceLiftVal;
    public float closeForwardBoostVal;
    public float thrustForceMultiplier;
    public float strafeVal;
    public float tuckedAngularDrag;
    public float untuckedAngularDrag;
    public float tuckedDrag;
    public float untuckedDrag;
    public float tuckLerpSpeed;
    public float flapToSide;
    public float flapPowerUp;
    public float flapPowerForward;
    public float horizonRightingForceVal;
    public float rightingForce;
    public float rightingDependentOnNotTouchingVal;
    public float closestHeight;
    public float furthestHeight;
    public float closestForce;
    public float furthestForce;
    public float tuckReduceUpdraftVal;
    public float windAmountToTheSide;
    public float groundForceTweenVal;
    public float forwardExtraBoostOnTuck;
    public float tuckDampeningReduction;
    public float velMatchMultiplier;
    public float bumperForce;
    public float bumperTorqueForce;
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
    public float carryingForceMultiplier;
    public float carryingDragMultiplier;
    public float groundPower;
    public float groundOut;
    public float groundDampening;
    public float rotateTowardsTargetOnGround;
    public float groundUpForce;
    public float groundUpVal;
    public float takeOffForwardForce;
    public float takeOffUpForce;
    public float paintedWindForceMultiplier;

    public float skimForceUp;
    public float skimForceForward;
    public float skimImpulseMulitplier;
    public float boostMultiplier;

    public float oceanToFlatOnExit;
    public float velocityReductionOnEnterWater;



}