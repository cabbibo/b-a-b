using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class IslandData : MonoBehaviour
{


    public TerrainPainter windPainter;
    public TerrainPainter foodPainter;
    public TerrainPainter weatherPainter;

    public TerrainPainter biomePainter;



    public Texture2D BiomeMap;


    public Texture2D windMap;
    public RenderTexture heightMap;

    public Texture2D biomeMap;

    public float[] biomeValus;



    public Vector3 size;


    public bool debugWind;

    void OnEnable()
    {
        windMap = windPainter.LoadTexture(); ;
        //biomeMap = painter.biomeMap;
        heightMap = God.terrainData.heightmapTexture;
        size = God.terrainData.size;


        Shader.SetGlobalTexture("_WindMap", windMap);
        // Shader.SetGlobalTexture("_BiomeMap");

    }


    public Vector3 GetWindPower(Vector3 p)
    {

        Vector3 uv = God.NormalizedPositionInMap(p);//new Vector2(.5f , .6f);
        Color c = windMap.GetPixelBilinear(uv.x, uv.z);


        Vector3 tPos = new Vector3(p.x, p.y, p.z);
        //        print(c);


        Vector3 v1 = new Vector3(c.r, c.g, c.b);

        if (debugWind)
        {
            if (lr == null)
            {
                lr = GetComponent<LineRenderer>();
            }
            lr.enabled = true;

            lr.SetPosition(0, tPos);
            lr.SetPosition(1, tPos + v1 * 10);
        }
        else
        {
            lr.enabled = false;
        }
        return v1;
    }


    public float GetBiomeValue(Vector3 p)
    {
        Vector3 uv = God.NormalizedPositionInMap(p);//new Vector2(.5f , .6f);
        Color c = biomeMap.GetPixelBilinear(uv.x, uv.z);

        print("BiomeValue :" + c);
        return c.a;

    }



    public LineRenderer lr;
    void Update()
    {

        // GetWindPower( new Vector3(1000, 0,1000) );
        /*if( God.wren != null ){
         GetWindPower(God.wren.transform.position);
        }*/

    }



}
