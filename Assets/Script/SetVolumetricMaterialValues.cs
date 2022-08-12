using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVolumetricMaterialValues : MonoBehaviour
{


    public VolumetricLightRays rays;
    // Start is called before the first frame update

    private MaterialPropertyBlock groundMPB;
    private MaterialPropertyBlock lightMPB;

    void Start()
    {
        if( rays == null ){ rays = GetComponent<VolumetricLightRays>(); }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
