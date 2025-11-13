using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using WrenUtils;
using God = WrenUtils.God;

public class WalkingTutorial : TutorialCoroutine
{
    public List<GameObject> tutorialTargets;

    public List<GameObject> tutorialTargets2;
    public int              currentTargetIndex = 0;

    public float hitRadius;

    public Transform      reachLocation;
    public ParticleSystem reachParticleSystem;

    public LerpOnCurveAmount etherCurve;


/*
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
*/

    public override bool ConditionsForCompleted()
    {

        //     print( "checking state" );

        if ( God.wrenCanDo.hasLearnedWalk ) {
//            print( "has learned walk" );
        } else {
            //           print( "has NOT learned walk" );
        }

        return God.wrenCanDo.hasLearnedWalk;
    }


    // STATE MACHINE FOR TUTORIAL
    public override IEnumerator TutorialSequence()
    {

        currentTargetIndex = 0;

        God.wren.shards.SetToBodyShards(); // NO EXTRAS PLEASE

        God.wren.interfaceUtils.ClearPointers();

        God.interfaceTutorial.ShowContinue( false );
        God.interfaceTutorial.ShowText();

        God.interfaceTutorial.ShowProgress( 0 );
        God.interfaceTutorial.SetBGFade( 0 );


        yield return God.interfaceTutorial.WaitWithCheat( 1 );
        yield return CheckWalkingTutorial();

        OnComplete();

    }

    public override void OnComplete()
    {

        print( "On COMPLETE" );
        base.OnComplete();
        God.interfaceTutorial.TutorialSectionComplete();

        foreach (var preObject in preObjects) preObject.SetActive( false );

        stateManager.OnTutorialEnd( this );
    }

    private IEnumerator CheckWalkingTutorial()
    {

        God.state.wrenCanDo.takeOff = false;

        God.interfaceTutorial.FadeFullGroupCoroutine( 0 , 1 ); //StartCoroutine(FadeGroup(groupContainer, 0, 1));


        float t;


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ground_Left ,
            "TURN"
        );

        t = 0;

        while (t < 1) {
            if ( God.wren.input.leftY < 0 && God.wren.input.rightY > 0 ) {
                t += Time.deltaTime;
                God.interfaceTutorial.ShowProgress( t );
            }

            yield return null;
        }

        God.interfaceTutorial.ShowProgress( 0 );
        God.interfaceTutorial.SmallSectionComplete();


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ground_Right ,
            "TURN"
        );

        t = 0;

        while (t < 1) {
            if ( God.wren.input.leftY > 0 && God.wren.input.rightY < 0 ) {
                t += Time.deltaTime;
                God.interfaceTutorial.ShowProgress( t );
            }

            yield return null;
        }

        God.interfaceTutorial.ShowProgress( 0 );
        God.interfaceTutorial.SmallSectionComplete();


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ground_Forward ,
            "FORWARD"
        );

        t = 0;

        while (t < 1) {
            if ( God.wren.input.leftY > 0 && God.wren.input.rightY > 0 ) {
                t += Time.deltaTime;
                God.interfaceTutorial.ShowProgress( t );
            }

            yield return null;
        }


        God.interfaceTutorial.ShowProgress( 0 );
        God.interfaceTutorial.SmallSectionComplete();


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.None ,
            "WALK"
        );


        God.wren.interfaceUtils.ClearPointers();
        God.wren.interfaceUtils.interfacePointer.AddPointer( reachLocation.transform , 0 ,
            new Vector4( 0 , 0 , 0 , 1 ) );


        reachParticleSystem.Play();
        etherCurve.gameObject.SetActive( true );
        etherCurve.LerpOn( 5 );
        God.cameraManager.pointOfInterestManager.SetPointOfInterestForTime( reachParticleSystem.transform , 5 , 3 , 80 , .1f );


        while ((God.wren.transform.position - reachLocation.position).magnitude > hitRadius)
            // TODO MORE HERE
            yield return null;


        reachParticleSystem.Stop();

        God.state.wrenCanDo.takeOff = true;

        God.wren.interfaceUtils.ClearPointers();


    }
}