using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using WrenUtils;

public class BindHeightMap : Binder
{

    public IslandData islandData;

    public override void Bind()
    {

        print(islandData);
        toBind.BindTexture("_HeightMap", () => WrenUtils.God.islandData.heightMap);
        toBind.BindVector3("_MapSize", () => WrenUtils.God.islandData.size);
        toBind.BindVector3("_MapOffset", () => WrenUtils.God.islandData.offset);
    }
}
