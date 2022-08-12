using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using MagicCurve;

public class PlaceParticlesOnSceneUsingCurve : Form
{
    public Curve curve;

    public float widthRange;

    public override void SetStructSize(){
        structSize = 16;
    }
    public override void Embody(){

        print("h");

        int index = 0;

        float[] vals = new float[count * structSize ];

        for( int i = 0; i < count; i++ ){

            float v = (float)i / (float)count;
            float side = Random.Range( 0,.9999f);
            Vector3 startPos = curve.GetPositionFromValueAlongCurve(v);
            Vector3 startNor = curve.GetUpFromValueAlongCurve(v);
            Vector3 startTan = curve.GetRightFromValueAlongCurve(v);

             RaycastHit hit;


        // Ignore ourselves for collision hit
        var layerMask = (1 << 10);
        layerMask = ~layerMask;

        float fWidth = (.5f - Mathf.Abs(.5f - v)) * 2 + Random.Range( 0.0f, .1f);
        if( Physics.Raycast(startPos + startTan * (side-.5f) * widthRange * fWidth , -startNor, out hit, 100000, layerMask)){
            startPos = hit.point;
            startNor = hit.normal;
           // startTan = hit.tangent;

        }else{
            print("NO HIT");
        }

            vals[index++] = startPos.x;
            vals[index++] = startPos.y;
            vals[index++] = startPos.z;

            
            vals[index++] = 0;
            vals[index++] = 0;
            vals[index++] = 0;

            
            vals[index++] = startNor.x;
            vals[index++] = startNor.y;
            vals[index++] = startNor.z;

            
            
            vals[index++] = startTan.x;
            vals[index++] = startTan.y;
            vals[index++] = startTan.z;


            
            vals[index++] = v;
            vals[index++] = side;
            vals[index++] = 1;
            vals[index++] = 1;




        }

        SetData(vals);


    }
}
