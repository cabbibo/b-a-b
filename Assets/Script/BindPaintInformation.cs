using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

public class BindPaintInformation : Binder
{
  public TerrainPainter painter;
  public float paintSpawnMultiplier;

  public override void Bind()
  {


    toBind.BindVector3("_PaintPosition", () => painter.paintPosition);
    toBind.BindVector3("_PaintDirection", () => painter.paintDirection);
    toBind.BindFloat("_PaintSize", () => painter.paintSize);
    //toBind.BindTexture("_WindMap", () => painter.windTexture);
    toBind.BindFloat("_IsPainting", () => painter.isPainting);
    toBind.BindFloat("_PaintSpawnMultiplier", () => paintSpawnMultiplier);


    data.BindTerrainData(toBind);

  }


}
