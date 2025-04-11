using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class PingTutorial : TutorialCoroutine
{
    public List<GameObject> tutorialTargets;

    public List<GameObject> tutorialTargets2;
    public int              currentTargetIndex = 0;

    public float hitRadius;


    public override void SetStartState()
    {
        base.SetStartState();
        targetManager = FindObjectOfType<TargetManager>();
        targetManager.gameObject.SetActive( false );
        targetManager.gameObject.SetActive( true );
    }

    public override void SetPreState()
    {
        base.SetPreState();
        targetManager.gameObject.SetActive( false );
        targetManager.gameObject.SetActive( true );
    }


    public override void SetPostState()
    {
        base.SetPostState();
        targetManager.gameObject.SetActive( false );
        targetManager.gameObject.SetActive( true );
    }


    public override bool ConditionsForCompleted()
    {
        return God.wrenCanDo.hasLearnedPing;
    }


    // STATE MACHINE FOR TUTORIAL
    public override IEnumerator TutorialSequence()
    {

        //   yield return BeginningWait();


        print( "DOING SEQUENCE" );
        // DoTutorialSequenceSetup();
        currentTargetIndex = 0;

        God.wren.interfaceUtils.ClearPointers();

        God.interfaceTutorial.ShowContinue( false );
        God.interfaceTutorial.ShowText();

        God.interfaceTutorial.ShowProgress( 0 );
        God.interfaceTutorial.SetBGFade( 0 );


        yield return God.interfaceTutorial.WaitWithCheat( 1 );


        yield return CheckForPingTarget();

        print( "POST PING" );


        OnComplete();


    }

    public override void OnComplete()
    {
        base.OnComplete();
        God.interfaceTutorial.TutorialSectionComplete();
        stateManager.OnTutorialEnd( this );
    }


    private IEnumerator CheckForPing()
    {


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ping ,
            "Press the Ping button to see objective locations"
        );

        God.interfaceTutorial.FadeFullGroupCoroutine( 0 , 1 ); //StartCoroutine(FadeGroup(groupContainer, 0, 1));


        while (true) {
            if ( God.input.triangle ) {
                break;
            }

            yield return null;
        }


    }

    public bool allConnected = false;


    private IEnumerator CheckForPingTarget()
    {
        // {
        //targetManager.currentTarget.transform.position = tutorialTargets[currentTargetIndex].transform.position;


        print( "PING SET" );

        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ping ,
            "Press the Ping button to see objective locations"
        );

        God.interfaceTutorial.FadeFullGroupCoroutine( 0 , 1 ); //StartCoroutine(FadeGroup(groupContainer, 0, 1));

        God.wren.interfaceUtils.ClearPointers();

        print( "ping target happening" );

        SelectTarget();

        while (currentTargetIndex < tutorialTargets.Count) {
            if ( Vector3.Distance( God.wren.transform.position ,
                    tutorialTargets[currentTargetIndex].transform.position ) < hitRadius ) {

                OnTargetHit();

                print( "YA GET FUCKED" );

                if ( currentTargetIndex + 1 == tutorialTargets.Count ) {
                    break;
                }

                currentTargetIndex++;
                SelectTarget();

            }

            yield return null;
        }


        // Swap to all of them

        God.wren.interfaceUtils.ClearPointers();
        targetManager.EraseAllTargets();
        God.interfaceTutorial.TutorialSectionComplete();


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ping ,
            "Connect to the crystals to complete the tutorial"
        );


        God.interfaceTutorial.FadeFullGroupCoroutine( 0 , 1 ); //StartCoroutine(FadeGroup(groupContainer, 0, 1));
        SetSecondTargets();


        while (allConnected != true)
            //  print("WAITING FOR CONNECTED");
            // In the big section
            yield return null;


        God.wren.interfaceUtils.ClearPointers();


        //  print("WE DONE NOW");


        // God.interfaceTutorial.FadeFullGroupCoroutine(1, 0);//StartCoroutine(FadeGroup(groupContainer, 0, 1));


    }


    public void SetSecondTargets()
    {
        for ( int i = 0; i < tutorialTargets2.Count; i++ ) {
            God.wren.interfaceUtils.interfacePointer.AddPointer( tutorialTargets2[i].transform , 0 ,
                new Vector4( 0 , 0 , 0 , 1 ) );
        }
    }

    public void SelectTarget()
    {
        print( "SelectingTarget" );
        //targetManager.SetTarget(tutorialTargets[currentTargetIndex].transform.position);
        //targetManager.DestroyAllPointers();
        targetManager.AddOnlyCurrentPointer( tutorialTargets[currentTargetIndex].transform.position );
    }


    public void OnSelect( GameObject crystal , GameObject lerpTarget , bool connected )
    {

        int id = -1;

        for ( int i = 0; i < tutorialTargets2.Count; i++ ) {
            if ( tutorialTargets2[i] == crystal ) {
                id = i;
                break;
            }
        }

        if ( id == -1 ) {
            Debug.LogError( "Object not found in objectsToConnect array." );
            return;
        } else {
            print( "ID IS " + id );
        }


        if ( connected ) {
            God.wren.interfaceUtils.interfacePointer.AddPointer( tutorialTargets2[id].transform , 0 ,
                new Vector4( 0 , 1 , 0 , 1 ) );
        } else {
            God.wren.interfaceUtils.interfacePointer.AddPointer( tutorialTargets2[id].transform , 0 ,
                new Vector4( 0 , 0 , 0 , 1 ) );
        }

        God.cameraManager.pointOfInterestManager.SetPointOfInterestForTime( lerpTarget.transform , 2 , 20 , 80 , .03f );


    }


    public void OnAllConnected( ConnectToCenter centerConnector )
    {

        print( "ALL CONNECTED" );
        God.particleSystems.EmitForTime( God.particleSystems.fountainParticleSystem ,
            centerConnector.transform.position , 10000 , 2 );
        God.cameraManager.pointOfInterestManager.SetPointOfInterestForTime( centerConnector.transform , 10 , 20 , 80 ,
            .03f );
        allConnected = true;

    }
}