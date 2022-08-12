using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class PaintTerrain : MonoBehaviour
{

    public int textureSize;

    public RenderTexture _PaintTexture;

    public ComputeShader shader;
    // Start is called before the first frame update
    void OnEnable()
    {
        _PaintTexture = new RenderTexture(textureSize, textureSize, 0);
        _PaintTexture.enableRandomWrite = true;
        _PaintTexture.Create();
    
    
    
    }

    // Update is called once per frame
    void Update()
    {
        
        if( God.wrenMaker.wrenBuffer != null ){
           
            God.instance.SetTerrainCompute(0,shader);
            God.instance.SetWrenCompute(0,shader);

            shader.SetTexture(0, "_PaintTexture", _PaintTexture);
            shader.SetInt("_TextureSize",textureSize);
            shader.Dispatch(0, textureSize / 32, textureSize/32, 1);

            Shader.SetGlobalTexture("_PaintTexture", _PaintTexture);

        }else{

            
            God.instance.SetTerrainCompute(0,shader);
            shader.SetTexture(1, "_PaintTexture", _PaintTexture);
            shader.SetInt("_TextureSize",textureSize);
            shader.Dispatch(1, textureSize / 32, textureSize/32, 1);

            Shader.SetGlobalTexture("_PaintTexture", _PaintTexture);

        }
    
    }
}
