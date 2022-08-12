using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalAudio : MonoBehaviour
{


    
    public AudioClip[] clips;

    public float[] noiseSize;

    
    // Start is called before the first frame update
    void Start()
    {
          for(int i = 0; i < clips.Length; i++ ){
              //  print( Mathf.PerlinNoise( p.x * noiseSize[i] * .0001f , p.y  * noiseSize[i] * .0001f));
                //God.audioPlayer.PlayLoop( clips[i] , i );// =   (Mathf.PerlinNoise( p.x * noiseSize[i] * .0001f , p.y  * noiseSize[i] * .0001f) + 1)/2;   
           }
    }

    // Update is called once per frame
    void Update()
    {

        if( God.wren ){
            Vector2 p = new Vector2(God.wren.transform.position.x, God.wren.transform.position.z);

            for(int i = 0; i < clips.Length; i++ ){
                print( Mathf.PerlinNoise( p.x * noiseSize[i] * .0001f , p.y  * noiseSize[i] * .0001f));
              //  God.audioPlayer.loopSources[i].volume =   Mathf.PerlinNoise( p.x * noiseSize[i] * .0001f , p.y  * noiseSize[i] * .0001f);   
           }
        }
        
    }
}
