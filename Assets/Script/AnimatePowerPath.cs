using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class AnimatePowerPath : MonoBehaviour
{

    public float _Amount;

    MaterialPropertyBlock mpb;
    Renderer renderer;
    // Update is called once per frame
    void Update()
    {

        if( renderer == null ){
            renderer = GetComponent<Renderer>();
        }
        if( mpb == null ){

            mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock( mpb );

        }

        mpb.SetFloat("_Amount",_Amount);
        renderer.SetPropertyBlock( mpb );
        
    }
}
