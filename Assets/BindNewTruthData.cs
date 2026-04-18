using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using EasyButtons;


public class BindNewTruthData : Binder
{
    public Form form;

    public int toAdd;
    public int toAddID = -1;
    public int total;

    public Transform truthAdder;

    public ComputeBuffer numCrystalsBuffer;

    public override void Create()
    {
        GetTotal();
        form.count = Mathf.Max( 1 , total );
    }

    public void GetTotal()
    {

        total = WrenUtils.God.state.TotalCrystals;

        numCrystalsBuffer = new ComputeBuffer( 7 , sizeof(int) );
        numCrystalsBuffer.SetData( WrenUtils.God.state.crystalsCollectedPerIsland );


    }

    public override void Bind()
    {

        toBind.BindInt( "_ToAdd" , () => toAdd );
        toBind.BindInt( "_Total" , () => total );
        toBind.BindBuffer( "_NumCrystalsBuffer" , () => numCrystalsBuffer );
        toBind.BindInt( "_ToAddID" , () => toAddID );

        toBind.BindMatrix( "_TruthAdder" , () => truthAdder.localToWorldMatrix );

    }


    public override void OnBirthed()
    {

    }

    public void SetToAdd( int id , int number )
    {
        toAdd = number;
        toAddID = id;
    }

    [Button( "OnAdded" )]
    public void OnAdded()
    {
        toAdd = 0;
        toAddID = -1;
    }

    [Button( "RandomToAdd" )]
    public void RandomToAdd()
    {
        // randomID
        int id = Random.Range( 0 , 7 );

        int count = WrenUtils.God.state.crystalsCollectedPerIsland[id];

        int randCount = Random.Range( 0 , count );
        SetToAdd( id , randCount );


    }
}