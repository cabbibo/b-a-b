using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class RevealGrass : Cycle
{

    public CurveMaterialController curveShownValue;
    public HairBasic hair;

    public float amount;

    public override void Bind(){
        hair.collision.BindFloat("_Amount" , () => amount);
    }

    public override void WhileLiving( float v ){
    //    print( curveShownValue.amount );

        amount = Mathf.Lerp( amount , curveShownValue.amount , .02f);
    }
}
