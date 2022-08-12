
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class RingSetShowOff : MonoBehaviour
{


    public Camera main;
    public LerpTo lerpTo;
    public GlitchHit glitch;
    public Camera cutSceneCamera;
    public PlayableDirector director;

    // Start is called before the first frame update
    void OnEnable()
    {
        director = GetComponent<PlayableDirector>();
        director.played += Director_Played;
        director.stopped += Director_Stopped;
    }

    void TurnOn(){
        Play();
    }

    
    private void Director_Stopped( PlayableDirector d ){

        glitch.StartGlitch();
        lerpTo.enabled = true;
    }


    private void Director_Played( PlayableDirector d ){
    

        glitch.StartGlitch();
        lerpTo.enabled = false;
        

    }
    public void Play(){



        director.Play();
    }

    public void OnEnd(){

    }
}
