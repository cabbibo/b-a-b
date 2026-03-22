using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class SetCameraInShadow : MonoBehaviour
{
    public bool  inShadow          = false;
    public float lerpedShadowValue = 0.0f;

    public float lerpedShadowSpeed = .4f;

    public Transform sun;

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        RaycastHit hit;

        inShadow = false;

        if ( Physics.Raycast( transform.position , sun.transform.forward * -1f , out hit , Mathf.Infinity ) ) {
            inShadow = true;
        }

        lerpedShadowValue = Mathf.Lerp( lerpedShadowValue , inShadow ? 1 : 0 , lerpedShadowSpeed );


        Shader.SetGlobalFloat( "_CameraInShadow" , inShadow ? 1 : 0 );
        Shader.SetGlobalFloat( "_CameraInShadowLerped" , lerpedShadowValue );
    }
}