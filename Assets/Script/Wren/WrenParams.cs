using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


[ExecuteAlways]
public class WrenParams : MonoBehaviour
{


    public Wren wren;
    public string[] paramFiles;
    public int paramID;
    public int oParamID;


    public void OnEnable()
    {
        paramFiles = allNames();
    }
    public void Reset()
    {

        paramFiles = allNames();
        loadParams(paramID);

    }


    public void NextParam()
    {
        paramID += 1;
        if (paramID >= paramFiles.Length) { paramID = 0; }
        loadParams(paramID);
    }

    public void PrevParam()
    {
        paramID -= 1;
        if (paramID < 0) { paramID = paramFiles.Length - 1; }
        loadParams(paramID);
    }

    public virtual void loadParams(int id)
    {
        Load(paramFiles[id]);
        oParamID = paramID;
    }






    public void SaveNewParamSet()
    {
        string name = "controlSet" + Mathf.Floor(Random.Range(0.001f, .999f) * 100000);
        paramID = paramFiles.Length;
        oParamID = paramID;
        Save(name);
        paramFiles = allNames();
    }


    public void Load()
    {

        Load(paramFiles[paramID]);
        oParamID = paramID;
    }

    public void Save()
    {
        Save(paramFiles[paramID]);
    }



    public MechanicParams GetWrenMechanics()
    {

        MechanicParams PamPam;
        PamPam = new MechanicParams();

        PamPam.invert = wren.physics.invert;

        PamPam.gravityForce = wren.physics.gravityForce;
        PamPam.tuckAddToGravityVal = wren.physics.tuckAddToGravityVal;

        PamPam.slowestTwistAngle = wren.physics.slowestTwistAngle;
        PamPam.fastestTwistAngle = wren.physics.fastestTwistAngle;

        PamPam.slowestBendAngle = wren.physics.slowestBendAngle;
        PamPam.fastestBendAngle = wren.physics.fastestBendAngle;

        PamPam.twistLerpSpeed = wren.physics.twistLerpSpeed;
        PamPam.bendLerpSpeed = wren.physics.bendLerpSpeed;

        PamPam.maxSpeed = wren.physics.maxSpeed;

        PamPam.twistForceVal = wren.physics.twistForceVal;

        PamPam.slowestAmountToSide = wren.physics.slowestAmountToSide;
        PamPam.fastestAmountToSide = wren.physics.fastestAmountToSide;

        PamPam.tuckReduceLiftVal = wren.physics.tuckReduceLiftVal;

        PamPam.closeForwardBoostVal = wren.physics.closeForwardBoostVal;
        PamPam.thrustForceMultiplier = wren.physics.thrustForceMultiplier;

        PamPam.rightingForce = wren.physics.rightingForce;

        PamPam.strafeVal = wren.physics.strafeVal;

        PamPam.tuckedAngularDrag = wren.physics.tuckedAngularDrag;
        PamPam.untuckedAngularDrag = wren.physics.untuckedAngularDrag;

        PamPam.tuckedDrag = wren.physics.tuckedDrag;
        PamPam.untuckedDrag = wren.physics.untuckedDrag;

        PamPam.closestHeight = wren.physics.closestHeight;
        PamPam.furthestHeight = wren.physics.furthestHeight;
        PamPam.closestForce = wren.physics.closestForce;
        PamPam.furthestForce = wren.physics.furthestForce;

        PamPam.tuckReduceUpdraftVal = wren.physics.tuckReduceUpdraftVal;
        PamPam.windAmountToTheSide = wren.physics.windAmountToTheSide;

        PamPam.rightingDependentOnNotTouchingVal = wren.physics.rightingDependentOnNotTouchingVal;
        PamPam.groundForceTweenVal = wren.physics.groundForceTweenVal;

        PamPam.tuckLerpSpeed = wren.physics.tuckLerpSpeed;
        PamPam.flapToSide = wren.physics.flapToSide;
        PamPam.flapPowerUp = wren.physics.flapPowerUp;
        PamPam.flapPowerForward = wren.physics.flapPowerForward;
        PamPam.takeOffUpForce = wren.physics.takeOffUpForce;
        PamPam.takeOffForwardForce = wren.physics.takeOffForwardForce;
        PamPam.groundUpForce = wren.physics.groundUpForce;
        PamPam.groundUpVal = wren.physics.groundUpVal;

        PamPam.groundPower = wren.physics.groundPower;
        PamPam.groundOut = wren.physics.groundOut;
        PamPam.groundDampening = wren.physics.groundDampening;

        PamPam.velMatchMultiplier = wren.physics.velMatchMultiplier;

        return PamPam;

    }



