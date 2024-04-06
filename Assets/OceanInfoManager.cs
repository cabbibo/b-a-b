using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;
using WrenUtils;

public class OceanInfoManager : MonoBehaviour
{

    public float height;
    public float distanceToSurface;
    public Vector3 normal;
    public Vector3 displacement;
    public Vector3 waterSurfaceVel;

    public float oHeight;
    public float oDistanceToSurface;

    public float oceanHeightSampleCutoff;

    SampleHeightHelper sampleHeightHelper;

    public bool closeToSurface;

    public LineRenderer lineRenderer;

    public float leftWingHeight;
    public float rightWingHeight;

    public Vector3 leftWingNormal;
    public Vector3 rightWingNormal;

    public Vector3 leftWingVel;
    public Vector3 rightWingVel;


    void OnEnable()
    {

    }
    // Update is called once per frame
    void Update()
    {

        if (God.wren)
        {

            // Only care if we are close  to ocean height other wise we dont need to sample!

            float dif = God.wren.transform.position.y - OceanRenderer.Instance.SeaLevel;
            if (dif > oceanHeightSampleCutoff)
            {
                closeToSurface = false;

                return;
            }

            closeToSurface = true;





            if (OceanRenderer.Instance != null)
            {

                if (sampleHeightHelper == null)
                {
                    sampleHeightHelper = new SampleHeightHelper();
                }


                oHeight = height;
                oDistanceToSurface = distanceToSurface;
                sampleHeightHelper.Init(God.wren.transform.position, 10);
                sampleHeightHelper.Sample(out displacement, out normal, out waterSurfaceVel);
                height = OceanRenderer.Instance.SeaLevel + displacement.y;
                distanceToSurface = God.wren.transform.position.y - height;

                //       God.wren.transform.position = new Vector3(God.wren.transform.position.x, height, God.wren.transform.position.z);



                //                print("hello");

                sampleHeightHelper.Init(God.wren.physics.leftWing.position, 1);
                sampleHeightHelper.Sample(out leftWingHeight, out leftWingNormal, out leftWingVel);

                leftWingHeight = OceanRenderer.Instance.SeaLevel + leftWingHeight;
                leftWingHeight = God.wren.physics.leftWing.position.y - leftWingHeight;


                sampleHeightHelper.Init(God.wren.physics.rightWing.position, 1);
                sampleHeightHelper.Sample(out rightWingHeight, out rightWingNormal, out rightWingVel);

                rightWingHeight = OceanRenderer.Instance.SeaLevel + rightWingHeight;
                rightWingHeight = God.wren.physics.rightWing.position.y - rightWingHeight;


                lineRenderer.SetPosition(0, God.wren.transform.position);
                lineRenderer.SetPosition(1, new Vector3(God.wren.transform.position.x, height, God.wren.transform.position.z));



                if (distanceToSurface < 0 && oDistanceToSurface >= 0)
                {
                    OnEnterWater();
                }

                if (distanceToSurface >= 0 && oDistanceToSurface < 0)
                {
                    OnExitWater();
                }




            }

        }

    }

    public void OnEnterWater()
    {
        // print("enter");
    }

    public void OnExitWater()
    {
        // print("exit");
    }
}
