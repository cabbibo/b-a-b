using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTutorialState : TutorialState
{


    public GameObject text;


    public override void OnComplete(){
        text.SetActive(true);
    }   

    public override void OnStart(){
        text.SetActive(false);
    }

}
