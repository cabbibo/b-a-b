using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WrenUtils;
public class UIImageTutorialState : TutorialState
{


    public Image image;


    public override void OnStart(){
        image.enabled = true;
       // God.audio.Play(God.sounds.tutorialSectionStartSound);
        OnStartEvent.Invoke();
        
    }
    public override void OnComplete(){
        image.enabled = false;
        God.audio.Play(God.sounds.tutorialSuccessSound);
        OnCompleteEvent.Invoke();
    }   


}
