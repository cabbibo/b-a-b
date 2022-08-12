using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteAlways]
public class Monolith : MonoBehaviour
{

    public float orbHeight;
    public float orbSize;
    public float active;
    public float _NoisePower = 1;
    public float _NoiseSize = 1;





    private MaterialPropertyBlock materialPropertyBlock;
    private Renderer renderer;


    // Update is called once per frame
    void Update()
    {
        if( materialPropertyBlock == null ){
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        if( renderer == null ){
            renderer = GetComponent<Renderer>();
        }

        materialPropertyBlock.SetFloat("_OrbHeight", orbHeight);
        materialPropertyBlock.SetFloat("_OrbSize", orbSize);
        materialPropertyBlock.SetFloat("_Active",active);   
        materialPropertyBlock.SetFloat("_NoiseSize",_NoiseSize);   
        materialPropertyBlock.SetFloat("_NoisePower",_NoisePower);   

        renderer.SetPropertyBlock(materialPropertyBlock);
    }
}
