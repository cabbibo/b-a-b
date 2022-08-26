using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class staminaMapper : MonoBehaviour
{   



    // Update is called once per frame
    void LateUpdate()
    {


var camera = God.camera;
        if( God.wren ){
            float val = God.wren.stats.stamina / God.wren.stats.maxStamina;

 var frustumHeight = 2.0f *camera.nearClipPlane * 2.0f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
 var frustumWidth = frustumHeight * camera.aspect;
    transform.localScale = new Vector3( .03f , val * frustumHeight * .25f , .03f );
            transform.localPosition =new Vector3( -frustumWidth/2 + .3f , val * frustumHeight * .25f  - frustumHeight * .45f , camera.nearClipPlane * 2.0f );
            }


    }
}
