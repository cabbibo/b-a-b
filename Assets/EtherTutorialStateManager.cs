using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Splines;

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

    public PlayCutScene finishCutScene;

    public BindNewTruthData newTruth;

    public void FinishGame()
    {
        God.state.FinishGame();
    }

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


            postTutorialIslandCutScene.SetEndValues();
            postWalkingCutScene.SetEndValues();

            StateCheck();

        }

//        print( "full after" );

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
        print( "AFTER" );
        // see if we've got new crystals
        int numWithMoreThanZero = 0;
        int whichIslandAdded = -1;

        for ( int i = 0; i < God.state.numIslands; i++ ) {
            if ( God.state.tmpCrystalsCollectedPerIsland[i] > 0 ) {
                print( "GOT ONE" );
                numWithMoreThanZero++;
                whichIslandAdded = i;
            }
        }

        if ( numWithMoreThanZero == 1 ) {
            // DoLerpToCenterFromCorrectIsland
            //  God.cameraManager.pointOfInterestManager.SetPointOfInterestForTime( lerpTarget.transform , 2 , 20 , 80 , .03f );

            print( "collecttting" );
            newTruth.SetToAdd( whichIslandAdded , God.state.tmpCrystalsCollectedPerIsland[whichIslandAdded] );
            God.state.ConsumeCrystals();
        }


        if ( numWithMoreThanZero > 1 ) {
            Debug.LogError( "NOOO MULTIPLE TMP CRYSTALS ADDED" );
        }

        print( "checking state" );
        bool newIslandCompleted = false;

        for ( int i = 0; i < God.state.numIslands; i++ ) {
            if ( God.state.islandsCompleted[i] == false &&
                 God.state.crystalsCollectedPerIsland[i] > God.state.crystalsNeededForIslandCompletion[i] ) {

                DoNewIslandCompleted( i );
                newIslandCompleted = true;
                break;
            }

            if ( God.state.islandsCompleted[i] == true ) {
                DoIslandAlreadyComplete( i );
            } else {
                DoIslandNotComplete( i );
            }


        }

        if ( God.state.gameFinished == false ) {
            finishCutScene.SetStartValues();
        } else {
            finishCutScene.SetEndValues();
        }

        if ( !newIslandCompleted ) {

            print( "now new island" );

            if ( numWithMoreThanZero == 1 ) {
                CollectCrystalsFromIsland( whichIslandAdded );
            }


            if ( numWithMoreThanZero > 1 ) {
                Debug.LogError( "NOOO MULTIPLE TMP CRYSTALS ADDED" );
            }

        }


    }

    public Scene scene;

    public void CollectCrystalsFromIsland( int whichIslandAdded )
    {


        print( "settting" );

        God.sceneController.blockAnimation = true;
        //   God.cameraManager.pointOfInterestManager.SetPointOfInterestForTime( newTruth.truthAdder , 12 , 20 , 80 , .03f );
        StartCoroutine( MoveTruthToCenter( scene.portals[whichIslandAdded].startPoint.position ) );


    }

    public IEnumerator MoveTruthToCenter( Vector3 point )
    {
        God.cameraManager.pointOfInterestManager.SetPointOfInterest( newTruth.truthAdder , .3f , false );

        float archHeight = 30f;
        var p0 = point;
        var p3 = newTruth.transform.position;
        var mid = (p0 + p3) * 0.5f;

        // Control points raised upward for smooth ascent/descent
        var p1 = Vector3.Lerp( p0 , mid , 0.33f ) + Vector3.up * archHeight;
        var p2 = Vector3.Lerp( mid , p3 , 0.33f ) + Vector3.up * archHeight;


        var curve = new BezierCurve( p0 , p1 , p2 , p3 );

        float duration = 12f;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01( elapsed / duration );
            print( "going + " + t );
            newTruth.truthAdder.position = CurveUtility.EvaluatePosition( curve , t );

            yield return null;
        }

        elapsed = 0;
        duration = 3;
        newTruth.OnAdded();

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            yield return null;
        }


        God.cameraManager.pointOfInterestManager.ClearPointOfInterest();


    }


    public void OnColorIslandComplete()
    {

        print( "OnColorIslandComplete" );

        // Add to the newTruth;

        // Consume the crystals into the main simulation
        newTruth.OnAdded();


        bool allFinished = true;

        for ( int i = 0; i < God.state.numIslands; i++ ) {
            if ( God.state.islandsCompleted[i] == false ) {
                print( "not finished: " + i );
                allFinished = false;
            }
        }


        print( "ALL FINISHED: " + allFinished );

        if ( allFinished && God.state.gameFinished == false ) {
            DoGameFinish();
        }

    }

    public void DoGameFinish()
    {
        finishCutScene.Play();
    }

    public void DoNewIslandCompleted( int islandID )
    {

        print( " NEW ISLAND COMPLETED" );
        print( islandID );
        print( "cutScenePlaying" );
        God.sceneController.blockAnimation = true;
        // do it
        // Check which 
        God.sceneController.EndPortalAnimation();
        cutScenes[islandID].Play();
        God.state.CompleteIsland( islandID );
        God.state.ResetIslandCompleted();
    }


    public void DoIslandAlreadyComplete( int islandID )
    {
        cutScenes[islandID].SetEndValues();
    }

    public void DoIslandNotComplete( int islandID )
    {
        cutScenes[islandID].SetStartValues();
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