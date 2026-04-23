using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class AlwaysAbove : MonoBehaviour
{

    public Transform target;
    public float upVal;
    public bool lockToCamera;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 tPos;
        Quaternion tRot;
        if( !lockToCamera ){

            tPos = target.position + Vector3.up * upVal;
            tRot = Quaternion.LookRotation( -(God.camera.transform.position - tPos)  , Vector3.up );// LookAt( God.camera.transform , Vector3.up );
            
        }else{

            tPos  = God.camera.transform.position +upVal * God.camera.transform.forward + God.camera.transform.up * upVal * .3f;
            tRot = Quaternion.LookRotation( God.camera.transform.forward ,  God.camera.transform.up );
        
        }

        transform.position = Vector3.Lerp(transform.position , tPos , .99f );
        transform.rotation = Quaternion.Slerp(transform.rotation , tRot , .99f );

        
    }
}
