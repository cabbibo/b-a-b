using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wing1 : MonoBehaviour
{
    public float[] limbLengths;
    public bool leftOrRight;
    public Transform[] bones;
    public Transform[] connections;
    public Vector3[] rots;

    public void Create(){}
    public void CreateGameObjects(){}
    public void Destroy(){}
    public void OnDestroy(){}
    

    public void UpdatePositions(){

        float lr = leftOrRight ? 1 :-1;
        for( int i= 0; i < limbLengths.Length; i++ ){
            bones[i].localPosition = new Vector3( lr * limbLengths[i] , 0 , 0 );
        }

        // update bown positions
        for( int i = 0; i < bones.Length; i++ ){
            bones[i].localRotation = Quaternion.Euler( rots[i]);//Quaternion.AngleAxis( sideRots[i], Vector3.up ) * Quaternion.AngleAxis( upRots[i], Vector3.forward );
        }
    }

}
