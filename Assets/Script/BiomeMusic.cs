using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeMusic : MonoBehaviour
{




    public AudioClip[] clips;

    public AudioSource[] sources;
    public float[] sourceVolumes;


    public int currentActiveSource;


    public AudioClip neutralClip;
    public IslandController data;


    public float fadeOutSpeed = .04f;
    public float fadeInSpeed = .04f;

    // Update is called once per frame
    void Update()
    {


        for (int i = 0; i < sources.Length; i++)
        {


            // lerp out if we aren't the currentSource
            if (i != currentActiveSource)
            {
                sources[i].volume = Mathf.Lerp(sources[i].volume, 0, fadeOutSpeed);
                if (sources[i].volume < .01f) { sources[i].Stop(); }
            }
            else
            {
                // if we are the current source, make it pop!
                sources[i].volume = Mathf.Lerp(sources[i].volume, data.currentIsland.maxBiomeValue, fadeInSpeed);
            }


        }
    }

    public void OnBiomeChange(int oldBiome, int newBiome)
    {

        AudioClip clip;
        if (newBiome < 0)
        {
            clip = neutralClip;
        }
        else
        {
            clip = clips[newBiome];

        }



        currentActiveSource++;
        currentActiveSource %= sources.Length;
        sources[currentActiveSource].clip = clip;
        sources[currentActiveSource].Play();


    }








}
