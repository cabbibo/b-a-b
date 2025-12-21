using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class BindWindPaintInfo : Binder
{
    public IslandData islandData;

    public override void Bind()
    {
        
        print(WrenUtils.God.islandData.windMap  );
        toBind.BindTexture( "_WindMap" , () => WrenUtils.God.islandData.windMap );
    }
}