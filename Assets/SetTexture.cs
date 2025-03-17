using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SetTexture : MonoBehaviour
{

    public MaterialPropertyBlock mpb;
    public Renderer rend;
    public Texture texture;

    public void Update(){
        if( mpb == null ){
            mpb = new MaterialPropertyBlock();
        }

        if( rend == null ){
            rend = GetComponent<Renderer>();
        }

        rend.GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", texture);
        rend.SetPropertyBlock(mpb);

    }
}
