using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FantasyTree;

public class GrowTree: MonoBehaviour
{




    [Range(0,1)]
    public float barkShown;
    
    [Range(0,1)]
    public float flowersShown;

    [Range(0,1)]
    public float flowersFallen;


    public Material flowersMaterial;
    public Material barkMaterial;


    public Renderer renderer;

    public bool growing;
    public float growSpeed;
    void OnEnable(){
        renderer = GetComponent<MeshRenderer>();
    }

    public void Grow(){
        growing = true;
    }


    public void SetGrown(){
        barkShown = 1;
        flowersShown = 1;
        flowersFallen = 0;
        growing = false;
    }


    // Update is called once per frame
    void Update()
    {

        if( growing == true ){
            barkShown += growSpeed;
            flowersShown += growSpeed * .5f;
            barkShown = Mathf.Clamp(barkShown,0,1);
            flowersShown = Mathf.Clamp(flowersShown,0,1);
        }

        renderer.materials[0].SetFloat("_AmountShown",barkShown);
      
        renderer.materials[1].SetFloat("_AmountShown",flowersShown);
        renderer.materials[1].SetFloat("_FallingAmount",flowersFallen);
    
    
    }
}
