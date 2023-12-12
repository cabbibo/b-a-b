using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class IslandData : MonoBehaviour
{


    public TerrainPainter painter;


    public Texture2D windMap;
    public RenderTexture heightMap;


    public Vector3 size;


    public bool debugWind;



    void OnEnable()
    {
        windMap = painter.windTexture;
        heightMap = God.terrainData.heightmapTexture;
        size = God.terrainData.size;
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
    void Update()
    {

        // GetWindPower( new Vector3(1000, 0,1000) );
        /*if( God.wren != null ){
         GetWindPower(God.wren.transform.position);
        }*/

    }



}
