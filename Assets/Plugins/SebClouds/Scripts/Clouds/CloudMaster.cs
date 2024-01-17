using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CloudMaster : MonoBehaviour
{
    const string headerDecoration = " --- ";

    [Header(headerDecoration + "March settings" + headerDecoration)]
    public int numStepsLight = 8;
    public float rayOffsetStrength;
    public Texture2D blueNoise;

    [Header(headerDecoration + "Base Shape" + headerDecoration)]
    public float cloudScale = 1;
    public float densityMultiplier = 1;
    public float densityOffset;
    public Vector3 shapeOffset;
    public Vector2 heightOffset;
    public Vector4 shapeNoiseWeights;

    [Header(headerDecoration + "Detail" + headerDecoration)]
    public float detailNoiseScale = 10;
    public float detailNoiseWeight = .1f;
    public Vector3 detailNoiseWeights;
    public Vector3 detailOffset;


    [Header(headerDecoration + "Lighting" + headerDecoration)]
    public float lightAbsorptionThroughCloud = 1;
    public float lightAbsorptionTowardSun = 1;
    [Range(0, 1)]
    public float darknessThreshold = .2f;
    [Range(0, 1)]
    public float forwardScattering = .83f;
    [Range(0, 1)]
    public float backScattering = .3f;
    [Range(0, 1)]
    public float baseBrightness = .8f;
    [Range(0, 1)]
    public float phaseFactor = .15f;

    [Header(headerDecoration + "Animation" + headerDecoration)]
    public float timeScale = 1;
    public float baseSpeed = 1;
    public float detailSpeed = 2;

    public float _HueStart = 0;
    public float _HueSize = 1;
    public float _SaturationStart = 0;
    public float _SaturationSize = 1;
    public float _LightnessStart = 0;
    public float _LightnessSize = 1;




    public MeshRenderer renderer;

    public Texture mainTex;
    public MaterialPropertyBlock mpb;

    // Internal
    [HideInInspector]
    public Material material;

    public Texture3D noiseTexture;
    public Texture3D detailTexture;

    void Awake()
    {

        var noise = FindObjectOfType<NoiseGenerator>();
        //noise.UpdateNoise();
    }
    void Update()
    {
        if (renderer != null)
        {



            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }


            numStepsLight = Mathf.Max(1, numStepsLight);


            var noise = FindObjectOfType<NoiseGenerator>();

            if (!Application.isPlaying)
            {
                //                noise.UpdateNoise();
            }



            //;mpb.SetTexture ("NoiseTex", noise.shapeTexture);
            //;mpb.SetTexture ("DetailNoiseTex", noise.detailTexture);

            mpb.SetTexture("NoiseTex", noiseTexture);
            mpb.SetTexture("DetailNoiseTex", detailTexture);
            mpb.SetTexture("BlueNoise", blueNoise);


            mpb.SetFloat("scale", cloudScale);
            mpb.SetFloat("densityMultiplier", densityMultiplier);
            mpb.SetFloat("densityOffset", densityOffset);
            mpb.SetFloat("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
            mpb.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
            mpb.SetFloat("darknessThreshold", darknessThreshold);
            mpb.SetFloat("rayOffsetStrength", rayOffsetStrength);

            mpb.SetFloat("detailNoiseScale", detailNoiseScale);
            mpb.SetFloat("detailNoiseWeight", detailNoiseWeight);
            mpb.SetVector("shapeOffset", shapeOffset);
            mpb.SetVector("detailOffset", detailOffset);
            mpb.SetVector("detailWeights", detailNoiseWeights);
            mpb.SetVector("shapeNoiseWeights", shapeNoiseWeights);
            mpb.SetVector("phaseParams", new Vector4(forwardScattering, backScattering, baseBrightness, phaseFactor));



            mpb.SetInt("numStepsLight", numStepsLight);


            mpb.SetFloat("timeScale", (Application.isPlaying) ? timeScale : 0);
            mpb.SetFloat("baseSpeed", baseSpeed);
            mpb.SetFloat("detailSpeed", detailSpeed);

            mpb.SetVector("_Scale", transform.lossyScale);


            mpb.SetFloat("_HueStart", _HueStart);
            mpb.SetFloat("_HueSize", _HueSize);
            mpb.SetFloat("_SaturationStart", _SaturationStart);
            mpb.SetFloat("_SaturationSize", _SaturationSize);
            mpb.SetFloat("_LightnessStart", _LightnessStart);
            mpb.SetFloat("_LightnessSize", _LightnessSize);


            renderer.SetPropertyBlock(mpb);

        }
    }

}