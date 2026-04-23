using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeCompleter : MonoBehaviour
{
    public int biome;

    public void OnComplete()
    {

        Debug.LogError( "BiomeCompleter OnComplete whyyyy" );
        // WrenUtils.God.state.OnBiomeCompleted(biome);
    }
}