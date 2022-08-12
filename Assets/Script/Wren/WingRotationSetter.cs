using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingRotationSetter : MonoBehaviour
{

    public string setterName;

    public float speed;
    public float lerpVal;

    public float[] upRots1;
    public float[] upRots2;
    public float[] upRots3;
    public float[] upRots4;


    public float[] sideRots1;
    public float[] sideRots2;
    public float[] sideRots3;
    public float[] sideRots4;


    public void SetTime(float t , float lr ,  ref float[] upRots, ref float[] sideRots ){


        for( int i = 0;  i < 4; i ++ ){
            float[] upRotsF = new float[]{
                lr * upRots1[i],
                lr * upRots2[i],
                lr * upRots3[i],
                lr * upRots4[i],
                lr * upRots1[i],
            };

            upRots[i] = Mathf.Lerp( upRots[i] , Helpers.cubicFromValue( t , upRotsF ) , lerpVal);

            float[] sideRotsF = new float[]{
                lr * sideRots1[i],
                lr * sideRots2[i],
                lr * sideRots3[i],
                lr * sideRots4[i],
                lr * sideRots1[i],
            };

            sideRots[i] = Mathf.Lerp( sideRots[i] , Helpers.cubicFromValue( t , sideRotsF ), lerpVal);

        }
  

    }
    

}
