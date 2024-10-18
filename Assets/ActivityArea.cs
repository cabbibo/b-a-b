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

            print("ACTIVITYAREAENTERED CALLLLLLLLLLLLLLLLLLLLLLLLLEd");
            print(activity.inSlide);
            print(activity.insideActivityInfoArea);
            if (activity.inSlide == false)
            {
                print("ACTIVITYAREAENTERED");
                activity.OnActivityAreaEntered();
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (WrenUtils.God.IsOurWren(other))
        {


            if (activity.inSlide == false)
            {
                print("ACTIVITYAREAEXITED");
                activity.OnActivityAreaExited();
            }
        }
    }

}
