using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TreeBiomeBugCatcher : MonoBehaviour
{


    public ButterflySpawner[] butterflySpawners;
    public Biome biome;


    // Start is called before the first frame update
    void Start()
    {

    }


    public void OnTriggerEnter(Collider c)
    {


        print("LETS GO");

        if (God.IsOurWren(c))
        {
            if (!biome.started)
            {
                biome.StartBiome();
            }
        }
    }


    public void OnBugAte(float v)
    {
        print("BUG ATE");
        print(v);
        biome.AddToCompletion(v);

    }




}
