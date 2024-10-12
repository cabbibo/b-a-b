using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;


// Has some text!
public class Slide : MonoBehaviour
{


    public string text;

    public Transform lookTarget;
    public Transform textTarget;


    public bool orbit;
    public bool canCancel;

    public UnityEvent onSet;
    public UnityEvent onRelease;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set()
    {

        God.cameraManager.slideManager.SetSlide(this);
        God.text.SetInfoText(text, transform);

        if (canCancel)
        {
            God.text.SetLargeText(" X : Continue || O : Exit", transform);
        }
        else
        {
            God.text.SetLargeText(" X : Continue", transform);
        }


        onSet.Invoke();

    }

    public void Release()
    {
        God.cameraManager.slideManager.ReleaseSlide();
        God.text.SetInfoText("");
        God.text.SetLargeText("");
        onRelease.Invoke();
    }


}
