using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof(OnCrystalDroppedAtPortal) )]
public class OnCrystalDroppedAtPortalEditor : Editor
{
    public override void OnInspectorGUI()
    {


        var script = (OnCrystalDroppedAtPortal)target;

        if ( GUILayout.Button( "Set Pre" ) ) {
            script.SetPre();
        }

        if ( GUILayout.Button( "Set Post" ) ) {
            script.SetPost();
        }

        if ( GUILayout.Button( "OnCrystalDropped" ) ) {
            script.OnCrystalDropped();
        }

        base.OnInspectorGUI();
    }
}


#endif

public class OnCrystalDroppedAtPortal : MonoBehaviour
{
    public Carryable  crystal;
    public GameObject portal;
    public Transform  locationForCrystal;

    public PlayCutScene playCutScene;

    public float positionLerpSpeed = .1f;

    public bool crystalLocked;

    public void OnCrystalDropped()
    {
        God.wren.carrying.DropAllCarriedItems();

        crystal.SetCarryable( false );

        playCutScene.Play();


    }


    public void SetPre()
    {
        playCutScene.SetStartValues();
    }


    public void SetPost()
    {
        playCutScene.SetEndValues();
    }


    // Update is called once per frame
    private void Update()
    {

        if ( playCutScene.playing ) {

            print( "helllo" );


            if ( crystalLocked ) {
                crystal.SetPosition( Vector3.Lerp( crystal.transform.position , locationForCrystal.position ,
                    positionLerpSpeed ) );
            }

        }

    }
}