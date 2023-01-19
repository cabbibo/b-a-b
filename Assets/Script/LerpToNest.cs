using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 using static Unity.Mathematics.math;
using Unity.Mathematics;
using WrenUtils;

public class LerpToNest : MonoBehaviour
{

    public float lerpSpeed;

    public Transform start;
    public Transform nest;

    public float upSize;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public float lerpStartTime;
    public bool lerping = true;

    // Update is called once per frame
    void Update()
    {

        if( lerping ){

            float v = (Time.time - lerpStartTime)/lerpSpeed;
            transform.position = cubicCurve( v , start.position , start.position + start.up * upSize ,  nest.position + nest.up * upSize ,nest.position );

            if( v == 1 ){
                EndLerp();
            }
        }
    }

    public void StartLerp(){
        lerping = true;
        lerpStartTime = Time.time;
        nest = God.wren.beacon.transform;

    }

    public void SetAtEnd(){
        
        transform.position = God.wren.beacon.transform.position;

    }

      public void SetAtStart(){
        
        transform.position = start.position;

    }

    public void EndLerp(){

        lerping = false;

    }

    float3 cubicCurve( float t , float3  c0 , float3 c1 , float3 c2 , float3 c3 ){

        float s  = 1 - t;

        float3 v1 = c0 * ( s * s * s );
        float3 v2 = 3 * c1 * ( s * s ) * t;
        float3 v3 = 3 * c2 * s * ( t * t );
        float3 v4 = c3 * ( t * t * t );

        float3 value = v1 + v2 + v3 + v4;

        return value;

}

}
