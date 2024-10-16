using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;


// Has some text!
public class Slide : MonoBehaviour
{


    public string text;

    public string tmpText;

    public Transform lookTarget;
    public Transform textTarget;


    public bool orbit;
    public bool canCancel;
    public bool canReset;

    public bool setWrenPosition;
    public Transform wrenPosition;

    public UnityEvent onSet;
    public UnityEvent onRelease;

    public string continueText = "X : Continue";
    public string cancelText = "O : Cancel";

    public string resetText = "Triangle : Reset";

    public float FOV = 80;


    public float waitTime = 0;

    public bool lerp;
    public float lerpSpeed = 3;



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

        StartCoroutine(WaitToSet());




    }


    public IEnumerator WaitToSet()
    {

        yield return new WaitForSeconds(waitTime);
        ActuallySet();

    }


    public void ActuallySet()
    {

        if (setWrenPosition)
        {

            God.wren.Crash(wrenPosition.position);
            God.wren.physics.rb.isKinematic = true;
            God.wren.physics.rb.position = wrenPosition.position;
            God.wren.physics.rb.rotation = wrenPosition.rotation;
            God.wren.state.canTakeOff = false;

        }
        else
        {
            God.wren.physics.rb.isKinematic = true;
        }


        //        print(text);
        //        print(tmpText);

        if (tmpText == "" || tmpText == null)
        {
            tmpText = text;
        }


        tmpText = tmpText.Replace("\\n", "\n");
        //print("Setting slide");
        //print(text);
        //print(tmpText);

        God.cameraManager.slideManager.SetSlide(this);
        God.text.SetInfoText(tmpText, transform);

        if (canCancel)
        {
            if (canReset)
            {

                God.text.SetLargeText(continueText + " || " + cancelText + " || " + resetText, transform);
            }
            else
            {
                God.text.SetLargeText(continueText + " || " + cancelText, transform);
            }
        }
        else
        {

            if (canReset)
            {
                God.text.SetLargeText(continueText + " || " + resetText, transform);
            }
            else
            {
                God.text.SetLargeText(continueText, transform);
            }
        }


        onSet.Invoke();

    }

    public void Release()
    {
        God.cameraManager.slideManager.ReleaseSlide();
        God.text.SetInfoText("");
        God.text.SetLargeText("");
        onRelease.Invoke();

        God.wren.state.canTakeOff = true;
        God.wren.physics.rb.isKinematic = false;

    }






}
