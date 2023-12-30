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

    public TerrainPainter biomePainter1;
    public TerrainPainter biomePainter2;



    public Texture2D BiomeMap;


    public Texture2D windMap;
    public RenderTexture heightMap;


    public Texture2D biomeMap1;
    public Texture2D biomeMap2;



    public Vector3 size;


    public bool debugWind;

    void OnEnable()
    {

        windMap = windPainter.LoadTexture(); ;
        //biomeMap = painter.biomeMap;
        heightMap = God.terrainData.heightmapTexture;
        size = God.terrainData.size;

        biomeMap1 = biomePainter1.LoadTexture();
        biomeMap2 = biomePainter2.LoadTexture();


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



    public LineRenderer lr;

    public Transform debugValueTransform;
    void Update()
    {



        // In editor ( no wren ) have a debug transform we can check values with!

        Vector3 positionToCheck = debugValueTransform.position;
        if (God.wren != null)
        {
            positionToCheck = God.wren.transform.position;
        }


        currentWindDirection = GetWind(positionToCheck);
        currentBiomeValues = GetBiomeValues(positionToCheck);



    }


    public Vector3 currentWindDirection;
    public float[] currentBiomeValues;


    public Vector3 GetWind(Vector3 p)
    {

        Vector3 uv = God.NormalizedPositionInMap(p);//new Vector2(.5f , .6f);
        Color c = windMap.GetPixelBilinear(uv.x, uv.z);

        print(uv);
        print(c);
        return new Vector3(c.r, c.g, c.b);

    }


    public float[] GetBiomeValues(Vector3 p)
    {
        Vector3 uv = God.NormalizedPositionInMap(p);//new Vector2(.5f , .6f);
        Color c1 = biomeMap1.GetPixelBilinear(uv.x, uv.z);
        Color c2 = biomeMap2.GetPixelBilinear(uv.x, uv.z);

        return new float[] { c1.r, c1.g, c1.b, c1.a, c2.r, c2.g, c2.b, c2.a };

    }


}
