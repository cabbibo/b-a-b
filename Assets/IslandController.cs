using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


[ExecuteAlways]
public class IslandController : MonoBehaviour
{

    /*

        Info / Todo:

        Each island has its own audio player that can fade out when we leave it. 
        the spining up and spinning down process of turning the islands on and off should be able to exist across a few frames 

        Entering a new island should put you in a new room?

        "Finishing" and island makes it so all the activites on that island appear ( and new ones appear on tutorial island? )


    need to fix all the rest of the painters
    need to fix the compute lookups
    need to find all assundry extra calcuations that use map size and remake them

    */

    public int defaultIslandID = 0;

    public IslandData[] islands;

    public int currentIslandID = -1;

    public Vector3 currentIslandSize;
    public Vector3 currentIslandOffset;

    public Texture currentHeightmap;
    public Texture currentWindmap;
    public Texture currentBiomeMap;

    //TODO: only 4 biomes per island? can write different shaders for different islands!
    public Texture currentBiomeMap1;
    public Texture currentBiomeMap2;
    public Texture currentFoodMap;

    public IslandData currentIsland;


    public Vector2[] islandDistances;
    public Vector2[] islandUVs;


    /*

    // code to update with offset
    Compute sampling
    frag / vert sampling
    cpu side sampling

    */



    public float islandSizeBuffer;



    // Start is called before the first frame update
    void Start()
    {

    }

    void OnEnable()
    {
        islandDistances = new Vector2[islands.Length];
        islandUVs = new Vector2[islands.Length];

        for (int i = 0; i < islands.Length; i++)
        {
            islands[i].Initialize();
        }

        OnNewIslandEntered(defaultIslandID);
    }

    // Update is called once per frame
    void Update()
    {


        if (God.wren)
        {

            for (int i = 0; i < islands.Length; i++)
            {

                Vector3 wrenPos = God.wren.transform.position;
                Vector3 islandPos = islands[i].transform.position;

                Vector3 difference = wrenPos - islandPos - Vector3.right * islands[i].size.x / 2 - Vector3.forward * islands[i].size.z / 2;

                islandDistances[i] = new Vector2(difference.x, difference.z);

                islandUVs[i] = new Vector2(
                    (difference.x + islands[i].size.x / 2) / islands[i].size.x,
                    (difference.z + islands[i].size.z / 2) / islands[i].size.z
                );



                if (
                    Mathf.Abs(islandUVs[i].x - .5f) < .5f + islandSizeBuffer &&
                    Mathf.Abs(islandUVs[i].y - .5f) < .5f + islandSizeBuffer &&
                    islands[i].onIsland == false)
                {

                    print(wrenPos);
                    print(islandPos);
                    print(Mathf.Abs(difference.x));
                    print(islands[i].size.x + islandSizeBuffer);
                    print(Mathf.Abs(difference.z));
                    print(islands[i].size.z + islandSizeBuffer);
                    print("ENTERING ISLAND");
                    OnNewIslandEntered(i);
                    //break;
                }

                if (
                    (Mathf.Abs(islandUVs[i].x - .5f) > .5f + islandSizeBuffer ||
                    Mathf.Abs(islandUVs[i].y - .5f) > .5f + islandSizeBuffer) &&
                    islands[i].onIsland == true)
                {
                    print("LEAVING ISLAND");
                    OnIslandLeft(i);
                    //break;
                }

            }
        }



    }


    public void OnNewIslandEntered(int islandIndex)
    {


        currentIslandID = islandIndex;
        currentBiomeMap1 = islands[islandIndex].biomeMap1;
        currentBiomeMap2 = islands[islandIndex].biomeMap2;
        currentHeightmap = islands[islandIndex].heightMap;
        currentWindmap = islands[islandIndex].windMap;
        currentFoodMap = islands[islandIndex].foodMap;

        currentIsland = islands[islandIndex];

        currentIslandSize = islands[islandIndex].size;
        currentIslandOffset = islands[islandIndex].transform.position;

        God.SetIslandData(islands[islandIndex]);
        islands[islandIndex].OnIslandEnter();



        //OnIslandLeft(currentIslandID);



    }

    public void OnIslandLeft(int islandIndex)
    {

        islands[islandIndex].OnIslandLeave();
        God.UnsetIslandData();

    }



}
