using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CurveMaterialController : MonoBehaviour
{
    public float direction;
    public float saturation;
    public float lightness;
    public float amount;
    public float hue;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    MaterialPropertyBlock mpb;
    Renderer renderer;


    // Update is called once per frame
    void Update()
    {

        if( mpb == null ){
            mpb = new MaterialPropertyBlock();
            renderer = GetComponent<Renderer>();
        }


        mpb.SetFloat("_Saturation",saturation);
        mpb.SetFloat("_Amount",amount);
        mpb.SetFloat("_Lightness",lightness);
        mpb.SetFloat("_Hue",hue);

        renderer.SetPropertyBlock(mpb);
    
    
    }
}
