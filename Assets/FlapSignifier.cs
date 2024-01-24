using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class FlapSignifier : MonoBehaviour
{

    public ParticleSystem psL;
    public ParticleSystem psR;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


        if (God.wren)
        {
            print(God.wren.physics.flapVelL);
            psL.transform.position = God.wren.bird.leftWing.transform.position + God.wren.bird.leftWing.transform.right * -.5f;
            psR.transform.position = God.wren.bird.rightWing.transform.position + God.wren.bird.leftWing.transform.right * .5f; ;
            psL.transform.rotation = God.wren.bird.leftWing.transform.rotation;
            psR.transform.rotation = God.wren.bird.rightWing.transform.rotation;

            if (God.wren.physics.flapVelL < -.5)
            {
                psL.Emit((int)(100 * Mathf.Pow(Mathf.Abs(God.wren.physics.flapVelL + .5f), 2)));

            }

            if (God.wren.physics.flapVelR < -.5)
            {
                psR.Emit((int)(100 * Mathf.Pow(Mathf.Abs(God.wren.physics.flapVelR + .5f), 2)));
            }
        }


    }
}
