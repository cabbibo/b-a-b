using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayLoop : MonoBehaviour
{


    public AudioClip[] clips;

    public AudioPlayer player;

    public float BPM;
    public float bars;
    public float beats;


    public Loop[] loops;

    public float[] volume;

    // Start is called before the first frame update
    void Start()
    {
        
        loops = new Loop[clips.Length];
        volume = new float[clips.Length];

        for(int i = 0; i < clips.Length; i++ ){
            loops[i] = player.MakeLoop( clips[i]);
            loops[i].GenerateLengthByBPMBarsAndBeats(BPM,bars,beats);
            
            //Just Start Playing, no fade in
            loops[i].SetFadeSpeed( 0 );
            
            loops[i].Start();

        }

    }

    // Update is called once per frame
    void Update()
    {
        
        for(int i = 0; i < clips.Length; i++ ){
            loops[i].currentVolume = volume[i];
        } 
               
    }



}
