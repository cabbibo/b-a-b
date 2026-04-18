using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WrenUtils;
using UnityEngine.Splines;
using UnityEngineInternal;

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

        if ( GUILayout.Button( "Do Random New Crystals" ) ) {
            etherTutorialStateManager.DoRandomNewCrystals(); //OnPostWalkingCutSceneComplete();
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

    public Transform cameraControllerForCrystalFilling;


    public float requestSpeed = 1;

    public Phrases    phrases;
    public TMP_Text[] textAssets;


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

        int tmpNumAdded = 0;

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
            print( "Number addtin + " + God.state.tmpCrystalsCollectedPerIsland[whichIslandAdded] );
            print( "Island Added to " + whichIslandAdded );
            tmpNumAdded = God.state.tmpCrystalsCollectedPerIsland[whichIslandAdded];
            newTruth.SetToAdd( whichIslandAdded , tmpNumAdded );
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
                CollectCrystalsFromIsland( whichIslandAdded , tmpNumAdded );
            }


            if ( numWithMoreThanZero > 1 ) {
                Debug.LogError( "NOOO MULTIPLE TMP CRYSTALS ADDED" );
            }

        }


    }

    public Scene scene;


    public void CollectCrystalsFromIsland( int whichIslandAdded , int tmpNumAdded )
    {

        print( "Collecting crystals From Island" );

        God.sceneController.blockAnimation = true;
        //   God.cameraManager.pointOfInterestManager.SetPointOfInterestForTime( newTruth.truthAdder , 12 , 20 , 80 , .03f );
        StartCoroutine( MoveTruthToCenter( scene.portals[whichIslandAdded].startPoint.position , whichIslandAdded , tmpNumAdded ) );


    }


    public void DoRandomNewCrystals()
    {
        God.state.tmpCrystalsCollectedPerIsland[Random.Range( 0 , God.state.tmpCrystalsCollectedPerIsland.Length )] = Random.Range( 0 ,
            100 );

        StateCheck();
    }

    // Helper to get the "weights" of which text section we are looking at
    private float GetWeight( float[] values , int i , float x )
    {
        float vi = values[i];

        float left;
        float right;

        if ( i == 0 ) {
            left = vi - (values[i + 1] - vi) * 0.5f;
        } else {
            left = 0.5f * (values[i - 1] + vi);
        }

        if ( i == values.Length - 1 ) {
            right = vi + (vi - values[i - 1]) * 0.5f;
        } else {
            right = 0.5f * (vi + values[i + 1]);
        }

        if ( x <= left || x >= right ) {
            return 0f;
        }

        if ( x < vi ) {
            return Mathf.InverseLerp( left , vi , x );
        } else {
            return Mathf.InverseLerp( right , vi , x );
        }
    }


    public static string ParseString( string input , int justGotten , int numCrystals , int crystalsLeft , string islandName )
    {
        if ( string.IsNullOrEmpty( input ) ) {
            return input;
        }

        var result = new System.Text.StringBuilder( input.Length );

        for ( int i = 0; i < input.Length; i++ ) {
            if ( input[i] == '{' && i + 2 < input.Length && input[i + 2] == '}' ) {
                char key = input[i + 1];

                switch (key) {

                    case 'J':
                        result.Append( justGotten );
                        break;
                    case 'N':
                        result.Append( numCrystals );
                        break;

                    case 'L':
                        result.Append( crystalsLeft );
                        break;

                    case 'I':
                        result.Append( islandName );
                        break;

                    default:
                        // unknown tag, keep original
                        result.Append( '{' ).Append( "ERROR" ).Append( '}' );
                        break;
                }

                i += 2; // skip {X}
            } else {
                result.Append( input[i] );
            }
        }

        return result.ToString();
    }

    public void PopulateTextFromPhrase( int whichIslandAdded , int numAdded )
    {

        print( "Doing phrase work" );
        print( whichIslandAdded );
        print( God.state.tmpCrystalsCollectedPerIsland[whichIslandAdded] );
        print( God.state.crystalsCollectedPerIsland[whichIslandAdded] );
        print( God.state.crystalsNeededForIslandCompletion[whichIslandAdded] );

        int crystalsNeeded = God.state.crystalsNeededForIslandCompletion[whichIslandAdded];
        int crystalsGotten = God.state.crystalsCollectedPerIsland[whichIslandAdded];
        int crystalsLeftToGo = crystalsNeeded - crystalsGotten;


        bool completed = God.state.islandsCompleted[whichIslandAdded];

        string islandName = God.state.islandNames[whichIslandAdded];
        int justGotten = numAdded;

        StringArrayWrapper phrase;

        if ( completed ) {
            phrase = phrases.alreadyCompletedPhrases[Random.Range( 0 , phrases.alreadyCompletedPhrases.Length )];
        } else {
            phrase = phrases.phrases[Random.Range( 0 , phrases.alreadyCompletedPhrases.Length )];
        }

        for ( int i = 0; i < textAssets.Length; i++ ) {
            textAssets[i].text = ParseString( phrase.data[i] , justGotten , crystalsGotten , crystalsLeftToGo , islandName );
        }

    }

    // TODO : Check if we've already brought crystals from here or not
    // if so, then this part becomes quick ( and less words )
    public IEnumerator MoveTruthToCenter( Vector3 point , int whichIslandAdded , int numAdded )
    {
        God.cameraManager.pointOfInterestManager.SetPointOfInterest( newTruth.truthAdder , cameraControllerForCrystalFilling , 1f );


        float archHeight = 30f;
        var p0 = point;
        var p3 = newTruth.transform.position;
        var mid = (p0 + p3) * 0.5f;

        // Control points raised upward for smooth ascent/descent
        var p1 = Vector3.Lerp( p0 , mid , 0.33f ) + Vector3.up * archHeight;
        var p2 = Vector3.Lerp( mid , p3 , 0.33f ) + Vector3.up * archHeight;


        var curve = new BezierCurve( p0 , p1 , p2 , p3 );
        PopulateTextFromPhrase( whichIslandAdded , numAdded );

        float[] textAssetPoints = new float[textAssets.Length];

        for ( int i = 0; i < textAssets.Length; i++ ) {

            float value = (float)i / textAssets.Length * .65f + .3f;
            textAssetPoints[i] = value;

            Vector3 position = CurveUtility.EvaluatePosition( curve , value );
            var startPos = p0;

            var offset = position + Random.onUnitSphere * 3;

            textAssets[i].transform.position = offset;
            textAssets[i].transform.LookAt( Vector3.Lerp( startPos , position , value * .5f ) , Vector3.up ); // look slightly inwards
            textAssets[i].transform.Rotate( Vector3.up , 180f );

            textAssets[i].gameObject.SetActive( true );


        }

        float duration = 8f;
        float elapsed = 0f;
        float t = 0f;
        float tOld = 0f;

        var noiseOffsetA = new Vector3( Random.value - .5f , Random.value - .5f , Random.value - .5f );
        var noiseOffsetB = new Vector3( Random.value - .5f , Random.value - .5f , Random.value - .5f );
        var noiseOffsetC = new Vector3( Random.value - .5f , Random.value - .5f , Random.value - .5f );

        noiseOffsetA *= 10;
        noiseOffsetB *= 10;
        noiseOffsetC *= 10;

        while (elapsed < duration) {
            t = Mathf.Clamp01( elapsed / duration );
            tOld = Mathf.Clamp01( t - .05f );

            Vector3 curvePoint = CurveUtility.EvaluatePosition( curve , t );
            Vector3 curvePointOld = CurveUtility.EvaluatePosition( curve , tOld );

            var blendedTextPoint = Vector3.zero;
            float totalWeight = 0f;
            float strongestWeight = 0f;

            for ( int i = 0; i < textAssetPoints.Length; i++ ) {
                float w = GetWeight( textAssetPoints , i , t );

                // soften the triangular weight into something smoother / more derivable
                w = Mathf.SmoothStep( 0f , 1f , w );

                blendedTextPoint += textAssets[i].transform.position * w;
                totalWeight += w;
                strongestWeight = Mathf.Max( strongestWeight , w );
            }

            if ( totalWeight > 0.0001f ) {
                blendedTextPoint /= totalWeight;
            } else {
                blendedTextPoint = curvePoint;
            }

            // smooth the overall pull toward the text
            float blendWeight = Mathf.SmoothStep( 0f , 1f , strongestWeight );

            // smooth continuous noise
            float noiseTime = elapsed * 0.35f;

            var noise =
                new Vector3(
                    Mathf.PerlinNoise( noiseTime + noiseOffsetA.x , noiseOffsetA.y ) - 0.5f ,
                    Mathf.PerlinNoise( noiseTime + noiseOffsetB.x , noiseOffsetB.y ) - 0.5f ,
                    Mathf.PerlinNoise( noiseTime + noiseOffsetC.x , noiseOffsetC.y ) - 0.5f
                ) * 2f;

            // stronger near text, weaker away from it
            float noiseAmount = Mathf.Lerp( 0.05f , 0.75f , blendWeight );

            var targetPos = Vector3.Lerp( curvePoint , blendedTextPoint , blendWeight );
            targetPos += noise; // * noiseAmount;

            newTruth.truthAdder.position = targetPos;

            // keep your lagging camera behavior
            cameraControllerForCrystalFilling.transform.position = curvePointOld;

            // much stronger slowdown near text
            float slowWeight = Mathf.SmoothStep( 0f , 1f , strongestWeight );
            float slowMultiplier = Mathf.Lerp( 1.2f , 0.3f , slowWeight * slowWeight * slowWeight );

            elapsed += Time.deltaTime * slowMultiplier;

            yield return null;
        }

        elapsed = 0;
        duration = 3;
        newTruth.OnAdded();

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            yield return null;
        }

        for ( int i = 0; i < textAssets.Length; i++ ) {

            textAssets[i].gameObject.SetActive( false );
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