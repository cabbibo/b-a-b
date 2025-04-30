using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using God = WrenUtils.God;

public class EtherStateController : MonoBehaviour
{
    public Scene scene;
    public bool  allOn;

    public int currentAnimationPlayed = 0;


    public PlayCutScene[] animationCutScenes;

    /*
        TODO
        Quest sets 'last quest completed' in player prefs
        when the quest is completed, we check if the animation has been played
        play based on which quest is completed

    */

    public void OnEnable()
    {


        currentAnimationPlayed = PlayerPrefs.GetInt( "MainAnimationsPlayed" , 0 );

        // Setting up cut scene start values
        for ( int i = 0; i < animationCutScenes.Length; i++ ) {
            animationCutScenes[i].SetStartValues();
        }

        // if its already played we play it
        for ( int i = 0; i < currentAnimationPlayed; i++ ) {
            animationCutScenes[i].SetEndValues();
        }


        animationCutScenes[currentAnimationPlayed].Play();
        currentAnimationPlayed++;

        /*

            for (int i = 0; i < scene.portals.Length; i++)
            {

                if (allOn)
                {
                    scene.portals[i].SetPortalFull();
                }
                else
                {




                }
            }

            */
    }


    /*if (God.state.questsCompleted[i])
             {
                 scene.portals[i].SetPortalFull();
             }
             else
             {
                 scene.portals[i].SetPortalOff();
             }*/


    public void OnDisable()
    {

    }
}