using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class SetHeightMap : MonoBehaviour
{


    private MaterialPropertyBlock mpb;
    private Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( mpb == null ){ mpb = new MaterialPropertyBlock(); }
        if( renderer == null ){ renderer= GetComponent<Renderer>();}

        mpb.SetTexture( "_HeightMap" , God.terrainData.heightmapTexture );
        renderer.SetPropertyBlock( mpb );
    }
}
