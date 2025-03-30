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






    public override void Initialize()
    {


        if (God.state.wrenCanDo.hasLearnedFlight == false)
        {
            flyingTutorial.JumpStartTutorial();
            foreach (GameObject go in postFlightTutorialObjects)
            {
                go.SetActive(false);
            }

            for (int i = 0; i < postPingTutorialObjects.Count; i++)
            {
                postPingTutorialObjects[i].SetActive(false);
            }
        }
        else
        {


            if (God.state.wrenCanDo.hasLearnedPing == false)
            {

                God.state.wrenCanDo.ping = true;
                print("STARTING PING TUTORIAL");
                God.wren.state.TakeOff();
                pingTutorial.JumpStartTutorial();//StartPingTutorial(); 

                foreach (GameObject go in postFlightTutorialObjects)
                {
                    go.SetActive(true);
                }

                foreach (GameObject go in postPingTutorialObjects)
                {
                    go.SetActive(false);
                }

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

                    //StartFullGame();
                }
            }




        }

    }


    public void OnPostWindTunnelBallHit()
    {

        print("STARTING HERE");


        pingTutorial.StartTutorial();
        takeOffTutorial.StartTutorial();

    }

    public void DoFlyingTutorialFinish()
    {


        God.state.wrenCanDo.hasLearnedFlight = true;
        God.state.wrenCanDo.ping = true;
        God.state.UpdateState();


        for (int i = 0; i < postFlightTutorialObjects.Count; i++)
        {
            postFlightTutorialObjects[i].SetActive(true);
        }

        God.wren.PhaseShift(windTunnelTeleportTarget);

        takeOffTutorial.StartTutorial();

    }

    public void DoPingTutorialFinish()
    {
        God.state.wrenCanDo.hasLearnedPing = true;
        God.state.UpdateState();
        for (int i = 0; i < postPingTutorialObjects.Count; i++)
        {
            postPingTutorialObjects[i].SetActive(true);
        }
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





    public override void OnTutorialEnd(TutorialCoroutine tutorial)
    {

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
        //Debug.Log("Progress: " + progress);
    }


}
