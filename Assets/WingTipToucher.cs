using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingTipToucher : MonoBehaviour
{



    public float distToGroundMax = 4;
    public float distToGroundMin = 1;

    public float minTime;
    public float maxTime;
    public Wren wren;

    public TrailRenderer trailRenderer;

    public float normDist;


    // Update is called once per frame
    void Update()
    {

        float distToGround = wren.physics.distToGround;
        float distToGroundNorm = Mathf.InverseLerp(distToGroundMin, distToGroundMax, distToGround);

        //        print(distToGroundNorm);
        normDist = distToGroundNorm;
        trailRenderer.time = Mathf.Lerp(minTime, maxTime, distToGroundNorm);
    }


}
