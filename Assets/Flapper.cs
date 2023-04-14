using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Audio;

public class Flapper : MonoBehaviour
{

    public Wren wren;

    public AudioClip[] openClips;
    public AudioClip[] closeClips;



    public float leftFlap;
    public float rightFlap;

    public float oLeftFlap;
    public float oRightFlap;

    public float pitchMultiplier = 1;
    public float volumeMultiplier = 1;

    public string group = "reverby";
    public AudioMixer mixer;

    // Update is called once per frame
    void Update()
    {

        if (wren.input.left2 > wren.input.o_left2)
        {
            float d = wren.input.left2 - wren.input.o_left2;
            print(d * volumeMultiplier);
            print(d * pitchMultiplier);
            God.audio.Play(closeClips, d * volumeMultiplier, d * pitchMultiplier, mixer, group);
        }

        if (wren.input.left2 < wren.input.o_left2)
        {
            float d = -(wren.input.left2 - wren.input.o_left2);
            print(d);
            God.audio.Play(closeClips, d * volumeMultiplier, d * pitchMultiplier, mixer, group);
        }



    }
}
