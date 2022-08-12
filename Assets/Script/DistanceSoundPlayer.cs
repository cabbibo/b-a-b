using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceSoundPlayer : MonoBehaviour
{


    public AudioClip[] clips;
    // Start is called before the first frame update
    void Start()
    {

        for( var i = 0; i < clips.Length; i++ ){
            God.audio.MakeLoop( clips[i] );
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
