using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteAlways]
public class WrenParams : MonoBehaviour
{
    public  Wren     wren;
    public  string[] paramFiles;
    private int      paramID;

    public string paramSetName;


    public void OnEnable()
    {
        //print("enabled");
        paramFiles = allNames();
        for ( int i = 0; i < paramFiles.Length; i++ ) {
            if ( paramFiles[i] == paramSetName ) {
                paramID = i;
            }
        }

    }

    public void Reset()
    {

        paramFiles = allNames();

        for ( int i = 0; i < paramFiles.Length; i++ ) {
            if ( paramFiles[i] == paramSetName ) {
                paramID = i;
            }
        }

        loadParams( paramID );

    }


    public void NextParam()
    {
        paramID += 1;
        if ( paramID >= paramFiles.Length ) {
            paramID = 0;
        }

        paramSetName = paramFiles[paramID];
        loadParams( paramID );
    }

    public void PrevParam()
    {
        paramID -= 1;
        if ( paramID < 0 ) {
            paramID = paramFiles.Length - 1;
        }

        paramSetName = paramFiles[paramID];
        loadParams( paramID );
    }

    public virtual void loadParams( int id )
    {
        Load( paramFiles[id] );
    }


    public void SaveNewParamSet()
    {
        paramID = paramFiles.Length;
        for ( int i = 0; i < paramFiles.Length; i++ ) {
            if ( paramFiles[i] == paramSetName ) {
                return;
            }
        }

        Save( paramSetName );
        paramFiles = allNames();
    }

    public void LoadParamSet( string name )
    {
        print( "LOADING" );
        paramSetName = name;
        Load();
    }


    public void Load()
    {
        for ( int i = 0; i < paramFiles.Length; i++ ) {
            if ( paramFiles[i] == paramSetName ) {
                paramID = i;
            }
        }

        Load( paramFiles[paramID] );
    }

    public void Save()
    {
        Save( paramSetName );
    }


    public PhysicsParams GetWrenMechanics()
    {

        PhysicsParams PamPam;
        PamPam = new PhysicsParams();

        // loop through every parameter in MechanicsParams and set it to the corresponding value in WrenPhysics
        for ( int i = 0; i < PamPam.GetType().GetFields().Length; i++ ) {
            PamPam.GetType().GetField( PamPam.GetType().GetFields()[i].Name ).SetValue( PamPam ,
                wren.physics.GetType().GetField( PamPam.GetType().GetFields()[i].Name ).GetValue( wren.physics ) );
        }


        return PamPam;

    }


    public void SetWrenMechanics( PhysicsParams PamPam )
    {


        // loop through every parameter in MechanicsParams and set it to the corresponding value in WrenPhysics
        for ( int i = 0; i < PamPam.GetType().GetFields().Length; i++ ) {
            //print(PamPam.GetType().GetFields()[i].Name);
            if ( PamPam.GetType().GetFields()[i].Name == "lockX" ) {
                //                print("print lockX");
                //print(PamPam.GetType().GetFields()[i].GetValue(PamPam));

            }

            //print(PamPam.GetType().GetFields()[i].Name);
            if ( PamPam.GetType().GetFields()[i].Name == "lockY" ) {
                // print("print lockY");
                //print(PamPam.GetType().GetFields()[i].GetValue(PamPam));

            }

            print( wren );
            print( wren.physics );

            print( PamPam.GetType().GetFields()[i].Name );

            wren.physics.GetType().GetField( PamPam.GetType().GetFields()[i].Name ).SetValue( wren.physics ,
                PamPam.GetType().GetFields()[i].GetValue( PamPam ) );
        }


    }

    public void SetWrenMechanicsFromOld( MechanicParams PamPam )
    {


        // loop through every parameter in MechanicsParams and set it to the corresponding value in WrenPhysics
        for ( int i = 0; i < PamPam.GetType().GetFields().Length; i++ ) {
            //print(PamPam.GetType().GetFields()[i].Name);
            if ( PamPam.GetType().GetFields()[i].Name == "lockX" ) {
                //                print("print lockX");
                //print(PamPam.GetType().GetFields()[i].GetValue(PamPam));

            }

            //print(PamPam.GetType().GetFields()[i].Name);
            if ( PamPam.GetType().GetFields()[i].Name == "lockY" ) {
                // print("print lockY");
                //print(PamPam.GetType().GetFields()[i].GetValue(PamPam));

            }

            wren.physics.GetType().GetField( PamPam.GetType().GetFields()[i].Name ).SetValue( wren.physics ,
                PamPam.GetType().GetFields()[i].GetValue( PamPam ) );
        }


    }

