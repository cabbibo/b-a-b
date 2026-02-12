using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using EasyButtons;


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

    [Button( "Assign Material To Scene Objects" )]
    public void AssignMaterialToSceneObjects()
    {
        // find all objects by tag SceneObjects
        var objs = GameObject.FindGameObjectsWithTag( "SceneObject" );

        for ( int i = 0; i < objs.Length; i++ ) {

            // if weve got an LOD group assign it to everyrenderer underneath
            if ( objs[i].GetComponent<LODGroup>() != null ) {
                foreach (var renderer in objs[i].GetComponentsInChildren<MeshRenderer>()) renderer.material = mainModelMaterial;
            }


            // if weve got a renderer ourselves assign it

            if ( objs[i].GetComponent<MeshRenderer>() != null ) {
                objs[i].GetComponent<MeshRenderer>().material = mainModelMaterial;
            }


        }

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

        // Sun/moon settings are now applied via PostParameters in PostController.SetPostParameters()

    }
}