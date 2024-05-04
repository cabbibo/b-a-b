using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;


[ExecuteAlways]
public class IslandData : MonoBehaviour
{

    public float radius;
    public bool active;

    public UnityEvent OnIslandEnterEvent;
    public UnityEvent OnIslandLeaveEvent;


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




    public void Initialize()
    {
        print("Enabling");

        //biomeMap = painter.biomeMap;
        heightMap = God.terrainData.heightmapTexture;
        size = God.terrainData.size;


        Shader.SetGlobalTexture("_WindMap", windMap);
        Shader.SetGlobalTexture("_BiomeMap1", biomeMap1);
        Shader.SetGlobalTexture("_BiomeMap2", biomeMap2);
        Shader.SetGlobalTexture("_FoodMap", foodMap);

        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].Initialize();
        }

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


    // Check to see if we are in our island or not
    void WhileHibernate()
    {

        if (God.wren)
        {
            Vector3 wrenPos = God.wren.transform.position;
            Vector3 islandPos = transform.position;
            float distance = Vector3.Distance(wrenPos, islandPos);
            if (distance > radius && active)
            {
                OnIslandLeave();
            }
            else if (distance < radius && !active)
            {
                OnIslandEnter();
            }
        }

    }
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
        wrenUVPosition = God.UVInMap(positionToCheck);



        if (wrenUVPosition.x > 1 || wrenUVPosition.y > 1 || wrenUVPosition.x < 0 || wrenUVPosition.y < 0)
        {
            if (oWrenUVPosition.x <= 1 && oWrenUVPosition.y <= 1 && oWrenUVPosition.x >= 0 && oWrenUVPosition.y >= 0)
            {
                OnIslandLeave();
            }
        }
        else
        {
            if (oWrenUVPosition.x > 1 || oWrenUVPosition.y > 1 || oWrenUVPosition.x < 0 || oWrenUVPosition.y < 0)
            {
                OnIslandEnter();
            }
        }



        currentWindDirection = GetWind(wrenUVPosition);
        currentBiomeValues = GetBiomeValues(wrenUVPosition);
        currentFoodValues = GetFood(wrenUVPosition);


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


    public Vector3 GetWind(Vector2 uv)
    {

        Color c = windMap.GetPixelBilinear(uv.x, uv.y);

        if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
        {
            return new Vector3(c.r, c.g, c.b);
        }
        else
        {
            return Vector3.zero;
        }

    }

    public Vector3 GetFood(Vector2 uv)
    {

        Color c = foodMap.GetPixelBilinear(uv.x, uv.y);


        if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
        {
            return new Vector3(c.r, c.g, c.b);
        }
        else
        {
            return Vector3.zero;
        }

    }


    public float[] GetBiomeValues(Vector2 uv)
    {

        Color c1 = biomeMap1.GetPixelBilinear(uv.x, uv.y);
        Color c2 = biomeMap2.GetPixelBilinear(uv.x, uv.y);

        if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
        {
            return new float[] { c1.r, c1.g, c1.b, c1.a, c2.r, c2.g, c2.b, c2.a };
        }
        else
        {
            return new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        }


    }

    public bool onIsland = false;

    public PlayCutScene islandDiscoveredCutScene;
    public PlayCutScene islandCompleteCutScene;



    public TutorialStateManager tutorialStateManager;

    public void OnIslandEnter()
    {
        print("ON ISLAND ENTER");
        onIsland = true;
        if (God.state.islandDiscovered == false)
        {

            print("here we are");
            print(tutorialStateManager.islandReached);
            if (tutorialStateManager.islandReached == false)
            {
                print("reached here");
                tutorialStateManager.OnIslandReached();
            }

            God.state.OnIslandDiscovered();
            // TODO PLAY DISCOVERED ANIMATION
            islandDiscoveredCutScene.Play();
        }

    }


    public void OnIslandLeave()
    {

        print("ON ISLAND LEAVE");
        onIsland = false;
    }

    public Helpers.DoubleIntEvent BiomeChangeEvent;
    public void OnBiomeChange(int oldBiome, int newBiome)
    {
        OnExitBiome(oldBiome);
        OnEnterBiome(newBiome);

        // print("old Biome : " + oldBiome);
        // print(" new Biome : " + newBiome);



        BiomeChangeEvent.Invoke(oldBiome, newBiome);


    }


    public void OnExitBiome(int oldBiome)
    {

        //print("old Biome : " + oldBiome);
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
        //        print(" new Biome : " + newBiome);
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