    public void SetWrenMechanics(MechanicParams PamPam)
    {




        wren.physics.invert = PamPam.invert;

        wren.physics.gravityForce = PamPam.gravityForce;
        wren.physics.tuckAddToGravityVal = PamPam.tuckAddToGravityVal;

        wren.physics.slowestTwistAngle = PamPam.slowestTwistAngle;
        wren.physics.fastestTwistAngle = PamPam.fastestTwistAngle;

        wren.physics.slowestBendAngle = PamPam.slowestBendAngle;
        wren.physics.fastestBendAngle = PamPam.fastestBendAngle;

        wren.physics.twistLerpSpeed = PamPam.twistLerpSpeed;
        wren.physics.bendLerpSpeed = PamPam.bendLerpSpeed;

        wren.physics.maxSpeed = PamPam.maxSpeed;

        wren.physics.twistForceVal = PamPam.twistForceVal;

        wren.physics.slowestAmountToSide = PamPam.slowestAmountToSide;
        wren.physics.fastestAmountToSide = PamPam.fastestAmountToSide;

        wren.physics.tuckReduceLiftVal = PamPam.tuckReduceLiftVal;

        wren.physics.closeForwardBoostVal = PamPam.closeForwardBoostVal;
        wren.physics.thrustForceMultiplier = PamPam.thrustForceMultiplier;

        wren.physics.rightingForce = PamPam.rightingForce;

        wren.physics.strafeVal = PamPam.strafeVal;

        wren.physics.tuckedAngularDrag = PamPam.tuckedAngularDrag;
        wren.physics.untuckedAngularDrag = PamPam.untuckedAngularDrag;

        wren.physics.tuckedDrag = PamPam.tuckedDrag;
        wren.physics.untuckedDrag = PamPam.untuckedDrag;

        wren.physics.closestHeight = PamPam.closestHeight;
        wren.physics.furthestHeight = PamPam.furthestHeight;
        wren.physics.closestForce = PamPam.closestForce;
        wren.physics.furthestForce = PamPam.furthestForce;


        wren.physics.tuckLerpSpeed = PamPam.tuckLerpSpeed;

        wren.physics.flapToSide = PamPam.flapToSide;
        wren.physics.flapPowerUp = PamPam.flapPowerUp;
        wren.physics.flapPowerForward = PamPam.flapPowerForward;


        wren.physics.tuckReduceUpdraftVal = PamPam.tuckReduceUpdraftVal;
        wren.physics.windAmountToTheSide = PamPam.windAmountToTheSide;

        wren.physics.rightingDependentOnNotTouchingVal = PamPam.rightingDependentOnNotTouchingVal;

        wren.physics.groundForceTweenVal = PamPam.groundForceTweenVal;

        wren.physics.takeOffUpForce = PamPam.takeOffUpForce;
        wren.physics.takeOffForwardForce = PamPam.takeOffForwardForce;

        wren.physics.groundUpForce = PamPam.groundUpForce;
        wren.physics.groundUpVal = PamPam.groundUpVal;
        wren.physics.velMatchMultiplier = PamPam.velMatchMultiplier;



        wren.physics.groundPower = PamPam.groundPower;
        wren.physics.groundOut = PamPam.groundOut;
        wren.physics.groundDampening = PamPam.groundDampening;

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

        DirectoryInfo dir = new DirectoryInfo(baseName());
        FileInfo[] info = dir.GetFiles("*.*");
        List<string> paramNames = new List<string>();///sting paramNames = new string[ info.Length ];

        //print("LOADING");
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

        return paramNames.ToArray();//new string[ info.Length ];


    }
}




[System.Serializable]
public class MechanicParams
{


    public bool invert;

    public float gravityForce;
    public float tuckAddToGravityVal;

    public float slowestTwistAngle;
    public float fastestTwistAngle;

    public float slowestBendAngle;
    public float fastestBendAngle;

    public float twistLerpSpeed;
    public float bendLerpSpeed;

    public float maxSpeed;

    public float twistForceVal;

    public float slowestAmountToSide;
    public float fastestAmountToSide;

    public float tuckReduceLiftVal;

    public float closeForwardBoostVal;
    public float thrustForceMultiplier;

    public float rightingForce;

    public float strafeVal;


    public float tuckedAngularDrag;
    public float untuckedAngularDrag;


    public float tuckedDrag;
    public float untuckedDrag;



    public float tuckLerpSpeed;

    public float flapToSide;
    public float flapPowerUp;
    public float flapPowerForward;

    /*

    How Wind is defined and closness to 
    surface matters
    */
    public float closestHeight;
    public float furthestHeight;
    public float closestForce;
    public float furthestForce;

    public float tuckReduceUpdraftVal;
    public float windAmountToTheSide;

    public float rightingDependentOnNotTouchingVal;

    public float groundForceTweenVal;

    public float takeOffUpForce;
    public float takeOffForwardForce;

    public float groundUpVal;
    public float groundUpForce;
    public float velMatchMultiplier;

    public float groundPower;
    public float groundOut;

    public float groundDampening;


}