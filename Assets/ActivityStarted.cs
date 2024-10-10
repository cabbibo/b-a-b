using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class ActivityStarted : MonoBehaviour
{

    public Activity activity;
    public float lastHitTime;
    public bool alreadyInside;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider c)
    {
        //print("triggerEnter");
        //print(c.gameObject.name);

        if (!God.IsOurWren(c))
        {
            return;
        }

        if (alreadyInside)
        {
            return;
        }


        alreadyInside = true;

        // Cant accidently retrigger immediately
        if (Time.time - lastHitTime > 3)
        {
            activity.OnActivityInfoAreaEntered();
            lastHitTime = Time.time;

        }



    }

    public void OnTriggerExit(Collider c)
    {

        if (!God.IsOurWren(c))
        {
            return;
        }

        activity.OnActivityInfoAreaExited();


        alreadyInside = false;
        lastHitTime = Time.time;


    }

}
