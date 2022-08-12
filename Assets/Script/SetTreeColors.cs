using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SetTreeColors : MonoBehaviour
{
    // Start is called before the first frame update

    public Renderer[] trees;

    public Color _BaseColor;
    public Color _TipColor;
    public float _TipColorMultiplier;
    public float _BaseColorMultiplier;

    private MaterialPropertyBlock mpb;
    void OnEnable()
    {

        trees[0].GetPropertyBlock(mpb,0);
        
    }

    // Update is called once per frame
    void Update()
    {

        mpb.SetColor("_BaseColor", _BaseColor);
        mpb.SetColor("_TipColor", _TipColor);

        
        mpb.SetFloat("_BaseColorMultiplier", _BaseColorMultiplier);
        mpb.SetFloat("_TipColorMultiplier", _TipColorMultiplier);

        for( int i = 0; i < trees.Length; i++ ){
            trees[i].SetPropertyBlock(mpb,0);
        }

    
        
    }
}
