using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class HueController : MonoBehaviour
{

    public float hueStart;
    public float hueSize;
    public float saturation;
    public float lightness;


    MaterialPropertyBlock mpb;
    Renderer renderer;

    // Update is called once per frame
    void Update()
    {
        if( mpb == null ){ mpb = new MaterialPropertyBlock(); }
        if( renderer == null ){ renderer = GetComponent<Renderer>(); }

        renderer.GetPropertyBlock(mpb);

        mpb.SetFloat("_HueStart" , hueStart );
        mpb.SetFloat("_HueSize" , hueSize );

        mpb.SetFloat("_Saturation" , saturation );
        mpb.SetFloat("_Lightness" , lightness );

        renderer.SetPropertyBlock( mpb );
    }
}
