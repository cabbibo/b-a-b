﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class QuillFadeMaterialController : MonoBehaviour
{
    public float fade;
    public float multiplier = 1.0f;
    public Color color;

    public Transform fadeLocation;


    // Start is called before the first frame update
    void Start()
    {

    }

    MaterialPropertyBlock mpb;
    public Renderer[] renderers;

    void doUpdate()
    {

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();

        }



        for (int i = 0; i < renderers.Length; i++)
        {


            renderers[i].GetPropertyBlock(mpb);

            mpb.SetFloat("_Fade", fade);
            mpb.SetColor("_Color", color);
            mpb.SetFloat("_Multiplier", multiplier);

            if (fadeLocation != null)
            {
                mpb.SetVector("_FadeLocation", fadeLocation.transform.position);
            }
            else
            {
                mpb.SetVector("_FadeLocation", transform.position);
            }


            renderers[i].SetPropertyBlock(mpb);

        }

    }

    // Update is called once per frame
    void Update()
    {
        doUpdate();
    }

    void OnEnable()
    {
        doUpdate();
    }
}
