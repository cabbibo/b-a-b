using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class PingTutorial : TutorialCoroutine
{

    public List<GameObject> tutorialTargets;
    public int currentTargetIndex = 0;

    public float hitRadius;





    // STATE MACHINE FOR TUTORIAL
    public override IEnumerator TutorialSequence()
    {

        //   yield return BeginningWait();




        print("DOING SEQUENCE");
        // DoTutorialSequenceSetup();
        currentTargetIndex = 0;

        God.wren.interfaceUtils.ClearPointers();

        God.interfaceTutorial.ShowContinue(false);
        God.interfaceTutorial.ShowText();

        God.interfaceTutorial.ShowProgress(0);
        God.interfaceTutorial.SetBGFade(0);


        yield return God.interfaceTutorial.WaitWithCheat(1);


        yield return CheckForPingTarget();

        print("POST PING");

        God.interfaceTutorial.TutorialSectionComplete();
        stateManager.OnTutorialEnd(this);



    }


    IEnumerator CheckForPing()
    {


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ping,
            "Press the Ping button to see objective locations"
            );

        God.interfaceTutorial.FadeFullGroupCoroutine(0, 1);//StartCoroutine(FadeGroup(groupContainer, 0, 1));


        while (true)
        {
            if (God.input.triangle)
            {
                break;
            }

            yield return null;
        }


    }

    IEnumerator CheckForPingTarget()
    {
        // {
        //targetManager.currentTarget.transform.position = tutorialTargets[currentTargetIndex].transform.position;


        print("PING SET");

        God.interfaceTutorial.SetControllerHint(
                   InterfaceTutorial.ControllerHint.Ping,
                   "Press the Ping button to see objective locations"
       );

        God.interfaceTutorial.FadeFullGroupCoroutine(0, 1);//StartCoroutine(FadeGroup(groupContainer, 0, 1));

        God.wren.interfaceUtils.ClearPointers();

        print("ping target happening");

        SelectTarget();

        while (currentTargetIndex < tutorialTargets.Count)
        {
            if (Vector3.Distance(God.wren.transform.position, tutorialTargets[currentTargetIndex].transform.position) < hitRadius)
            {

                OnTargetHit();

                print("YA GET FUCKED");

                if (currentTargetIndex + 1 == tutorialTargets.Count)
                {
                    break;
                }

                currentTargetIndex++;
                SelectTarget();

            }

            yield return null;
        }


        print("WE DONE NOW");



        God.interfaceTutorial.FadeFullGroupCoroutine(1, 0);//StartCoroutine(FadeGroup(groupContainer, 0, 1));



    }

    public void SelectTarget()
    {
        print("SelectingTarget");
        targetManager.SetTarget(tutorialTargets[currentTargetIndex].transform.position);
        God.wren.interfaceUtils.interfacePointer.AddPointer(targetManager.currentTarget.transform, 0, new Vector4(0, 0, 0, 1));
    }

}
