using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicCurve;

using static Unity.Mathematics.math;
using Unity.Mathematics;
using IMMATERIA;


public class WaterfallParticles : Particles
{

    public Curve curve;

    public float start;
    public float end;
    public float startWidth;
    public float endWidth;

 

    public override void Embody(){
                float[] values = new float[count * structSize ];
        int index = 0;

            float3 p1;
            float3 f1;
            float3 u1;
            float3 r1;
            float s1;
        for( int i = 0; i < count; i++ ){

            float val = UnityEngine.Random.Range(.00001f,.999f);
            float lengthAlongTube = Mathf.Lerp( start,end,val);
            curve.GetDataFromValueAlongCurve( lengthAlongTube  , out p1,out f1,out u1,out r1,out s1);

            float3 fPos = p1 + r1* Mathf.Lerp( startWidth,endWidth,val) * UnityEngine.Random.Range(-.5f,.5f)* s1;

            values[index++] = p1.x;
            values[index++] = p1.y;
            values[index++] = p1.z;

            values[index++] = f1.x;
            values[index++] = f1.y;
            values[index++] = f1.x;

            values[index++] = u1.x;
            values[index++] = u1.y;
            values[index++] = u1.x;

            values[index++] = fPos.x;
            values[index++] = fPos.y;
            values[index++] = fPos.z;

            values[index++] = u1.x;
            values[index++] = u1.y;
            values[index++] = u1.z;
            values[index++] = 0;

        }

        SetData(values);
    }
}
