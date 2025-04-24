using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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