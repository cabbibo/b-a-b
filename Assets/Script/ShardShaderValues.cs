using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class ShardShaderValues : MonoBehaviour
{

    public float hueStart = 1;
    public float hueSize = 1;
    public float noiseSpeed = 1;
    public float noiseSize = 10;
    public float saturation = .8f;
    public float lightness = 1;
    public float contrast = 1;
    public float _ColorMultiplier = 1;
    public float _CenterOrbFalloff = .5f;
    public float _CenterOrbFalloffSharpness = .5f;
    public float _CenterOrbImportance = 1;

    public float _DeltaStepSize = .1f;

    private Renderer renderer;
    private MaterialPropertyBlock mpb;
    // Update is called once per frame
    void Update()
    {



        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
        }

        // dont *NEED* to have a renderer attached!
        if (renderer != null)
        {
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
            mpb.SetFloat("_DeltaStepSize", _DeltaStepSize);
            renderer.SetPropertyBlock(mpb);
        }

    }
}
