using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

public class RingsInARow : MonoBehaviour
{


    public List<Booster> rings;


    public float threadingStartTime;
    public bool threading;

    public int currentRing;

    public TrailRenderer trail;

    public float totalTimeAllowed = 5;

    public UnityEvent CompletedThreadingEvent;


    public void OnRingHit(Booster ring)
    {

        print("RING HIT");
        print(ring);
        for (int i = 0; i < rings.Count; i++)
        {
            if (rings[i] == ring)
            {


                print("FOUND RING");
                if (i == 0)
                {
                    StartThreading();
                    print("FIRST RING");
                }
                else
                {
                    print("NOT FIRST RING");

                    if (i == currentRing + 1)
                    {
                        currentRing = i;
                        RightRingHit();
                        if (currentRing == rings.Count - 1)
                        {
                            FinishThreading();
                        }
                    }
                    else
                    {
                        WrongRingHit();
                    }
                }

                break;

            }
        }
    }

    public void StartThreading()
    {
        God.particleSystems.Emit(God.particleSystems.smallSuccessParticleSystem, God.wren.transform.position, 1);
        threadingStartTime = Time.time;
        threading = true;
        trail.Clear();
        trail.time = totalTimeAllowed;
        trail.enabled = true;

        currentRing = 0;

    }
    public void StopThreading()
    {

        trail.Clear();
        trail.enabled = false;

        threading = false;
        currentRing = -1;

    }


    public void RightRingHit()
    {
        God.feedbackSystems.DoSmallSuccess();
        print("RIGHT RING HIT");
    }

    public void WrongRingHit()
    {
        print("WRONG RING HIT");

        God.feedbackSystems.DoSmallFail();
        God.particleSystems.Emit(God.particleSystems.smallFailParticleSystem, God.wren.transform.position, 100);
        StopThreading();
    }

    public void FinishThreading()
    {
        God.feedbackSystems.DoLargeSuccess();
        print("FINISH THREADING");
        CompletedThreadingEvent.Invoke();
        StopThreading();
    }


    public void Update()
    {


        if (threading)
        {

            trail.enabled = true;
            trail.gameObject.transform.position = God.wren.transform.position;
            if (Time.time - threadingStartTime > totalTimeAllowed)
            {
                StopThreading();
            }
        }

    }




}
