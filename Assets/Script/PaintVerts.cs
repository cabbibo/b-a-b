using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;
using UnityEngine.Rendering;

public class PaintVerts : Form
{

  public int width;
  public int totalDataSize;


  [SerializeField] public string[] dataTypes;
  // windDirX
  // windDirY
  // windDirZ
  // grassHeight
  // mountain
  // temple
  // desert
  // forest
  // island
  // cave
  // city
  // food
  // water


  public override void SetStructSize()
  {

    totalDataSize = dataTypes.Length;
    structSize = 1; //totalDataSize;

    //  print(structSize);
  }

  public override void SetCount()
  {
    count = width * width * totalDataSize;
    //size = WrenUtils.God.terrainData.size.x;
  }

  public override void WhileDebug()
  {

    mpb.SetBuffer("_VertBuffer", _buffer);
    mpb.SetInt("_Width", width);
    mpb.SetInt("_Count", count);
    mpb.SetInt("_TotalBrushes", totalDataSize);

    mpb.SetTexture("_HeightMap", WrenUtils.God.terrainData.heightmapTexture);
    mpb.SetVector("_MapSize", WrenUtils.God.terrainData.size);

    Graphics.DrawProcedural(debugMaterial, new Bounds(transform.position, Vector3.one * 50000), MeshTopology.Triangles, count * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));

  }




}
