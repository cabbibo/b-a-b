using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TutorialIslandTutorialStateManager : TutorialStateManager
{


    public FlyingTutorial flyingTutorial;

    public PingTutorial pingTutorial;

    public TakeOffTutorial takeOffTutorial;

    // public TakeOffTutorial takeOffTutorial;

    public List<GameObject> postFlightTutorialObjects;
    public List<GameObject> postPingTutorialObjects;



    public Transform windTunnelTeleportTarget;



    public Portal portal;




    public override void Initialize()
    {


        if (God.state.wrenCanDo.hasLearnedFlight == false)
        {
            flyingTutorial.JumpStartTutorial();
            foreach (GameObject go in postFlightTutorialObjects)
            {
                go.SetActive(false);
            }


            SetPreFlightState();
            SetPrePingState();
        }
        else
        {


            if (God.state.wrenCanDo.hasLearnedPing == false)
            {

                God.state.wrenCanDo.ping = true;
                print("STARTING PING TUTORIAL");
                God.wren.state.TakeOff();
                pingTutorial.JumpStartTutorial();//StartPingTutorial(); 

                SetPostFlightState();
                SetPrePingState();
                if (God.state.wrenCanDo.hasLearnedTakeOff == false)
                {
                    takeOffTutorial.StartTutorial();
                }

            }
            else
            {

                God.state.wrenCanDo.ping = true;

                SetPostFlightState();
                SetPostPingState();


                if (God.state.wrenCanDo.hasLearnedTakeOff == false)
                {
                    takeOffTutorial.StartTutorial();
                }
                else
                {

                    //StartFullGame();
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
        for (int i = 0; i < postFlightTutorialObjects.Count; i++)
        {
            postFlightTutorialObjects[i].SetActive(false);
        }

    }

    public void SetPrePingState()
    {
        for (int i = 0; i < postPingTutorialObjects.Count; i++)
        {
            postPingTutorialObjects[i].SetActive(false);
        }

    }


    public void SetPostFlightState()
    {
        for (int i = 0; i < postFlightTutorialObjects.Count; i++)
        {
            postFlightTutorialObjects[i].SetActive(true);
        }

    }

    public void SetPostPingState()
    {
        for (int i = 0; i < postPingTutorialObjects.Count; i++)
        {
            postPingTutorialObjects[i].SetActive(true);
        }
        portal.SetPortalFull();
    }

    public void DoPingTutorialFinish()
    {
        God.state.wrenCanDo.hasLearnedPing = true;
        God.state.UpdateState();

        SetPostPingState();
    }


    public void DoTakeoffTutorialFinish()
    {

        God.state.wrenCanDo.hasLearnedTakeOff = true;
        God.state.UpdateState();


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