    public void Save( string name )
    {

        var PamPam = GetWrenMechanics();


        var bf = new BinaryFormatter();
        var file = File.Create( fullName( name ) );
        bf.Serialize( file , PamPam );
        file.Close();

    }


    public void SaveCurrentAsScriptableObject()
    {


        var PamPam = GetWrenMechanics();

#if UNITY_EDITOR

        AssetDatabase.CreateAsset( PamPam , "Assets/Resources/Parameters/Physics/" + paramSetName + ".asset" );
        AssetDatabase.SaveAssets();

#endif
        /* BinaryFormatter bf = new BinaryFormatter();
         FileStream file = File.Create(fullName(name));
         bf.Serialize(file, PamPam);
         file.Close();*/

    }


    public void LoadPhysicsParams( PhysicsParams pamPam )
    {
        paramSetName = pamPam.name;
        Load( pamPam );
    }

    public void Load( PhysicsParams pamPam )
    {
        paramSetName = pamPam.name;
        SetWrenMechanics( pamPam );
    }

    public void Load( string name )
    {
        if ( File.Exists( fullName( name ) ) ) {
            var bf = new BinaryFormatter();
            var file = File.Open( fullName( name ) , FileMode.Open );
            var PamPam_M = (MechanicParams)bf.Deserialize( file );
            file.Close();
            SetWrenMechanicsFromOld( PamPam_M );


            paramSetName = name;

            paramFiles = allNames();
            bool found = false;
            for ( int i = 0; i < paramFiles.Length; i++ ) {
                if ( paramFiles[i] == name ) {
                    //                    print("FOUND");
                    paramID = i;
                    found = true;
                }
            }

            if ( !found ) {
                print( "NOT FOUND" );
                print( name );
                Debug.LogError( "paramID not found   " );
            }
        }
        else {

            print( "NO FILE" );

        }

    }

    public string baseName()
    {
        return Application.streamingAssetsPath + "/mechanics/";
    }

    public string fullName( string n )
    {
        return baseName() + n + ".wren";
    }

    public string[] allNames()
    {
        //        print("LOADING111");
        var dir = new DirectoryInfo( baseName() );
        var info = dir.GetFiles( "*.*" );
        var paramNames = new List<string>(); ///sting paramNames = new string[ info.Length ];

        //      print("LOADING");


        foreach (var f in info) {

            string[] s = f.Name.Split( new string[] { ".wren" } ,
                System.StringSplitOptions.None ); //);//, StringSplitOptions.None));
            string[] s2 = f.Name.Split( new string[] { ".meta" } ,
                System.StringSplitOptions.None ); //);//, StringSplitOptions.None));
            //print( f.Name );
            //print( s.Length );
            //print(s2.Length);
            //print( s[0]);


            if ( s2.Length == 1 ) {
                paramNames.Add( s[0] );
            }
        }

        //        print(paramNames.Count);

        return paramNames.ToArray(); //new string[ info.Length ];


    }


    public void CopyParamsToScriptableObjects()
    {
        //print("Copying params to scriptable objects");

        string[] names = allNames();
        for ( int i = 0; i < names.Length; i++ ) {
            Load( names[i] );
            //CopyToScriptableObject();
        }


    }
}


[System.Serializable]
public class MechanicParams
{
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
}


[System.Serializable]
public class GeneralWrenParams
{
    public bool canHover;
    public bool canBoost;
    public bool canPing;
    public bool canDisintegrate;

    public bool canCall;
    public bool canMagnitize;
    public bool canPlaceBeacon;
    public bool canRewind;

    public bool canCarry;
}


[System.Serializable]
public class GrowthParams
{
    public float staminaCooldownTime;
    public float staminaRefillSpeed;

    public int crystalsLostPerBoost;
    public int crystalsLostPerPing;
    public int crystalsLostPerDisintegrate;
    public int crystalsLostPerCall;
    public int crystalsLostPerMagnitize;
    public int crystalsLostPerPlaceBeacon;
    public int crystalsLostPerRewind;

    public int crystalsLostWhileCarrying;
}