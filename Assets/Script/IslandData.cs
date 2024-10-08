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


    public Terrain terrain;


    public Quest[] quests;

    public Biome[] biomes;

    public int[] biomeIDs = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };



    public Texture2D BiomeMap;
    public RenderTexture heightMap;



    public Texture2D windMap;

    public Texture2D biomeMap1;


    public Texture2D biomeMap2;

    public Texture2D foodMap;



    public Vector3 size;
    public Vector3 offset;
    public Vector4 currentFoodValues;


    public bool debugWind;




    public void Initialize()
    {
        print("Enabling");

        //biomeMap = painter.biomeMap;
        heightMap = terrain.terrainData.heightmapTexture;
        size = terrain.terrainData.size;


        Shader.SetGlobalTexture("_WindMap", windMap);
        Shader.SetGlobalTexture("_BiomeMap1", biomeMap1);
        Shader.SetGlobalTexture("_BiomeMap2", biomeMap2);
        Shader.SetGlobalTexture("_FoodMap", foodMap);

        for (int i = 0; i < quests.Length; i++)
        {
            quests[i].Initialize();
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
        /*
                // TODO can remove
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
        */



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

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        terrain.GetSplatMaterialPropertyBlock(mpb);
        mpb.SetTexture("_BiomeMap1", biomeMap1);
        mpb.SetTexture("_BiomeMap2", biomeMap2);
        //mpb.SetVector("_BiomeIDs1", new Vector4(biomeIDs[0], biomeIDs[1], biomeIDs[2], biomeIDs[3]));
        //mpb.SetVector("_BiomeIDs2", new Vector4(biomeIDs[4], biomeIDs[5], biomeIDs[6], biomeIDs[7]));


        terrain.SetSplatMaterialPropertyBlock(mpb);


        if (!onIsland)
        {
            WhileHibernate();
        }
        else
        {


            currentWindDirection = GetWind(wrenUVPosition);
            currentBiomeValues = GetBiomeValues(wrenUVPosition);
            currentFoodValues = GetFood(wrenUVPosition);

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



    /*public void OnBiomeCompleted(int i)
    {

        bool islandCompleted = true;

        for (int j = 0; j < biomes.Length; j++)
        {
            if (biomes[j].completed == false)
            {
                islandCompleted = false;
            }
        }

        if (islandCompleted == true)
        {
            print("ISLAND COMPLETED");
            islandCompleteCutScene.Play();

        }
    }*/


}
