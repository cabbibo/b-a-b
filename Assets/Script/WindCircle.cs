using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


public class WindCircle : MonoBehaviour
{

    public float radius;
    public float speed;

    public bool inWind = false;

    public void Update()
    {
        if (God.wren != null)
        {

            Vector3 dif = God.wren.transform.position - transform.position;
            dif.y = 0;
            if (dif.magnitude > radius)
            {
                God.wren.physics.AddForce(dif.normalized * speed);
                inWind = true;
                // TODO add wind sound / particles

            }
            else
            {
                inWind = false;
            }

        }

    }


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }



}
