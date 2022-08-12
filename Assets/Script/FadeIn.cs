using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class FadeIn : MonoBehaviour
{


    public Renderer renderer;

    public float value;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private MaterialPropertyBlock mpb;
    // Update is called once per frame
    void Update()
    {

        if( mpb== null ){ mpb = new MaterialPropertyBlock();}
        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Fade",value);
        renderer.SetPropertyBlock(mpb);
        
    }
}
