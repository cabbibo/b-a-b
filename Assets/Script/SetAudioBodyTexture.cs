using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IMMATERIA {
public class SetAudioBodyTexture : Cycle
{ 
    public AudioListenerTexture audioForm;
    public Body body;
    

    public override void OnLive(){
      body.mpb.SetTexture("_AudioMap" , audioForm.texture );
    }


  }
}