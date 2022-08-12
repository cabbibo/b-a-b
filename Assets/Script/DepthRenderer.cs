
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class DepthRenderer : MonoBehaviour
{

  public RenderTexture texture;
  
  public string layerToRender;


  public Renderer debugRenderer;
  public Camera cam;




  private RenderTextureDescriptor textureDescriptor;

  public  void OnEnable(){
    
    Set();
  }


  public void Update(){
     // Set();
  }



  public void Set(){
    
    //textureDescriptor = new RenderTextureDescriptor( cam.pixelWidth ,cam.pixelHeight,RenderTextureFormat.Depth,24);
    texture =  new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24,RenderTextureFormat.Depth);
    texture.Create();

    //print( texture.width);
   
    cam.depthTextureMode = DepthTextureMode.DepthNormals;
    cam.SetTargetBuffers(texture.colorBuffer , texture.depthBuffer );
    cam.Render();

    debugRenderer.sharedMaterial.SetTexture("_MainTex", texture );
    debugRenderer.transform.localScale = Vector3.one * cam.orthographicSize * 2;

  //RenderTexture.ReleaseTemporary( texture );

  }



}

