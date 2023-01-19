using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;

[ExecuteAlways]
public class SetGlobalTerrainShaderInfo : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {

          Shader.SetGlobalTexture( "_HeightMap" ,  God.terrainData.heightmapTexture );
        Shader.SetGlobalVector("_MapSize",  God.terrainData.size);
    }

}
