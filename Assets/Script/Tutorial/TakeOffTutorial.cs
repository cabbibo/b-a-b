using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TakeOffTutorial : TutorialCoroutine
{

    public int numTimesTakenOff;
    public int numTimesTakenOffToComplete = 3;


    public bool oWrenOnGround;
    public bool wrenOnGround;

    public override IEnumerator TutorialSequence()
    {

        numTimesTakenOff = 0;




        yield return CheckForTakeOff();

        stateManager.OnTutorialEnd(this);


    }




    public InterfaceTutorial.ControllerHint tmpHint;
    public string tmpHintText;
    IEnumerator CheckForTakeOff()
    {



        God.interfaceTutorial.FadeFullGroupCoroutine(0, 1);//StartCoroutine(FadeGroup(groupContainer, 0, 1));


        while (numTimesTakenOff < numTimesTakenOffToComplete)
        {

            //print("hi");

            oWrenOnGround = wrenOnGround;
            wrenOnGround = God.wren.physics.onGround;




            if (oWrenOnGround == false && wrenOnGround == true)
            {
                tmpHint = God.interfaceTutorial.currentHint;
                tmpHintText = God.interfaceTutorial.currentHintText;


                God.interfaceTutorial.FadeInIfOff();
                //   yield return God.interfaceTutorial.WaitWithCheat(3);
                God.interfaceTutorial.SetControllerHint(
                          InterfaceTutorial.ControllerHint.TakeOff,
                          "Press the take off button to take off"
                      );


                print("LANDING");
            }


            if (oWrenOnGround == true && wrenOnGround == false)
            {

                God.interfaceTutorial.SetControllerHint(
                            tmpHint,
                            tmpHintText
                        );



                print("TAKING OFF");

                numTimesTakenOff++;
                God.interfaceTutorial.ShowProgress((float)numTimesTakenOff / (float)numTimesTakenOffToComplete);
            }



            God.interfaceTutorial.FadeFullGroupCoroutine(1, 0);//StartCoroutine(FadeGroup(groupContainer, 1, 0));

            yield return null;
        }


    }
}
