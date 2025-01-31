using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class PingTutorial : TutorialCoroutine
{





    // STATE MACHINE FOR TUTORIAL
    IEnumerator TutorialSequence()
    {

        //   yield return BeginningWait();



        // DoTutorialSequenceSetup();

        yield return God.interfaceTutorial.WaitWithCheat(10);

        yield return God.interfaceTutorial.FadeGroup(God.interfaceTutorial.groupContainer, 0, 1);
        yield return God.interfaceTutorial.WaitWithCheat(5);


    }

}
