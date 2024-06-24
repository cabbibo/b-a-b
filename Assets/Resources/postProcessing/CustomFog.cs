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

    public PostProcessVolume volume;
    private FogEffect fog;

    private VolumeProfile profile;

    public float _Intensity;
    public Matrix4x4 _InverseProjection;

    public RenderTexture heightMap;
    public Vector3 mapSize;

    public Camera cam;
    void OnEnable()
    {
        volume = GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings(out fog);

    }

    void Update()
    {

        fog.intensity.value = _Intensity;
        heightMap = God.terrainData.heightmapTexture;
        mapSize = God.terrainData.size;


        //fog.inverseProjection.value = _InverseProjection;

        //       print("helloa");
        //        print(heightMap);
        fog.heightMap.value = heightMap;
        fog.mapSize.value = mapSize;
        // glitch.blend.value = blend;
    }



}

