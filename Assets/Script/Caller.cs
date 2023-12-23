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


        // Play in correct Location
        God.audio.Play(possibleClips[Random.Range(0, possibleClips.Length)], 1, 1, 0, 10, mixer, group, wren.transform.position);
        particles.Play();
        particles.transform.position = wren.transform.position;

    }
}
