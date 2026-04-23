using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using EasyButtons;
using System.Runtime.InteropServices;
using UnityEngine.Events;

public class ShardToggleGroup : MonoBehaviour
{
    public Shard[]     shards;
    public LightData[] data;
    public bool        allOn;

    public ComputeBuffer shardsBuffer;

    public Renderer[] renderers;

    public UnityEvent OnAllCollected;


    public void Start()
    {
        MakeShardBuffer();
    }

    public void Update()
    {
        UpdateMaterials();

    }

    // buffer is :
    // float3 pos
    // float onOff

    // shard buffer 

    private LightData tmp;

    public struct LightData
    {
        public Vector4 position;
        public Vector4 up;
        public Vector4 forward;

        public LightData( Shard shard )
        {
            var t = shard.transform;

            position = new Vector4(
                t.position.x ,
                t.position.y ,
                t.position.z ,
                shard.collected ? 1 : 0
            );

            up = new Vector4(
                t.up.x ,
                t.up.y ,
                t.up.z ,
                shard.timeCollected
            );

            forward = new Vector4(
                t.forward.x ,
                t.forward.y ,
                t.forward.z ,
                0
            );

        }
    }


    [Button( "Make Shard Buffer" )]
    public void MakeShardBuffer()
    {

        int count = shards.Length;
        shardsBuffer = new ComputeBuffer( count , 12 * sizeof(float) );

        data = new LightData[count];


        // fake it
        for ( int i = 0; i < count; i++ ) {

            print( "shard collected " + i + " " + (shards[i].collected ? 1 : 0) );

            tmp = new LightData( shards[i] );
            data[i] = tmp;

        }

        shardsBuffer.SetData( data );

        BakeUniformGridCompute();

    }

    public void OnDestroy()
    {
        shardsBuffer.Release();

        if ( gridBuffer != null ) {
            gridBuffer.Release();
        }

    }


    public void OnShardHit( GameObject shardGO )
    {


        bool allOnTmp = true;

        // Get index of shard
        for ( int i = 0; i < shards.Length; i++ ) {
            if ( shards[i].collected == false ) {
                allOnTmp = false;
            }

            if ( shards[i].gameObject == shardGO ) {
                OnToggle( i );
            }
        }

        if ( allOnTmp == true && allOn == false ) {
            allOn = true;
            OnAllCollected.Invoke();
        }

    }

    public void OnToggle( int index )
    {
        UpdateShardData( index );
    }

    public void UpdateShardData( int i )
    {

        tmp = new LightData( shards[i] );
        data[i] = tmp;

        shardsBuffer.SetData( data , i , i , 1 );


        BakeUniformGridCompute();


    }

    public MaterialPropertyBlock mpb;

    public void UpdateMaterials()
    {
        if ( mpb == null ) {
            mpb = new MaterialPropertyBlock();
        }

        for ( int i = 0; i < renderers.Length; i++ ) {
            renderers[i].GetPropertyBlock( mpb );

            mpb.SetBuffer( "_ShardBuffer" , shardsBuffer );
            mpb.SetInt( "_ShardBuffer_COUNT" , shards.Length );

            if ( gridBuffer != null ) {
                mpb.SetBuffer( "_ShardGridBuffer" , gridBuffer );
                mpb.SetVector( "_ShardGridMin" , gridMin );
                mpb.SetVector( "_ShardGridMax" , gridMax );
                mpb.SetVector( "_ShardGridCellSize" , gridCellSize );
                mpb.SetVector( "_ShardGridResolution" , new Vector4( gridResolution.x , gridResolution.y , gridResolution.z , 0 ) );
            }

            renderers[i].SetPropertyBlock( mpb );
        }
    }


    [Button( "Clear State" )]
    public void ClearState()
    {
        for ( int i = 0; i < shards.Length; i++ ) {
            shards[i].ClearSavedState();
            shards[i].Respawn();
        }

        MakeShardBuffer();

    }

    public ComputeShader gridBakeCompute;


    [Header( "Uniform Grid" )]
    public Vector3Int gridResolution = new(16 , 8 , 16);

    public ComputeBuffer gridBuffer;

    public Vector3 gridMin;
    public Vector3 gridMax;
    public Vector3 gridCellSize;


    public Bounds GetCombinedRendererBounds()
    {
        if ( renderers == null || renderers.Length == 0 ) {
            return new Bounds( transform.position , Vector3.one );
        }

        var b = renderers[0].bounds;

        for ( int i = 1; i < renderers.Length; i++ ) {
            if ( renderers[i] != null ) {
                b.Encapsulate( renderers[i].bounds );
            }
        }

        return b;
    }

    public int FlattenCell( int x , int y , int z )
    {
        return x + gridResolution.x * (y + gridResolution.y * z);
    }


    public int cellCount;

    [Button( "Bake Uniform Grid" )]
    public void BakeUniformGridCompute()
    {
        if ( shards == null || shards.Length == 0 ) {
            return;
        }

        if ( gridBakeCompute == null ) {
            return;
        }

        var bounds = GetCombinedRendererBounds();

        gridMin = bounds.min;
        gridMax = bounds.max;

        var size = bounds.size;

        gridResolution.x = Mathf.Max( 1 , gridResolution.x );
        gridResolution.y = Mathf.Max( 1 , gridResolution.y );
        gridResolution.z = Mathf.Max( 1 , gridResolution.z );

        gridCellSize = new Vector3(
            size.x / gridResolution.x ,
            size.y / gridResolution.y ,
            size.z / gridResolution.z
        );


        int newCellCount = gridResolution.x * gridResolution.y * gridResolution.z;


        // only remake the buffer if weve got a new cell count or dont have the grid 
        if ( newCellCount != cellCount ) {
            if ( gridBuffer != null ) {
                gridBuffer.Release();
                gridBuffer = null;
            }

            cellCount = newCellCount;

            gridBuffer = new ComputeBuffer( cellCount , sizeof(uint) * 16 );
        } else {

            cellCount = newCellCount;

            if ( gridBuffer == null ) {
                gridBuffer = new ComputeBuffer( cellCount , sizeof(uint) * 16 );
            }
        }


        int kernel = gridBakeCompute.FindKernel( "BakeGrid" );

        gridBakeCompute.SetBuffer( kernel , "_ShardBuffer" , shardsBuffer );
        gridBakeCompute.SetBuffer( kernel , "_ShardGridBuffer" , gridBuffer );

        gridBakeCompute.SetInt( "_ShardBuffer_COUNT" , shards.Length );
        gridBakeCompute.SetInts( "_GridResolution" , gridResolution.x , gridResolution.y , gridResolution.z );
        gridBakeCompute.SetVector( "_GridMin" , gridMin );
        gridBakeCompute.SetVector( "_GridCellSize" , gridCellSize );

        int tx = Mathf.CeilToInt( gridResolution.x / 4.0f );
        int ty = Mathf.CeilToInt( gridResolution.y / 4.0f );
        int tz = Mathf.CeilToInt( gridResolution.z / 4.0f );

        gridBakeCompute.Dispatch( kernel , tx , ty , tz );
    }
}