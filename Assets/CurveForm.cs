using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
using Unity.Mathematics;
using IMMATERIA;

using MagicCurve;

public class CurveForm : Form
{
    
    public Curve curve;



    // float3 point
    // float3 Directions
    // float3 normal
    // float3 tangent
    // float2 id
    // float  width
    // float dist
    
    public override void SetStructSize(){
        print("ehlso");
        structSize = 16;
    }

    public override void SetCount(){
        //count = curve.bakedDists.Length;
    }


    public override void Embody(){

        float[] values = new float[count * structSize ];
        int index = 0;

        for( int i = 0; i < count; i++ ){


            float lengthAlongTube = (float)i/(float)count;

            float3 p1;
            float3 f1;
            float3 u1;
            float3 r1;
            float s1;
            
            curve.GetDataFromValueAlongCurve( lengthAlongTube  , out p1,out f1,out u1,out r1,out s1);


        
           //f1 = normalize(f1);
           //u1 = normalize(u1);
           //r1 = normalize(r1);

            values[index++] = p1.x;
            values[index++] = p1.y;
            values[index++] = p1.z;

            values[index++] = f1.x;
            values[index++] = f1.y;
            values[index++] = f1.x;

            values[index++] = u1.x;
            values[index++] = u1.y;
            values[index++] = u1.x;

            values[index++] = r1.x;
            values[index++] = r1.y;
            values[index++] = r1.x;

            values[index++] = i;
            values[index++] = lengthAlongTube;

            values[index++] = s1;
            values[index++] = 0;

        }

        SetData(values);

    }



}
