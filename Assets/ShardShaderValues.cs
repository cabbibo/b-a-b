using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ShardShaderValues : MonoBehaviour
{

    public float hueStart;
    public float hueSize;
    public float noiseSpeed;
    public float noiseSize;
    public float saturation;
    public float lightness;
    public float contrast;
    public float _ColorMultiplier;
    public float _CenterOrbFalloff;
    public float _CenterOrbFalloffSharpness;
    public float _CenterOrbImportance;

    private Renderer renderer;
    private MaterialPropertyBlock mpb;
    // Update is called once per frame
    void Update()
    {

        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
        }

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_HueStart", hueStart);
        mpb.SetFloat("_HueSize", hueSize);
        mpb.SetFloat("_NoiseSpeed", noiseSpeed);
        mpb.SetFloat("_NoiseSize", noiseSize);
        mpb.SetFloat("_Saturation", saturation);
        mpb.SetFloat("_Lightness", lightness);
        mpb.SetFloat("_Contrast", contrast);
        mpb.SetFloat("_ColorMultiplier", _ColorMultiplier);
        mpb.SetFloat("_CenterOrbFalloff", _CenterOrbFalloff);
        mpb.SetFloat("_CenterOrbFalloffSharpness", _CenterOrbFalloffSharpness);
        mpb.SetFloat("_CenterOrbImportance", _CenterOrbImportance);
        renderer.SetPropertyBlock(mpb);

    }
}
