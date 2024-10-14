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

    public UnityEvent onSet;
    public UnityEvent onRelease;

    public string continueText = "X : Continue";
    public string cancelText = "O : Cancel";

    public string resetText = "Triangle : Reset";



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

        print(text);
        print(tmpText);

        if (tmpText == "" || tmpText == null)
        {
            tmpText = text;
        }


        tmpText = tmpText.Replace("\\n", "\n");
        print("Setting slide");
        print(text);
        print(tmpText);

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
    }






}
