using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof(EtherTutorialStateManager) )]
public class EtherTutorialStateManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var etherTutorialStateManager = (EtherTutorialStateManager)target;

        DrawDefaultInspector();

        if ( GUILayout.Button( "SetStartValues" ) ) {
            etherTutorialStateManager.postTutorialIslandCutScene.SetStartValues();
            etherTutorialStateManager.postWalkingCutScene.SetStartValues();
        }

        if ( GUILayout.Button( "SetFirstAnimationEnd" ) ) {
            etherTutorialStateManager.postTutorialIslandCutScene.SetEndValues();
        }

        if ( GUILayout.Button( "SetSecondAnimationEnd" ) ) {
            etherTutorialStateManager.postWalkingCutScene.SetEndValues(); //OnPostWalkingCutSceneComplete();
        }

        /*  if ( GUILayout.Button( "OnPostCutSceneComplete" ) ) {
              etherTutorialStateManager.OnPostCutSceneComplete();
          }*/


    }
}

#endif

public class EtherTutorialStateManager : TutorialStateManager
{
    public WalkingTutorial walkingTutorial;


    public PlayCutScene postTutorialIslandCutScene;
    public PlayCutScene postWalkingCutScene;

    public PlayCutScene[] cutScenes;

    public override void Initialize()
    {

        // No MATTER WHAT we can do these
        God.state.wrenCanDo.ping = true;
        God.state.wrenCanDo.takeOff = true;

        walkingTutorial.CheckState();

        if ( God.state.wrenCanDo.hasLearnedWalk == false ) {

            print( "hiii" );

            God.wren.shards.SetToBodyShards();
            postTutorialIslandCutScene.SetStartValues();
            postWalkingCutScene.SetStartValues();
            postTutorialIslandCutScene.Play();

        } else {
            // TODO check to see what other islands have been visited / completed

            print( "AFTER" );
            postTutorialIslandCutScene.SetEndValues();
            postWalkingCutScene.SetEndValues();

            StateCheck();

        }

        print( "full after" );

    }

    public void OnPostTutorialCutSceneComplete()
    {
        walkingTutorial.JumpStartTutorial();
    }

    public void OnPostWalkingCutSceneComplete()
    {

    }

    public void OnPostCutSceneComplete()
    {

    }

    public void StateCheck()
    {
        if ( God.state.newIslandDiscovered != -1 ) {
            DoNewIslandDiscovered( God.state.newIslandDiscovered );
        }

        if ( God.state.newIslandCompleted != -1 ) {
            DoNewIslandCompleted( God.state.newIslandCompleted );
        }


    }


    public void DoNewIslandCompleted( int islandID )
    {
        // do it
        // Check which 

        cutScenes[islandID].Play();

        God.state.ResetIslandCompleted();
    }

    public void DoNewIslandDiscovered( int islandID )
    {
        // do it


        God.state.ResetIslandDiscovered();

    }


    public void DoWalkingTutorialFinish()
    {

        God.state.wrenCanDo.hasLearnedWalk = true;
        God.state.UpdateState();

        postWalkingCutScene.Play();

    }


    public override void OnTutorialEnd( TutorialCoroutine tutorial )
    {

        print( "OnTutorialEnd" );

        if ( tutorial is WalkingTutorial ) {
            print( "WALKING TUTORIAL END" );
            DoWalkingTutorialFinish();
        }


    }


    public override void OnProgress( TutorialCoroutine tutorial , float progress )
    {

    }
}