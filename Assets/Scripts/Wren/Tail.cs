using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{

    public Transform baseBone;
    public Transform[] tailBones;

    public Vector3[] rots;

    Vector3 v1; 
    Vector3 v2; 
    Vector3 v3; 
    Vector3 v4; 

    public void UpdatePositions(){

        v1 = baseBone.position;
        v2 = baseBone.position;
        v3 = baseBone.position;
        v4 = baseBone.position;

        tailBones[0].position = transform.position;//- transform.forward;
        tailBones[1].position = transform.position;//- transform.forward;
        tailBones[2].position = transform.position;//- transform.forward;

        tailBones[0].localRotation = Quaternion.Euler( rots[0] );
        tailBones[1].localRotation = Quaternion.Euler( rots[1] );
        tailBones[2].localRotation = Quaternion.Euler( rots[2] );


    }


}
