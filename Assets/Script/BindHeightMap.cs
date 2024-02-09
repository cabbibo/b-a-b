using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class BindHeightMap : Binder
{

    public IslandData islandData;

    public override void Bind()
    {
        toBind.BindTexture("_HeightMap", () => islandData.heightMap);
        toBind.BindVector3("_MapSize", () => islandData.size);
    }
}
