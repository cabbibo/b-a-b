using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


// Gets positions that are static based on 'Slides'
public class BaseCameraManager : MonoBehaviour
{
    // The importances of this thing
    public float weight;
    public float FOV;
    public OverallCameraManager overallManager;


    public virtual void WhileInUse()
    {

    }

    public void RequestPriority()
    {
        overallManager.RequestPriority(this);
    }

    public void RequestPriority(float speed)
    {
        overallManager.RequestPriority(this, speed);
    }

    public void ReleasePriority()
    {
        overallManager.ReleasePriority(this);
    }

    public void ReleasePriority(float speed)
    {
        overallManager.ReleasePriority(this, speed);
    }


}
