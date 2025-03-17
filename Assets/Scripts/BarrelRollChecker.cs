using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BarrelRollChecker : MonoBehaviour
{

    public int numCrystalsOnBarrelRoll;

    public float barrelRollSpeed;



    public float angleDelta;
    public float lerpedAngle;
    public float currentAngle;
    public float oldAngle;


    // Start is called before the first frame update
    void Start()
    {

    }


    public bool rolling;
    public int direction;
    // Update is called once per frame
    void Update()
    {

        if (God.wren)
        {

            oldAngle = currentAngle;
            currentAngle = God.wren.transform.eulerAngles.z;



            if (currentAngle > 180 && currentAngle < 270 && oldAngle < 180 && oldAngle > 90)
            {
                StartBarrelRollRight();
            }

            if (currentAngle < 180 && currentAngle > 90 && oldAngle > 180 && oldAngle < 270)
            {
                StartBarrelRollLeft();
            }

            /*  if (currentAngle < 180 && oldAngle > 180)
              {
                  DoBarrelRoll();
              }*/


            if (rolling && direction == 1)
            {
                if (currentAngle > 300)
                {
                    DoBarrelRoll();
                    rolling = false;
                }
            }

            if (rolling && direction == -1)
            {
                if (currentAngle < 60)
                {
                    DoBarrelRoll();
                    rolling = false;
                }
            }
        }

    }


    public void StartBarrelRollLeft()
    {
        direction = -1;
        rolling = true;
    }

    public void StartBarrelRollRight()
    {
        direction = 1;
        rolling = true;
    }
    public void DoBarrelRoll()
    {
        God.wren.shards.CollectShards(numCrystalsOnBarrelRoll, 0, God.wren.transform.position);
        God.wren.physics.Boost();
    }
}
