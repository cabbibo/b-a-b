using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


using TMPro;
public class MenuOption : MonoBehaviour
{

    
    public UnityEvent[] selectEvents;
    public UnityEvent[] deselectEvents;
    public UnityEvent[] dLeftEvents;
    public UnityEvent[] dRightEvents;

    public Color onColor;
    public Color offColor;


    public TextMeshProUGUI text;

    public void OnEnable(){
        text = GetComponent<TextMeshProUGUI>();
    }
    public void Select(){
        for( var i = 0; i < selectEvents.Length; i++ ){
            selectEvents[i].Invoke();
        }
    }

    public void Deselect(){
        for( var i = 0; i < deselectEvents.Length; i++ ){
            deselectEvents[i].Invoke();
        }
    }

    public void Activate(){
        text.color = onColor;
    }
    
    public void Deactivate(){
        text.color = offColor;
    }


    public void DLeft(){
        for( var i = 0; i < dLeftEvents.Length; i++ ){
            dLeftEvents[i].Invoke();
        }
    }

    public void DRight(){
        for( var i = 0; i < dRightEvents.Length; i++ ){
            dRightEvents[i].Invoke();
        }
    }

}
