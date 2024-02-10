using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingTipToucher : MonoBehaviour
{



    public float distToGroundMax = 4;
    public float distToGroundMin = 1.1f;

    public float minTime;
    public float maxTime;
    public Wren wren;

    public TrailRenderer trailRenderer;

    public float normDist;

    public float lerpSpeed;



    // Update is called once per frame
    void Update()
    {

        float distToGround = wren.physics.distToGround;
        float distToGroundNorm = Mathf.InverseLerp(distToGroundMin, distToGroundMax, distToGround);

        //        print(distToGroundNorm);
        normDist = Mathf.Lerp(normDist, distToGroundNorm, lerpSpeed);
        trailRenderer.time = Mathf.Lerp(minTime, maxTime, distToGroundNorm);
    }


}
