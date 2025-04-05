using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TutorialIslandTutorialStateManager : TutorialStateManager
{


    public FlyingTutorial flyingTutorial;

    public PingTutorial pingTutorial;

    public TakeOffTutorial takeOffTutorial;

    public CarryTutorial carryTutorial;

    // public TakeOffTutorial takeOffTutorial;


    public Portal portal;


    public Transform windTunnelTeleportTarget;







    public override void Initialize()
    {


        flyingTutorial.CheckState();
        pingTutorial.CheckState();
        takeOffTutorial.CheckState();
        carryTutorial.CheckState();



        if (God.state.wrenCanDo.hasLearnedFlight == false)
        {
            flyingTutorial.JumpStartTutorial();


        }
        else
        {


            if (God.state.wrenCanDo.hasLearnedPing == false)
            {

                God.state.wrenCanDo.ping = true;
                print("STARTING PING TUTORIAL");
                God.wren.state.TakeOff();
                pingTutorial.JumpStartTutorial();//StartPingTutorial(); 

                if (God.state.wrenCanDo.hasLearnedTakeOff == false)
                {
                    takeOffTutorial.StartTutorial();
                }

            }
            else
            {

                God.state.wrenCanDo.ping = true;


                if (God.state.wrenCanDo.hasLearnedTakeOff == false)
                {
                    takeOffTutorial.StartTutorial();
                }
                else
                {


                    if (God.state.wrenCanDo.hasLearnedCarry == false)
                    {
                        carryTutorial.JumpStartTutorial();
                    }
                    else
                    {
                        SetPostCarryState();
                    }
                }
            }




        }

    }


    public void OnPostWindTunnelBallHit()
    {

        print("STARTING HERE");


        //pingTutorial.StartTutorial();
        takeOffTutorial.StartTutorial();

    }

    public void DoFlyingTutorialFinish()
    {


        God.state.wrenCanDo.hasLearnedFlight = true;
        God.state.wrenCanDo.ping = true;
        God.state.UpdateState();

        SetPostFlightState();

        God.wren.PhaseShift(windTunnelTeleportTarget);

        takeOffTutorial.StartTutorial();

    }


    public void SetPreFlightState()
    {


    }

    public void SetPrePingState()
    {


    }


    public void SetPostFlightState()
    {


    }

    public void SetPostPingState()
    {

        portal.SetPortalOff();


    }


    public void SetPreCarryState()
    {
        //carryTutorial.gameObject.SetActive(false);
    }

    public void SetPostCarryState()
    {
        carryTutorial.gameObject.SetActive(true);
        portal.SetPortalFull();
    }


    public void DoPingTutorialFinish()
    {
        God.state.wrenCanDo.hasLearnedPing = true;
        God.state.UpdateState();

        SetPostPingState();


        if (God.state.wrenCanDo.hasLearnedCarry == false)
        {
            carryTutorial.StartTutorial();
        }
        else
        {
            carryTutorial.CheckState();
        }
    }


    public void DoTakeoffTutorialFinish()
    {

        God.state.wrenCanDo.hasLearnedTakeOff = true;
        God.state.UpdateState();

        if (God.state.wrenCanDo.hasLearnedCarry == false && God.state.wrenCanDo.hasLearnedPing == true)
        {
            carryTutorial.JumpStartTutorial();
        }
        else
        {
            carryTutorial.CheckState();
        }


    }


    public void DoCarryTutorialFinish()
    {

        print("DOING CARRY TUTORIAL FINISH");

        /*
        God.state.wrenCanDo.hasLearnedCarry = true;
        God.state.UpdateState();

        SetPostCarryState();
        */


    }


    public void TeleportToWindTunnel()
    {

        God.wren.PhaseShift(windTunnelTeleportTarget);

    }


    public void Update()
    {
        //        print(flyingTutorial.tutSequence);
        //       print(pingTutorial.tutSequence);
        //      print(takeOffTutorial.tutSequence);
    }





    public override void OnTutorialEnd(TutorialCoroutine tutorial)
    {


        print("OnTutorialEnd");
        if (tutorial is FlyingTutorial)
        {
            DoFlyingTutorialFinish();
        }
        else if (tutorial is PingTutorial)
        {
            DoPingTutorialFinish();
        }
        else if (tutorial is TakeOffTutorial)
        {
            DoTakeoffTutorialFinish();
        }
        else if (tutorial is CarryTutorial)
        {
            DoCarryTutorialFinish();
            //SetPostCarryState();
        }
    }


    /*

        TODO 

        Start the ping tutorial when the player first uses the take off 

    */

    public override void OnProgress(TutorialCoroutine tutorial, float progress)
    {

        print("OnProgress: " + progress);
        //Debug.Log("Progress: " + progress);
        if (tutorial is TakeOffTutorial)
        {


            if (pingTutorial.hasStarted == false && God.state.wrenCanDo.hasLearnedPing == false)
            {
                pingTutorial.StartTutorial();
            }

        }
        else if (tutorial is PingTutorial)
        {
            //Debug.Log("Ping Progress: " + progress);
            if (progress >= 1f)
            {
                //Debug.Log("Ping Complete!");
                // Do something when the ping tutorial is complete
            }
        }

    }


}
