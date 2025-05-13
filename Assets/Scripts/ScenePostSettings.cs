using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class ScenePostSettings : MonoBehaviour
{
    public Material skyboxMaterial;

    public Material mainModelMaterial;
    public Material birdMaterial;
    public Material terrainMaterial;
    public Material shardTrail;
    public Material forcesMaterial;

    public Gradient sunGradient;
    public Gradient moonGradient;

    public PostParameters postParameters;

    public bool setOnEnable;


    public void OnEnable()
    {
        if ( setOnEnable ) {
            Set();
        }
    }


    // Update is called once per frame
    private void Update()
    {

    }


    public void Set()
    {

        God.skyboxUpdater.UpdateSkybox( skyboxMaterial );

        if ( God.wren != null ) {
            // God.wren.bird.SetMaterial(birdMaterial);
            God.wren.bird.featherMaterial = birdMaterial;
            God.wren.shards.shardTrail.debugMaterial = shardTrail;
        }

        God.postController.SetPostParameters( postParameters );


    }
}