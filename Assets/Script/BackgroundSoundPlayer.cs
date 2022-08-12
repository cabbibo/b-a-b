using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSoundPlayer : MonoBehaviour
{

    public float speed;
    public float speedRandomness;

    public float volume;
    public float volumeRandomness;

    private float lastTimePlayed;

    private float lastTimeRandomness;

    public void OnEnable(){
        lastTimePlayed = Time.time;
        lastTimeRandomness = Random.Range(-speedRandomness,speedRandomness) * speed;

    }
    public void Update(){

        if(Time.time - lastTimePlayed > speed + lastTimeRandomness ){


///            print("playing Audio");
            God.audio.Play( God.sounds.backgroundSounds , volume +  Random.Range(-volumeRandomness,volumeRandomness) * volume , "background" );

            lastTimePlayed = Time.time;
            lastTimeRandomness = Random.Range(-speedRandomness,speedRandomness) * speed;

        }



    }


}
