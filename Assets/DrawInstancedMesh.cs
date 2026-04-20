using System;
using UnityEngine;
using IMMATERIA;

public class DrawInstancedMesh : MonoBehaviour
{
    public float    _Size;
    public Mesh     mesh;
    public Material material;
    public Form     form;

    public Material runtimeMaterial;

    private          ComputeBuffer argsBuffer;
    private readonly uint[]        args = new uint[5] { 0 , 0 , 0 , 0 , 0 };

    private void OnEnable()
    {
        if ( material == null || mesh == null || form == null ) {
            return;
        }

        runtimeMaterial = new Material( material );

        if ( !runtimeMaterial.enableInstancing ) {
            Debug.LogError( "Material does not have GPU instancing enabled!" , this );
            return;
        }

        CreateArgsBuffer();
    }

    private void OnDisable()
    {
        ReleaseArgsBuffer();

        if ( runtimeMaterial != null ) {
            if ( Application.isPlaying ) {
                Destroy( runtimeMaterial );
            } else {
                DestroyImmediate( runtimeMaterial );
            }

            runtimeMaterial = null;
        }
    }

    private void OnDestroy()
    {
        ReleaseArgsBuffer();

        if ( runtimeMaterial != null ) {
            if ( Application.isPlaying ) {
                Destroy( runtimeMaterial );
            } else {
                DestroyImmediate( runtimeMaterial );
            }

            runtimeMaterial = null;
        }
    }

    private void CreateArgsBuffer()
    {
        ReleaseArgsBuffer();

        if ( mesh == null ) {
            return;
        }

        argsBuffer = new ComputeBuffer( 1 , args.Length * sizeof(uint) , ComputeBufferType.IndirectArguments );

        args[0] = (uint)mesh.GetIndexCount( 0 );
        args[1] = 0;
        args[2] = (uint)mesh.GetIndexStart( 0 );
        args[3] = (uint)mesh.GetBaseVertex( 0 );
        args[4] = 0;

        argsBuffer.SetData( args );
    }

    private void ReleaseArgsBuffer()
    {
        if ( argsBuffer != null ) {
            argsBuffer.Release();
            argsBuffer = null;
        }
    }

    private void Update()
    {
        if ( runtimeMaterial == null || mesh == null || form == null || form._buffer == null || argsBuffer == null ) {
            return;
        }

        args[1] = (uint)form.count;
        argsBuffer.SetData( args );

        runtimeMaterial.SetBuffer( "_FormBuffer" , form._buffer );
        runtimeMaterial.SetFloat( "_Size" , _Size );

        var bounds = new Bounds( transform.position , Vector3.one * 100000f );

        Graphics.DrawMeshInstancedIndirect(
            mesh ,
            0 ,
            runtimeMaterial ,
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