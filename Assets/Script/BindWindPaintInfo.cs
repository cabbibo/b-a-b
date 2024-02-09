using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class BindWindPaintInfo : Binder
{

    public IslandData islandData;

    public override void Bind()
    {
        toBind.BindTexture("_WindTexture", () => islandData.windMap);
    }


}
