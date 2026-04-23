using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class WindZoneBuffer : MonoBehaviour
{

    public float windForce;

    public Vector3 currentWindDirection;
    public Transform[] windZones;

    int totalCount;

    bool dynamic;

    public ComputeBuffer _buffer;
    // Start is called before the first frame update   
    void OnEnable()
    {
        totalCount = windZones.Length;
        _buffer = new ComputeBuffer( totalCount , sizeof(float) * 32);
        PopulateBuffer();
        
    }


    void PopulateBuffer(){

        Matrix4x4[] values = new Matrix4x4[totalCount * 2 ];
        for( int i = 0; i < totalCount; i++ ){
            values[2*i+0] = windZones[i].worldToLocalMatrix;
            values[2*i+1] = windZones[i].localToWorldMatrix;
        }

        _buffer.SetData(values);

    }

    void OnDisable(){
        if( _buffer != null ){ _buffer.Dispose(); }
    }



    void Update(){
        if(dynamic){PopulateBuffer();}
    }

}
