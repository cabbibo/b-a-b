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

    public void Call()
    {


        // Play in correct Location
        God.audio.Play(possibleClips[Random.Range(0, possibleClips.Length)], 1, 1, 0, 10, mixer, group, God.wren.transform.position);
        particles.Play();
        particles.transform.position = God.wren.transform.position;

    }
}
