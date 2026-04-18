using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class CollectionCrown : MonoBehaviour
{
    public int   count;
    public float radius;
    public int   maxRows;

    public Mesh mesh;

    public float _Size;

    public Material material;

    private ComputeBuffer argsBuffer;
    private uint[]        args = new uint[5] { 0 , 0 , 0 , 0 , 0 };


    // Start is called before the first frame update
    private void OnEnable()
    {
        // Ensure the material has instancing enabled
        if ( !material.enableInstancing ) {
            Debug.LogError( "Material does not have GPU instancing enabled!" );
            return;
        }

    }

    private void OnDisable()
    {
        if ( argsBuffer != null ) {
            argsBuffer.Release();
            argsBuffer = null;
        }
    }

    private void OnDestroy()
    {
        if ( argsBuffer != null ) {
            argsBuffer.Release();
            argsBuffer = null;
        }
    }

    // Update is called once per frame
    private void Update()
    {


        if ( argsBuffer == null ) {


            argsBuffer = new ComputeBuffer( 1 , args.Length * sizeof(uint) , ComputeBufferType.IndirectArguments );
            uint numIndices = mesh != null ? (uint)mesh.GetIndexCount( 0 ) : 0;
            args[0] = numIndices; // Index count per instance
            args[1] = (uint)(count * maxRows); // Instance count
            args[2] = (uint)mesh.GetIndexStart( 0 ); // Start index location
            args[3] = (uint)mesh.GetBaseVertex( 0 ); // Base vertex location
            args[4] = 0; // Start instance location
            argsBuffer.SetData( args );
        }

        args[1] = (uint)(count * maxRows);
        argsBuffer.SetData( args );


        material.SetMatrix( "_Transform" , transform.localToWorldMatrix );
        material.SetFloat( "_Radius" , radius );


        // Bounding volume for frustum culling (must encompass all instances)
        var bounds = new Bounds( Vector3.zero , Vector3.one * 1000f );
        Graphics.DrawMeshInstancedIndirect(
            mesh ,
            0 ,
            material ,
            bounds ,
            argsBuffer ,
            0 ,
            null ,
            UnityEngine.Rendering.ShadowCastingMode.On ,
            true ,
            gameObject.layer
        );

    }
}