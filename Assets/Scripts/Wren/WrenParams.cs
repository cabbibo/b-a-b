using System;
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
    public Wren wren;
    //public  string[] physicsParams;
    //private int      physicsParamsID;

    public PhysicsParams[] physicsParams;
    public int             physicsParamsID;
    public string          physicsParamsSetName;
    public PhysicsParams   currentPhysicsParams;


    public void NextParam()
    {
        physicsParamsID += 1;


        if ( physicsParamsID >= physicsParams.Length ) {
            physicsParamsID = 0;
        }

        physicsParamsSetName = physicsParams[physicsParamsID].name;
        loadParams( physicsParamsID );
    }

    public void PrevParam()
    {
        physicsParamsID -= 1;

        if ( physicsParamsID < 0 ) {
            physicsParamsID = physicsParams.Length - 1;
        }

        physicsParamsSetName = physicsParams[physicsParamsID].name;
        loadParams( physicsParamsID );

    }

    public virtual void loadParams( int id )
    {
        LoadPhysics( physicsParams[id] );
    }

    public void Reset()
    {

        if ( currentPhysicsParams != null ) {
            LoadPhysics( currentPhysicsParams );
        } else {
            LoadPhysics( physicsParams[0] );
        }
    }


    public void LoadParamSet( string name )
    {
        print( "LOADING" );
        physicsParamsSetName = name;
        LoadPhysics();
    }


    public void LoadPhysics()
    {
        for ( int i = 0; i < physicsParams.Length; i++ ) {
            if ( physicsParams[i].name == physicsParamsSetName ) {
                physicsParamsID = i;
            }
        }

        SetPhysics( physicsParams[physicsParamsID] );
    }

    public void SetPhysics( PhysicsParams p )
    {
        currentPhysicsParams = p;
        physicsParamsSetName = p.name;
        SetWrenPhysics( p );

    }

    public void OnPhysicsParamsValidate( PhysicsParams p )
    {
        SetWrenPhysics( p );
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


    public void SetWrenPhysics( PhysicsParams PamPam )
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


    public void SaveCurrentAsScriptableObject()
    {


        var PamPam = GetWrenMechanics();

#if UNITY_EDITOR

        AssetDatabase.CreateAsset( PamPam , "Assets/Resources/Parameters/Physics/" + physicsParamsSetName + ".asset" );
        AssetDatabase.SaveAssets();

#endif


    }


    public void LoadPhysics( PhysicsParams pamPam )
    {
        SetPhysics( pamPam );
    }
}


[Serializable]
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


[Serializable]
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