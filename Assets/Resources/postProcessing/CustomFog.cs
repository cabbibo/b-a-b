using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using WrenUtils;
using UnityEditor;


[ExecuteAlways]
public class CustomFog : MonoBehaviour
{
    public Camera camera;

    public PostController postController;

    public PostProcessVolume volume;

    private VolumeProfile profile;

    public float     _Intensity;
    public Matrix4x4 _InverseProjection;

    public RenderTexture heightMap;
    public Vector3       mapSize;
    public Vector3       mapOffset;

    public Camera cam;


    private void Update()
    {

        postController.fogEffect_Reference.intensity.value = _Intensity;
        heightMap = God.terrainData.heightmapTexture;
        mapSize = God.terrainData.size;
        mapOffset = God.terrainOffset;


        //fog.inverseProjection.value = _InverseProjection;

        //       print("helloa");
        //        print(heightMap);
        postController.fogEffect_Reference.heightMap.value = heightMap;
        postController.fogEffect_Reference.mapSize.value = mapSize;
        postController.fogEffect_Reference.mapOffset.value = mapOffset;


        // glitch.blend.value = blend;

        postController.sketchEffect_Reference.heightMap.value = heightMap;
        postController.sketchEffect_Reference.mapSize.value = mapSize;
        postController.sketchEffect_Reference.mapOffset.value = mapOffset;

    }
}