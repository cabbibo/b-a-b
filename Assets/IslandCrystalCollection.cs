using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class IslandCrystalCollection : MonoBehaviour
{
    public Portal       portal;
    public LineRenderer lineRenderer;

    public float radiusForLineRenderer = 1.0f;
    public float lineWidth             = 0.1f;
    public int   segmentCount          = 50;

    public int crystalCount =>
        God.state.crystalsCollectedPerIsland[portal.biome] + God.state.tmpCrystalsCollectedPerIsland[portal.biome];

    public float crystalPercent => (float)crystalCount / (float)God.state.crystalsNeededForIslandCompletion[portal.biome];


    public void OnEnable()
    {


        lineRenderer.positionCount = segmentCount + 1;

        for ( int i = 0; i <= segmentCount; i++ ) {
            float angle = (float)i / (float)segmentCount * Mathf.PI;
            var pos = new Vector3( 0.0f , Mathf.Sin( angle ) , Mathf.Cos( angle ) ) * radiusForLineRenderer;
            lineRenderer.SetPosition( i , pos );
        }

        lineRenderer.SetWidth( lineWidth , lineWidth );

//        print( portal.biome );
        //      print( God.state.crystalsCollectedPerIsland.Length );
        UpdateRepresentation();
    }

    public void OnTriggerEnter( Collider other )
    {
        if ( God.IsOurWren( other ) ) {
            DropCrystals();
        }


    }

    public void DropCrystals()
    {

        int currentExtraShards = God.wren.shards.numExtraShards;

        ///        print( currentExtraShards );

        God.state.AddToTMPCrystalCount( portal.biome , currentExtraShards );
        God.wren.shards.SpendExtraShards();
        UpdateRepresentation();


    }

    public void UpdateRepresentation()
    {

//        print( crystalPercent );

        lineRenderer.material.SetFloat( "_FillAmount" , crystalPercent );
    }

    public void Update()
    {
        UpdateRepresentation();
    }
}