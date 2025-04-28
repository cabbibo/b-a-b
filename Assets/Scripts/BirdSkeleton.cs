using System.Collections;
using System.Collections.Generic;
using Rewired.ComponentControls.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class BirdSkeleton : MonoBehaviour
{
    public FullBird       fullBird;
    public DebugHierarchy debugHierarchy;

    private uint[] args = new uint[5] { 0 , 0 , 0 , 0 , 0 };

    private int           instanceCount = 0;
    public  Mesh          instanceMesh;
    public  Material      instanceMaterial;
    public  int           subMeshIndex        = 0;
    private int           cachedInstanceCount = -1;
    private int           cachedSubMeshIndex  = -1;
    private ComputeBuffer argsBuffer;

    public ComputeBuffer finalTransformBuffer;
    public ComputeShader transformShader;

    private void OnEnable()
    {
        argsBuffer = new ComputeBuffer( 1 , args.Length * sizeof(uint) , ComputeBufferType.IndirectArguments );
        instanceCount = debugHierarchy.connections.Length;
        UpdateBuffers();
    }

    private void UpdateBuffers()
    {
//        print( "updaing buffers" );

        // Ensure submesh index is in range
        if ( instanceMesh != null ) {
            subMeshIndex = Mathf.Clamp( subMeshIndex , 0 , instanceMesh.subMeshCount - 1 );
        }


        // Indirect args
        if ( instanceMesh != null ) {
            args[0] = (uint)instanceMesh.GetIndexCount( subMeshIndex );
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart( subMeshIndex );
            args[3] = (uint)instanceMesh.GetBaseVertex( subMeshIndex );
        } else {
            args[0] = args[1] = args[2] = args[3] = 0;
        }

        argsBuffer.SetData( args );

        if ( finalTransformBuffer != null ) {
            finalTransformBuffer.Release();
        }

        finalTransformBuffer = new ComputeBuffer( instanceCount , 24 * sizeof(float) );
        values = new float[instanceCount * 24];

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }


    public void Destroy()
    {
        if ( finalTransformBuffer != null ) {
            finalTransformBuffer.Release();
        }

    }

    public int  numGroups;
    public uint numThreads;

    public float _Size;

    public float[] values;

    // Update is called once per frame


    public void UpdateBones()
    {

        //   print( "uppp" );
        fullBird.SetBirdParameters( transformShader );

        uint y;
        uint z;
        transformShader.GetKernelThreadGroupSizes( 0 , out numThreads , out y , out z );

        transformShader.SetInt( "_Count" , instanceCount );
        // print( instanceCount );
        // / print( debugHierarchy._connectionsBuffer );
        transformShader.SetBuffer( 0 , "_FinalTransformBuffer" , finalTransformBuffer );
        transformShader.SetBuffer( 0 , "_TransformBuffer" , debugHierarchy._buffer );
        transformShader.SetBuffer( 0 , "_ConnectionBuffer" , debugHierarchy._connectionsBuffer );

        transformShader.SetFloat( "_Size" , _Size );
        transformShader.SetVector( "_Soul" , fullBird.hip.position );

        //  print( fullBird.hip.position );

        numGroups = (instanceCount + ((int)numThreads - 1)) / (int)numThreads;

        if ( numGroups <= 0 ) {
            numGroups = 1;
        }

        transformShader.Dispatch( 0 , numGroups , 1 , 1 );


        finalTransformBuffer.GetData( values );

    }

    private void LateUpdate()
    {


        if ( debugHierarchy.enabled == false ) {
            debugHierarchy.UpdateMatrices();
        } else {

        }

        instanceCount = debugHierarchy.connections.Length;

        if ( cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex ) {

            UpdateBuffers();
        }


        //  UpdateBones();


        instanceMaterial.SetInt( "_Count" , debugHierarchy.connections.Length );
        //instanceMaterial.SetBuffer( "_TransformBuffer" , debugHierarchy._buffer );
        //instanceMaterial.SetBuffer( "_ConnectionBuffer" , debugHierarchy._connectionsBuffer );
        instanceMaterial.SetBuffer( "_FinalTransformBuffer" , finalTransformBuffer );

        Graphics.DrawMeshInstancedIndirect( instanceMesh , subMeshIndex , instanceMaterial ,
            new Bounds( Vector3.zero , new Vector3( 10000.0f , 10000.0f , 10000.0f ) ) , argsBuffer );

    }
}