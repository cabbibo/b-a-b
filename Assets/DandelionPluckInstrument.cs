using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class DandelionPluckInstrument : Cycle
{



    public DataForm pluckForm;
  
    public int[] steps;
    public AudioClip[] clips;

    public float oNumPlucked;

    public float lastPlayTime;
    public string mixerName;

    public float pitchMultiplier;
    public float volumeMultiplier;

    public override void OnLive()
    {
        lastPlayTime = Time.time;
    }


    public float minPlayTime;
    public override void WhileLiving(float v ){
        float numPlucked = pluckForm.values[0];

        float newPlucks = numPlucked - oNumPlucked;

        oNumPlucked = numPlucked;



        if( newPlucks > 0 && Time.time - lastPlayTime > minPlayTime){
        
            AudioClip clip = clips[Random.Range(0,clips.Length)];
            int step = steps[Random.Range(0,steps.Length)];

          //  AudioMixer mix = WrenUtils.God.audio.defaultMixer;
            string groupName = mixerName;
            WrenUtils.God.audio.Play( clip , numPlucked * pitchMultiplier  , numPlucked * volumeMultiplier ,0,1 , WrenUtils.God.audio.defaultMixer , groupName );

            lastPlayTime = Time.time;

        }

    }

}
