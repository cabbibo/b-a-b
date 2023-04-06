using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteAlways]
public class SetDataFromTimeline : MonoBehaviour
{

    public bool useIt;



    public PlayCutScene animation;

    public bool startEnd;

    public void OnEnable()
    {
        if (useIt)
        {
            if (startEnd)
                animation.SetStartValues();
            else
                animation.SetEndValues();
        }

    }

}
