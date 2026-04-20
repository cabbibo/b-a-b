using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class IslandCrystalCollection : MonoBehaviour
{
    public Portal       portal;
    public LineRenderer lineRenderer;

    public bool isEther = false;

    public float     radiusForLineRenderer = 1.0f;
    public float     lineWidth             = 0.1f;
    public int       segmentCount          = 50;
    public Transform lookAtPoint;
    public Transform cameraLookFromPoint;

    public int islandID => isEther ? portal.sceneID : portal.biome;

    public int crystalCount =>
        God.state.crystalsCollectedPerIsland[islandID] + God.state.tmpCrystalsCollectedPerIsland[islandID];


    public int crystalsNeededForCompletion => God.state.crystalsNeededForIslandCompletion[islandID];
    public float crystalPercent => (float)crystalCount / (float)God.state.crystalsNeededForIslandCompletion[islandID];

    public void OnEnable()
    {


        lineRenderer.positionCount = segmentCount + 1;

        for ( int i = 0; i <= segmentCount; i++ ) {
            float angle = (float)i / (float)segmentCount * Mathf.PI;
            var pos = new Vector3( 0.0f , Mathf.Sin( angle ) , Mathf.Cos( angle ) ) * radiusForLineRenderer;
            lineRenderer.SetPosition( i , pos );
        }

        lineRenderer.SetWidth( lineWidth , lineWidth );

//        print( islandID );
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

        if ( currentExtraShards > 0 ) {

            God.state.AddToTMPCrystalCount( islandID , currentExtraShards );


            God.wren.shards.SpendExtraShards();
            God.cameraManager.pointOfInterestManager.SetPointOfInterestForTime( lookAtPoint , cameraLookFromPoint , 5 , 100 , 60 , .01f ,
                false , .1f );


            UpdateRepresentation();

        }


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