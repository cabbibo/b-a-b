using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


[ExecuteAlways]
public class PostController : MonoBehaviour{

 
    public PostProcessVolume volume;
    private MainPost post;

    private VolumeProfile profile;


    public float _Hue;
    public float _Saturation;
    public float _Lightness;
    public float _Blend;
    public float _Fade;

    void OnEnable()
    {
        volume = GetComponent<PostProcessVolume>();
         
        volume.profile.TryGetSettings(out post);
    }


    public float angleOffset;
    public float sizeToFullSaturation;



    public void Update(){

        if( God.wren != null ){
            CartToPolar( God.wren.transform.position );
        }
        post._Hue.value = _Hue;
        post._Saturation.value = _Saturation;
        post._Lightness.value = _Lightness;
        post._Blend.value = _Blend;
        post._Fade.value = _Fade;

    }



    Vector2 CartToPolar( Vector3 position ){

        float angle = Mathf.Atan2( position.x, position.z );
        float radius = ( new Vector2( position.x , position.z )).magnitude;

        print( angle );
        print( radius );

        _Hue = angle  / Mathf.PI;

        return new Vector2( angle , radius );

    }


}
