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

    public bool justSwapped = false;

    private IEnumerator CheckForCarry()
    {

        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ping,
            "FIND"
        );


        God.interfaceTutorial.FadeFullGroupCoroutine(0, 1); //StartCoroutine(FadeGroup(groupContainer, 0, 1));

        God.wren.interfaceUtils.SetObjectOfInterest(objectToCarry.transform); //


        while (objectToCarry.BeingCarried == false)
        {

            UpdateCarryableFeedback();
            //  print("NOT CARRIED");
            yield return null;

        }


        portal.gameObject.SetActive(true);

        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Release,
            "Release L1 / R1 to drop the object"
        );


        while (objectToCarry.BeingCarried == true)
            yield return null;


        God.interfaceTutorial.SetControllerHint(
            InterfaceTutorial.ControllerHint.Ping,
            "Grab it again! ( ping to find it )"
        );


        while (objectToCarry.BeingCarried == false)
        {

            UpdateCarryableFeedback();

            //  print("NOT CARRIED");
            yield return null;

        }


        while ((objectToCarry.transform.position - portal.position).magnitude > portalHitDistance)
        {


            if (objectToCarry.BeingCarried == true)
            {


                God.wren.interfaceUtils.SetObjectOfInterest(portal.transform); //


                God.interfaceTutorial.SetControllerHint(
                    InterfaceTutorial.ControllerHint.Ping,
                    "FIND PORTAL"
                );


            }
            else
            {

                UpdateCarryableFeedback();


            }

            //print("TOO FAR AWAY");
            yield return null;
        }


        print("PORTAL HIT111111111111111111111111111111111111111");


        God.wren.interfaceUtils.ReleaseObjectOfInterest();
        // Trigger Release
        God.wren.carrying.DropAllCarriedItems(); //.ReleaseObject();


        //return null;


    }


    public void UpdateCarryableFeedback()
    {

        if (God.wren.carrying.carryableObjects.Contains(objectToCarry.gameObject) && justSwapped == false)
        {
            justSwapped = true;
            God.interfaceTutorial.SetControllerHint(
                InterfaceTutorial.ControllerHint.Carry,
                "L1 / R1 to Carry"
            );
        }
        else if (!God.wren.carrying.carryableObjects.Contains(objectToCarry.gameObject) &&
                    justSwapped == true)
        {
            justSwapped = false;
            God.wren.interfaceUtils.SetObjectOfInterest(objectToCarry.gameObject.transform); //
            God.interfaceTutorial.SetControllerHint(
                InterfaceTutorial.ControllerHint.Ping,
                "Get Close"
            );
        }
    }
}