using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TurnOnWrenTrails : MonoBehaviour
{


    public AudioSource audioSource;

    public float trailOnTime;

    public void OnEnable()
    {
        currentTimeToTurnOff = Time.time - 100;
    }


    // Update is called once per frame
    void Update()
    {

        if (Time.time > currentTimeToTurnOff)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0, .05f);
            if (God.wren != null)
            {
                God.wren.bird.rightWingTrailFromFeathers_gpu.emitting = 0;
                God.wren.bird.leftWingTrailFromFeathers_gpu.emitting = 0;
            }
        }
        else
        {
            if (God.wren != null)
            {
                God.wren.bird.rightWingTrailFromFeathers_gpu.emitting = 1;
                God.wren.bird.leftWingTrailFromFeathers_gpu.emitting = 1;
            }
            audioSource.volume = Mathf.Lerp(audioSource.volume, 1, .03f);
        }


    }

    public float currentTimeToTurnOff;
    public void AddToTrail()
    {


        currentTimeToTurnOff = Time.time + trailOnTime;


    }
}
