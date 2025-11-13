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


    public PostParameters postParameters;

    public Gradient SunGradient = new()
    {
        colorKeys = new GradientColorKey[]
        {
            new(new Color( 1 , 0 , 0 ) , 0f) ,
            new(new Color( 1 , 1 , 1 ) , 0.5f) ,
            new(new Color( 1 , 0 , 0 ) , 1f)
        } ,
        alphaKeys = new GradientAlphaKey[]
        {
            new(1f , 0f) ,
            new(1f , 1f)
        }
    };

    public Gradient MoonGradient = new()
    {
        colorKeys = new GradientColorKey[]
        {
            new(new Color( 0 , 0 , 1 ) , 0f) ,
            new(new Color( 1 , 1 , 1 ) , 0.5f) ,
            new(new Color( 0 , 0 , 1 ) , 1f)
        } ,
        alphaKeys = new GradientAlphaKey[]
        {
            new(1f , 0f) ,
            new(1f , 1f)
        }
    };

    public bool sunAutoUpdate = true;

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

#if UNITY_EDITOR
        if ( UnityEditor.BuildPipeline.isBuildingPlayer ) {
            return;
        }
#endif

        God.skyboxUpdater.UpdateSkybox( skyboxMaterial );

        if ( God.wren != null ) {
            // God.wren.bird.SetMaterial(birdMaterial);
            God.wren.bird.featherMaterial = birdMaterial;
            God.wren.shards.shardTrail.debugMaterial = shardTrail;
            God.wren.physics.forceDebugMaterial = forcesMaterial;
        }

        God.postController.SetPostParameters( postParameters );

        God.weatherManager.sunManager.dayColor = SunGradient;
        God.weatherManager.sunManager.nightColor = MoonGradient;
        God.weatherManager.sunManager.auto = sunAutoUpdate;

    }
}