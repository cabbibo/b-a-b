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


    public Biome[] biomes;



    public Texture2D BiomeMap;
    public RenderTexture heightMap;



    public Texture2D windMap;

    public Texture2D biomeMap1;


    public Texture2D biomeMap2;

    public Texture2D foodMap;



    public Vector3 size;
    public Vector4 currentFoodValues;


    public bool debugWind;

    void OnEnable()
    {

        //biomeMap = painter.biomeMap;
        heightMap = God.terrainData.heightmapTexture;
        size = God.terrainData.size;




        Shader.SetGlobalTexture("_WindMap", windMap);
        Shader.SetGlobalTexture("_BiomeMap1", biomeMap1);
        Shader.SetGlobalTexture("_BiomeMap2", biomeMap2);
        Shader.SetGlobalTexture("_FoodMap", foodMap);

        // Shader.SetGlobalTexture("_BiomeMap");

    }

    void Start()
    {

        OnBiomeChange(God.state.currentBiomeID, God.state.currentBiomeID);
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


    public float maxBiomeValue;
    public float secondMaxBiomeValue;

    public float oMaxBiomeValue;
    public float oSecondMaxBiomeValue;


    public int maxBiomeID;
    public int secondMaxBiomeID;



    public int oMaxBiomeID;
    public int oSecondMaxBiomeID;


    public Vector2 wrenUVPosition;
    public Vector2 oWrenUVPosition;


    void Update()
    {



        // In editor ( no wren ) have a debug transform we can check values with!


        Vector3 positionToCheck = Vector3.zero;

        if (debugValueTransform != null) { positionToCheck = debugValueTransform.position; }
        if (God.wren != null)
        {
            positionToCheck = God.wren.transform.position;
        }

        oWrenUVPosition = wrenUVPosition;
        wrenUVPosition = God.NormalizedPositionInMap(positionToCheck);


        currentWindDirection = GetWind(positionToCheck);
        currentBiomeValues = GetBiomeValues(positionToCheck);
        currentFoodValues = GetFood(positionToCheck);


        oSecondMaxBiomeValue = secondMaxBiomeValue;
        oMaxBiomeID = maxBiomeID;

        oMaxBiomeValue = maxBiomeValue;
        oSecondMaxBiomeValue = secondMaxBiomeValue;

        maxBiomeValue = 0;
        secondMaxBiomeValue = 0;

        maxBiomeID = -1;
        oSecondMaxBiomeID = -1;
        for (int i = 0; i < currentBiomeValues.Length; i++)
        {
            if (currentBiomeValues[i] > maxBiomeValue)
            {
                secondMaxBiomeValue = maxBiomeValue;
                secondMaxBiomeID = maxBiomeID;
                if (secondMaxBiomeValue == 0)
                {
                    secondMaxBiomeValue = currentBiomeValues[i];
                    secondMaxBiomeID = i;
                }
                maxBiomeValue = currentBiomeValues[i];
                maxBiomeID = i;
            }
        }



        if (maxBiomeID != oMaxBiomeID)
        {
            OnBiomeChange(oMaxBiomeID, maxBiomeID);
        }






    }


    public Vector3 currentWindDirection;
    public float[] currentBiomeValues;


    public Vector3 GetWind(Vector3 p)
    {

        Vector3 uv = God.NormalizedPositionInMap(p);//new Vector2(.5f , .6f);
        Color c = windMap.GetPixelBilinear(uv.x, uv.z);
        return new Vector3(c.r, c.g, c.b);

    }

    public Vector3 GetFood(Vector3 p)
    {

        Vector3 uv = God.NormalizedPositionInMap(p);//new Vector2(.5f , .6f);
        Color c = foodMap.GetPixelBilinear(uv.x, uv.z);
        return new Vector4(c.r, c.g, c.b, c.r);

    }


    public float[] GetBiomeValues(Vector3 p)
    {
        Vector3 uv = God.NormalizedPositionInMap(p);//new Vector2(.5f , .6f);
        Color c1 = biomeMap1.GetPixelBilinear(uv.x, uv.z);
        Color c2 = biomeMap2.GetPixelBilinear(uv.x, uv.z);

        return new float[] { c1.r, c1.g, c1.b, c1.a, c2.r, c2.g, c2.b, c2.a };

    }

    public bool onIsland = false;

    public void OnIslandEnter()
    {
        onIsland = true;
        if (God.state.islandDiscovered == false)
        {
            God.state.OnIslandDiscovered();
            // TODO PLAY DISCOVERED ANIMATION
        }
    }

    public void OnIslandLeave()
    {
        onIsland = false;
    }

    public Helpers.DoubleIntEvent BiomeChangeEvent;
    public void OnBiomeChange(int oldBiome, int newBiome)
    {
        OnExitBiome(oldBiome);
        OnEnterBiome(newBiome);

        print("old Biome : " + oldBiome);
        print(" new Biome : " + newBiome);



        BiomeChangeEvent.Invoke(oldBiome, newBiome);


    }


    public void OnExitBiome(int oldBiome)
    {

        print("old Biome : " + oldBiome);
        if (oldBiome == -1 || oldBiome == 7)
        {
            LeaveNeutralZone();
        }
        else
        {

            biomes[oldBiome].OnExitBiome();
        }
    }

    public void OnEnterBiome(int newBiome)
    {
        print(" new Biome : " + newBiome);
        if (newBiome == -1 || newBiome == 7)
        {
            EnterNeutralZone();
        }
        else
        {

            biomes[newBiome].OnEnterBiome();
        }
    }

    public void EnterNeutralZone()
    {

    }

    public void LeaveNeutralZone()
    {

    }


}
