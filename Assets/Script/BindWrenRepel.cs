using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using WrenUtils;


public class BindWrenRepel : Binder
{

    public float repelRadius = 1;
    public float repelForce = 10;
    public Transform t;

    public Vector3 vel;


    public override void Bind()
    {

        toBind.BindFloat("_RepelRadius", () => repelRadius);
        toBind.BindFloat("_RepelForce", () => repelForce);
        toBind.BindVector3("_WrenPos", () => t.position);
        toBind.BindVector3("_WrenDir", () => t.forward);
        toBind.BindVector3("_WrenVel", () => vel);

    }

    public override void WhileLiving(float v)
    {
        if (WrenUtils.God.wren != null)
        {
            t = WrenUtils.God.wren.transform;
            vel = WrenUtils.God.wren.physics.rb.velocity;
        }
        else
        {
            t = transform;
            vel = Vector3.forward * .01f; //(0, 0, .1);
        }


    }

}
