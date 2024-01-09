using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WrenUtils;


/*
  
  This Script is going to be a relatively massive singleton
  that is going to hold all the data and helpers for binding
  that data throughout the project! 

  I'm hoping this will help me get away from the horrific
  spagetti code I'm used to!


*/
namespace IMMATERIA
{
  public class Data : Cycle
  {


    public Transform camera;
    public IMMATERIA.God god;


    public float time;

    public override void Create()
    {
      if (camera == null) { camera = Camera.main.transform; }
      if (god == null) { GetComponent<IMMATERIA.God>(); }
    }

    public override void WhileLiving(float v)
    {

      time = Time.time;

      if (WrenUtils.God.wren != null)
      {
        //      print("we are going well");
      }
      else
      {
        //print("null");
      }

    }
    Transform t;
    Vector3 vel;
    public void BindPlayerData(Life toBind)
    {

      if (WrenUtils.God.wren != null)
      {

        t = WrenUtils.God.wren.transform;
        vel = WrenUtils.God.wren.physics.rb.velocity;

      }
      toBind.BindVector3("_WrenPos", () => getWrenPos());
      toBind.BindVector3("_WrenDir", () => getWrenForward());
      toBind.BindVector3("_WrenVel", () => vel);



    }



    Vector3 getWrenPos()
    {

      if (WrenUtils.God.wren != null)
      {
        t = WrenUtils.God.wren.transform;
      }

      if (t != null) { return t.position; } else { return WrenUtils.God.camera.transform.position; }
    }


    Vector3 getWrenForward()
    {

      if (WrenUtils.God.wren != null)
      {
        t = WrenUtils.God.wren.transform;
      }

      if (t != null) { return WrenUtils.God.wren.physics.vel; } else { return WrenUtils.God.camera.transform.forward; }
    }


    public void BindWrenBuffer(Life toBind)
    {

      toBind.BindBuffer("_WrenBuffer", () => WrenUtils.God.wrenMaker.wrenBuffer);
      toBind.BindInt("_WrenBuffer_COUNT", () => WrenUtils.God.wrenMaker.numWrens);

    }



    public void BindTerrainData(Life toBind)
    {

      //    print(WrenUtils.God.terrainData.size);    
      toBind.BindTexture("_HeightMap", () => WrenUtils.God.terrainData.heightmapTexture);
      toBind.BindVector3("_MapSize", () => WrenUtils.God.terrainData.size);
      toBind.BindVector3("_MapCenter", () => WrenUtils.God.terrainData.size);
    }

    public void BindCameraData(Life toBind)
    {
      if (camera == null) { camera = Camera.main.transform; }
      toBind.BindVector3("_CameraForward", () => this.camera.forward);
      toBind.BindVector3("_CameraUp", () => this.camera.up);
      toBind.BindVector3("_CameraRight", () => this.camera.right);
    }

    public void BindAllData(Life life)
    {
      BindCameraData(life);
      BindPlayerData(life);
      BindTerrainData(life);
      // BindWrenBuffer(life);
    }



    public void OnUpdate()
    {
      //  print( WrenUtils.God.wren);
    }

  }
}