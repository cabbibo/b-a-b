using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

public class AssignShardMaterialsToShards : MonoBehaviour
{
    public Material collectedMaterial;
    public Material uncollectedMaterial;

    public Shard[] shards;

    [Button( "Assign Materials" )]
    public void AssignMaterials()
    {

        for ( int i = 0; i < shards.Length; i++ ) {
            shards[i].Collected.GetComponent<Renderer>().material = collectedMaterial;
            shards[i].Uncollected.GetComponent<Renderer>().material = uncollectedMaterial;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }
}