using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialState : MonoBehaviour
{
    public bool instant;
    
    public UnityEvent OnStartEvent;
    public UnityEvent OnCompleteEvent;
    public virtual void OnStart(){

    }
    public virtual void OnComplete(){

    }

    
}
