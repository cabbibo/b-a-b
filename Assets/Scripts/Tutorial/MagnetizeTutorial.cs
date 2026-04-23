using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class MagnetizeTutorial : TutorialCoroutine
{
    public List<GameObject> tutorialTargets;


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
        return God.wrenCanDo.hasLearnedMagnetize;
    }


    // STATE MACHINE FOR TUTORIAL
    public override IEnumerator TutorialSequence()
    {


        God.wren.interfaceUtils.ClearPointers();

        God.interfaceTutorial.ShowContinue( false );
        God.interfaceTutorial.ShowText();

        God.interfaceTutorial.ShowProgress( 0 );
        God.interfaceTutorial.SetBGFade( 0 );


        yield return God.interfaceTutorial.WaitWithCheat( 1 );


        yield return CheckForMagnetize();

        print( "POST PING" );


        OnComplete();


    }

    public ColorManagerForCenterConnection connectToCenter;


    public override void OnComplete()
    {
        base.OnComplete();
        God.interfaceTutorial.TutorialSectionComplete();
        stateManager.OnTutorialEnd( this );
    }


    public bool allConnected = false;


    private IEnumerator CheckForMagnetize()
    {
        God.interfaceTutorial.FadeFullGroupCoroutine( 0 , 1 ); //StartCoroutine(FadeGroup(groupContainer, 0, 1));

        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Magnetize ,
            "SUQ"
        );


        God.wren.interfaceUtils.ClearPointers();

        yield return null;


    }
}