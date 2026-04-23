using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class SetTerrainImage : MonoBehaviour
{


    public TerrainData  terrainData;
    public Texture texture;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex",terrainData.heightmapTexture);
    }
}
