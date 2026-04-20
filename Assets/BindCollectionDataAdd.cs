using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using God = WrenUtils.God;

public class BindCollectionDataAdd : Binder
{
    public IslandCrystalCollection crystalCollection;
    public Form                    form;

    public int maxRows;

    public DrawInstancedMesh instanceRenderer;

    public override void Create()
    {
        form.count = maxRows * crystalCollection.crystalsNeededForCompletion;
    }

    public override void Bind()
    {
        toBind.BindInt( "_MaxCrystals" , () => crystalCollection.crystalsNeededForCompletion );

        toBind.BindVector3( "_WrenPos" , () => God.wren != null ? God.wren.transform.position : Vector3.zero );
        toBind.BindVector3( "_WrenVel" , () => God.wren != null ? God.wren.physics.rb.velocity : Vector3.one * .001f );

    }

    public float lerpedPercentage;

    public override void WhileLiving( float v )
    {

        lerpedPercentage = Mathf.Lerp( lerpedPercentage , (float)crystalCollection.crystalPercent , .03f );
        instanceRenderer.material.SetFloat( "_CrystalPercentage" , lerpedPercentage );
        instanceRenderer.material.SetInt( "_CrystalsForComplete" , crystalCollection.crystalsNeededForCompletion );
        instanceRenderer.material.SetInt( "_MaxRows" , maxRows );

    }
}