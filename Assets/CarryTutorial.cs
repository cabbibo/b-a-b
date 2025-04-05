using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CarryTutorial : TutorialCoroutine
{

    public Carryable objectToCarry;

    public float giveCarryableInfoDistance = 30f;

    public Transform portal;

    public float portalHitDistance = 20f;




    // STATE MACHINE FOR TUTORIAL
    public override IEnumerator TutorialSequence()
    {

        //   yield return BeginningWait();








        print("DOING SEQUENCE");

        God.wren.interfaceUtils.ClearPointers();

        God.interfaceTutorial.ShowContinue(false);
        God.interfaceTutorial.ShowText();

        God.interfaceTutorial.ShowProgress(0);
        God.interfaceTutorial.SetBGFade(0);


        yield return God.interfaceTutorial.WaitWithCheat(1);


        yield return CheckForCarry();

        print("POST PING");

        OnComplete();



    }

    public override bool ConditionsForCompleted()
    {
        return God.wrenCanDo.hasLearnedCarry;
    }


    public override void OnComplete()
    {
        base.OnComplete();
        God.interfaceTutorial.TutorialSectionComplete();
        stateManager.OnTutorialEnd(this);
    }



    IEnumerator CheckForCarry()
    {

        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ping,
            "Fly to the holy crystal ( ping to find it)"
            );



        God.interfaceTutorial.FadeFullGroupCoroutine(0, 1);//StartCoroutine(FadeGroup(groupContainer, 0, 1));

        God.wren.interfaceUtils.SetObjectOfInterest(objectToCarry.transform);//




        while ((God.wren.transform.position - objectToCarry.transform.position).magnitude > giveCarryableInfoDistance)
        {
            //            print("TOO FAR AWAY");
            yield return null;
        }


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Carry,
            "Hold R1 OR L1 to pick up and carry objects"
            );

        God.interfaceTutorial.FadeFullGroupCoroutine(0, 1);//StartCoroutine(FadeGroup(groupContainer, 0, 1));


        while (objectToCarry.BeingCarried == false)
        {
            //  print("NOT CARRIED");

            yield return null;
        }

        portal.gameObject.SetActive(true);

        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Release,
            "Release L1 / R1 to drop the object"
            );



        while (objectToCarry.BeingCarried == true)
        {
            //print("CARRIED");

            yield return null;
        }


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Carry,
            "Grab it again! ( ping to find it )"
        );


        while (objectToCarry.BeingCarried == false)
        {
            // print("NOT CARRIED");
            yield return null;
        }









        while ((objectToCarry.transform.position - portal.position).magnitude > portalHitDistance)
        {



            if (objectToCarry.BeingCarried == true)
            {


                God.wren.interfaceUtils.SetObjectOfInterest(portal.transform);//


                God.interfaceTutorial.SetControllerHint(
                    InterfaceTutorial.ControllerHint.Release,
                    "Carry it to the Portal! ( ping to find it )"
                );
            }
            else
            {
                God.wren.interfaceUtils.SetObjectOfInterest(objectToCarry.transform);//


                God.interfaceTutorial.SetControllerHint(
                    InterfaceTutorial.ControllerHint.Carry,
                    "Hold R1 OR L1 to pick up and carry objects"
                );
            }

            //print("TOO FAR AWAY");
            yield return null;
        }

        print("PORTAL HIT111111111111111111111111111111111111111");


        God.wren.interfaceUtils.ReleaseObjectOfInterest();
        // Trigger Release
        God.wren.carrying.DropAllCarriedItems();//.ReleaseObject();



        //return null;



    }


}
