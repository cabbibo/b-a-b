using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
public class ActivityRadialStarter : MonoBehaviour
{


    public Activity activity;

    public float radius;

    public float dist;
    public float oDist;



    // Update is called once per frame
    void Update()
    {

        if (God.wren != null)
        {
            oDist = dist;
            dist = Vector3.Distance(God.wren.transform.position, transform.position);
        }

        if (dist <= radius && oDist > radius)
        {

            activity.OnActivityAreaEntered();
        }

        if (dist > radius && oDist <= radius)
        {

            activity.OnActivityAreaExited();
        }


    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);

    }



}
