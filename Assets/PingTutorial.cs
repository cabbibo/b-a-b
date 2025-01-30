using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingTutorial : TutorialCoroutine
{





    // STATE MACHINE FOR TUTORIAL
    IEnumerator TutorialSequence()
    {

        //   yield return BeginningWait();



        // DoTutorialSequenceSetup();

        yield return WaitWithCheat(10);

        yield return FadeGroup(groupContainer, 0, 1);
        yield return WaitWithCheat(5);


    }

}
