using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeCompleter : MonoBehaviour
{
    public int biome;

    public void OnComplete()
    {
        WrenUtils.God.state.OnBiomeCompleted(biome);
    }

}

