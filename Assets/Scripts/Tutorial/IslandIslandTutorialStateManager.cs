using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class IslandIslandTutorialStateManager : TutorialStateManager
{
    public MagnetizeTutorial magnetizeTutorial;

    public override void Initialize()
    {


        magnetizeTutorial.CheckState();

        if ( God.state.wrenCanDo.hasLearnedMagnetize == false ) {
            magnetizeTutorial.JumpStartTutorial();
        }

        God.state.UpdateState();

    }


    public void DoMangetizeTutorialFinish()
    {


        God.state.wrenCanDo.hasLearnedMagnetize = true;
        God.state.UpdateState();

        DoFullFinish();


    }


    public override void OnTutorialEnd( TutorialCoroutine tutorial )
    {


        print( "OnTutorialEnd" );
        DoMangetizeTutorialFinish();

    }

    public bool DoFullFinish()
    {

        God.state.UpdateState();

        return true;
    }


    /*

        TODO

        Start the ping tutorial when the player first uses the take off

    */

    public override void OnProgress( TutorialCoroutine tutorial , float progress )
    {
        print( "progggers" );
    }
}