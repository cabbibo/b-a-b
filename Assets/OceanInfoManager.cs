using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    SampleHeightHelper sampleHeightHelperL;
    SampleHeightHelper sampleHeightHelperR;

    public bool closeToSurface;

    public LineRenderer lineRenderer;

    public float leftWingHeight;
    public float rightWingHeight;

    public float leftWingDisplacement;
    public float rightWingDisplacement;

    public float leftWingDistanceToSurface;
    public float rightWingDistanceToSurface;

    public Vector3 leftWingNormal;
    public Vector3 rightWingNormal;

    public Vector3 leftWingVel;
    public Vector3 rightWingVel;





    SampleHeightHelper sampleHeightHelperGround;

    void OnEnable()
    {

    }

    public Vector3 groundPosition;
    public Vector3 GetGroundPosition(Vector3 pos)
    {

        if (OceanRenderer.Instance == null)
        {
            return pos;
        }

        if (sampleHeightHelperGround == null)
        {
            sampleHeightHelperGround = new SampleHeightHelper();
        }

        sampleHeightHelperGround.Init(pos, 1);

        float displacement1;
        Vector3 normal1;
        Vector3 waterSurfaceVel1;

        sampleHeightHelperGround.Sample(out displacement1, out normal1, out waterSurfaceVel1);

        float h = displacement1;

        return new Vector3(pos.x, h, pos.z);


    }

    public bool hasOcean
    {
        get
        {
            return OceanRenderer.Instance != null;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        if (God.wren && OceanRenderer.Instance != null)
        {

            groundPosition = GetGroundPosition(God.wren.transform.position);

            // Only care if we are close  to ocean height other wise we dont need to sample!

            float dif = God.wren.transform.position.y - OceanRenderer.Instance.SeaLevel;
            if (dif > oceanHeightSampleCutoff)
            {
                closeToSurface = false;

                oHeight = height;
                oDistanceToSurface = distanceToSurface;

                height = OceanRenderer.Instance.SeaLevel;
                distanceToSurface = dif;
                leftWingHeight = oceanHeightSampleCutoff;
                rightWingHeight = oceanHeightSampleCutoff;

                leftWingNormal = Vector3.up;
                rightWingNormal = Vector3.up;
                leftWingVel = Vector3.zero;
                rightWingVel = Vector3.zero;


                return;
            }

            closeToSurface = true;



            waterJustHit = false;
            waterJustLeft = false;

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



                if (sampleHeightHelperL == null)
                {
                    sampleHeightHelperL = new SampleHeightHelper();
                }

                sampleHeightHelperL.Init(God.wren.physics.leftWing.position, 1);
                sampleHeightHelperL.Sample(out leftWingDisplacement, out leftWingNormal, out leftWingVel);

                leftWingHeight = leftWingDisplacement;
                leftWingDistanceToSurface = God.wren.physics.leftWing.position.y - leftWingHeight;


                if (sampleHeightHelperR == null)
                {
                    sampleHeightHelperR = new SampleHeightHelper();
                }


                sampleHeightHelperR.Init(God.wren.physics.leftWing.position, 1);
                sampleHeightHelperR.Sample(out rightWingDisplacement, out rightWingNormal, out rightWingVel);

                rightWingHeight = rightWingDisplacement;
                rightWingDistanceToSurface = God.wren.physics.rightWing.position.y - rightWingHeight;






                lineRenderer.SetPosition(0, God.wren.physics.rightWing.position);
                lineRenderer.SetPosition(1, new Vector3(God.wren.physics.rightWing.position.x, height, God.wren.physics.rightWing.position.z));



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

    public bool waterJustHit;
    public bool waterJustLeft;

    public UnityEvent OnEnterWaterEvent;
    public UnityEvent OnExitWaterEvent;

    public void OnEnterWater()
    {
        waterJustHit = true;
        // print("enter");
        OnEnterWaterEvent.Invoke();
    }

    public void OnExitWater()
    {
        waterJustLeft = true;
        // print("exit");
        OnExitWaterEvent.Invoke();
    }
}
