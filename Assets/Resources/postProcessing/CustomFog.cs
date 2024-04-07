using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;



[ExecuteAlways]
public class CustomFog : MonoBehaviour
{

    public Camera camera;

    public PostProcessVolume volume;
    private FogEffect fog;

    private VolumeProfile profile;
    void OnEnable()
    {
        volume = GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings(out fog);

    }

    void Update()
    {


        // glitch.blend.value = blend;
    }



}

