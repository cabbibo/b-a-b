using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

using UnityEngine.Audio;
public class Caller : MonoBehaviour
{

    public Wren wren;


    public ParticleSystem particles;
    public AudioClip[] possibleClips;

    public AudioMixer mixer;
    public string group;
    // Update is called once per frame
    void Update()
    {

        if (wren.input.o_circle < .5 && wren.input.circle > .5)
        {
            Call();
        }

    }

    public void Call()
    {

        God.audio.Play(possibleClips, wren.input.circle, wren.input.circle, mixer, group);
        particles.Play();
        particles.transform.position = wren.transform.position;

    }
}
