using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;

public class StopAfterSomeSeconds : MonoBehaviour
{



    public float startTime;

    public float holdTime = 10;
    public float fadeTime = 4;

    public AudioClip clip;
    void OnEnable()
    {
        startTime = Time.time;
        God.audio.Play(clip);
    }

    public float val = 1;

    void Update()
    {


        val = 1;


        if (Time.time < startTime + holdTime)
        {
            val = 1;
        }
        else if (Time.time < startTime + holdTime + fadeTime)
        {
            val = 1 - (Time.time - (startTime + holdTime)) / fadeTime;
        }
        else
        {
            val = 0;
            Application.Quit();
        }


        God.postController._Fade = 1 - val * val;

        if (val == 0)
        {
            Application.Quit();
        }

    }

}
