using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;
public class PlaySound : MonoBehaviour
{

    public AudioClip clip;
    
    public void Play(){
        God.audio.Play( clip );
    }
}
