using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class BiomeStarter : MonoBehaviour
{

    public Biome biome;

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

}
