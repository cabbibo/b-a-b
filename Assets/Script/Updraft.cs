using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Updraft : MonoBehaviour
{
    public float distanceCutoff = 200;
    public float multiplier = 3000;
    public float divisionMultiplier = 1;
    public float upLessForceMultiplier = 10;

    void FixedUpdate()
    {



        if (God.wren)
        {

            float upForce = 0;

            Vector3 dist = God.wren.transform.position - transform.position;

            dist = Vector3.Scale(dist, Vector3.left + Vector3.forward);
            float mag = dist.magnitude;



            if (mag < distanceCutoff) { upForce = (multiplier) / (divisionMultiplier * mag + 10); }


            upForce /= 1 + God.wren.transform.position.y * upLessForceMultiplier;

            if (!God.wren.state.onGround)
            {
                God.wren.physics.rb.AddForce(Vector3.up * upForce);
            }
        }


    }

}
