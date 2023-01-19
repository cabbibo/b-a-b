using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;
public class RandomAudioPlayer : MonoBehaviour
{
    // Start is called before the first frame update


    public AudioClip[] clips;

    public float timeBetweenClips;
    public float clipTimeRandomness;


    float oldTime = 0;
    float randomTime= 0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if( Time.time-oldTime > randomTime + timeBetweenClips ){
           
            oldTime = Time.time;

            God.audio.Play( clips );

            randomTime = Random.Range(0,clipTimeRandomness);
        }
        
    }
}
