using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class ActivityLeavingHelper : MonoBehaviour
{

    public Activity activity;

    public void OnEnable()
    {
        activity.WhileLeavingEvent.AddListener(WhileLeaving);
        activity.OnActivityFullExitedEvent.AddListener(OnFullExitActivityArea);
        activity.OnActivityAreaEnteredEvent.AddListener(OnActivityAreaEntered);
        activity.OnActivityAreaExitedEvent.AddListener(OnActivityAreaExited);

    }

    public void SetLeavingText(string text)
    {
        God.wren.interfaceUtils.warningText.text.text = text;
    }


    public void OnActivityAreaEntered()
    {


        if (activity.doingActivity)
        {
            SetLeavingText("Returned");
            God.wren.interfaceUtils.warningText.Ping();
        }
    }

    public void OnActivityAreaExited()
    {
        if (activity.doingActivity)
        {
            SetLeavingText("Leaving Activity");
            God.wren.interfaceUtils.warningText.Ping();
        }
    }

    public void WhileLeaving(float v)
    {


        SetLeavingText("Leaving Acitivity in " + Mathf.Floor(((1 - v) * activity.timeAllowedWhileExitedActivityArea) * 100) / 100 + " seconds");
        God.wren.interfaceUtils.warningText.SetFade(Mathf.Sin(v * 100) + 1);


        //print("should be setting");

        //God.wren.interfaceUtils.AddPointer(mainPointOfInterest); // can call a bunch but shouldnt re add!
        God.wren.interfaceUtils.SetPointerFade(activity.mainPointOfInterest, Mathf.Sin(v * 100) + 1);


    }

    public void OnFullExitActivityArea()
    {
        SetLeavingText("Activity Exited");
        God.wren.interfaceUtils.warningText.Ping();
        God.wren.interfaceUtils.SetPointerFade(activity.mainPointOfInterest, 0);
    }

}
