using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SetTexture : MonoBehaviour
{

    public MaterialPropertyBlock mpb;
    public Renderer rend;
    public Texture texture;

    public float textureAnimationSpeed = 1.0f;
    public float animationTextureSpeedOffset;
    public Texture[] animationTextures;

    public float selfMultiplier;
    public bool doSelfMultiplier;
    

    public void Update(){
        if( mpb == null ){
            mpb = new MaterialPropertyBlock();
        }

        if( rend == null ){
            rend = GetComponent<Renderer>();
        }

        rend.GetPropertyBlock(mpb);

        if( animationTextures.Length > 0 ){
            texture = animationTextures[(int)(Time.time * textureAnimationSpeed + animationTextureSpeedOffset) % animationTextures.Length];
            mpb.SetTexture("_MainTex", texture);
        }else{
         mpb.SetTexture("_MainTex", texture);
        }

        if( doSelfMultiplier ){
            mpb.SetFloat("_OverallMultiplier", selfMultiplier);
        }
        rend.SetPropertyBlock(mpb);


    }
}
