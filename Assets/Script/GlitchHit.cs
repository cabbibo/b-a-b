using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
 


[ExecuteAlways]
public class GlitchHit : MonoBehaviour
{

    public float amount;
    public float speed;
    public float size;
    public float split;
    public float blend;
 
    public PostProcessVolume volume;
    private GlitchEffect glitch;

    private VolumeProfile profile;

    public bool inGlitch =false;
    public float glitchStartTime;
    public float glitchLength;


    public bool debug;
    void OnEnable()
    {
        volume = GetComponent<PostProcessVolume>();
         
        volume.profile.TryGetSettings(out glitch);
        inGlitch = false;
    }
 
    void Update()
    {


        if( inGlitch ){

            float v = (Time.time - glitchStartTime) / glitchLength;

            if( v > 1 ){ v=1; }
             

            float glitchStrength = Mathf.Pow(Mathf.Min(v * 40, (1-v)) ,4);//Mathf.Min( v , (1-v) * 4);

            //amount = glitchStrength * .007f;
            //speed = 0
            //size =2;


             
            glitch.amount.value =glitchStrength *  amount;
            if( v==1){ EndGlitch(); }

        }else{
            glitch.amount.value =0;
        }


        glitch.speed.value = speed;
        glitch.size.value = size;
        glitch.split.value = split;
        glitch.blend.value = blend;

        if( inGlitch == false && debug == true){
            StartGlitch();
        }

    }

    public void StartGlitch(){

        inGlitch = true;
        glitchStartTime = Time.time;

    }

    public void EndGlitch(){
        inGlitch = false;
    }
 
}

