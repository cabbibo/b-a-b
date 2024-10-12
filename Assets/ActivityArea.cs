using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityArea : MonoBehaviour
{

    public Activity activity;

    public void OnTriggerEnter(Collider other)
    {

        if (WrenUtils.God.IsOurWren(other))
        {
            activity.OnActivityAreaEntered();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (WrenUtils.God.IsOurWren(other))
        {
            activity.OnActivityAreaExited();
        }
    }

}
