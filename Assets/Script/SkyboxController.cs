using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SkyboxController : MonoBehaviour
{

    public float lightness;

    // Update is called once per frame
    void Update()
    {
        RenderSettings.skybox.SetFloat( "_Lightness" , lightness);
    }
}
