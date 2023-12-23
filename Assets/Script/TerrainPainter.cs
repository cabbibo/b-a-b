using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

using IMMATERIA;
using WrenUtils;


[ExecuteInEditMode]
public class TerrainPainter : Simulation
{

  public float displayScale;
  [SerializeField] public string[] options;
  public int brushType;


  public string terrainPath;

  public bool painting;

  public PaintVerts verts;
  // how many undos we get!
  public int undoBufferSize;
  public int currentUndoLocation;

  public List<float[]> undoBuffer;

  public string safeName;


  //Textur informs the start
  public Texture2D[] startTexture;
  public Texture2D[] undoTexture;
  public Texture2D[] currentTexture;

  public Texture2D biomeMap;


  // getting position and direction
  public Vector3 paintPosition;
  private Vector3 oPP;
  public Vector3 paintDirection;
  public Vector2 paintScreenPosition;
  public Vector2 paintScreenDirection;
  public Vector2 oSP;

  // brush size
  public float paintSize;
  public float paintOpacity;
  public float paintStrength;
  public float shift;
  public float fn;


  public float isPainting;

  public Material[] debugMaterials;


  public bool debugAll;


  private Color[] colors;
  private float[] values;


  public float reset;

  public Transform paintTip;
  private MaterialPropertyBlock mpb;

  Texture2D dataTexture;



  MeshRenderer paintTipRenderer;

  // to get our data back from gpu
  Queue<AsyncGPUReadbackRequest> _requests = new Queue<AsyncGPUReadbackRequest>();

  public override void Create()
  {


    if (mpb == null) { mpb = new MaterialPropertyBlock(); }

    paintTipRenderer = paintTip.GetComponent<MeshRenderer>();

    if (undoBuffer == null)
    {
      undoBuffer = new List<float[]>();
    }


    /*
        startTexture = new Texture2D( verts.width , verts.width );

       // if( undoTexture == null ){
          undoTexture = new Texture2D(startTexture.width, startTexture.height);
          Graphics.CopyTexture(startTexture, undoTexture);
        //}

       // if( currentTexture == null ){
          currentTexture = new Texture2D(startTexture.width, startTexture.height);
          Graphics.CopyTexture(startTexture, currentTexture);
        //}

        */

  }


  public int totalTextures;

  public Texture2D windTexture;
  public void SetUpTextures()
  {

    totalTextures = (int)Mathf.Ceil((float)verts.dataTypes.Length / 4);

  }


  public void ExtractWindColors()
  {

    values = verts.GetData();

    colors = new Color[(verts.count / verts.dataTypes.Length)];
    for (int i = 0; i < verts.count; i += verts.totalDataSize)
    {

      // extracting height
      float x = values[i + 0];//height;
      float y = values[i + 1];//height;
      float z = values[i + 2];//height;


      colors[i / verts.totalDataSize] = new Color(x, y, z, 0);

    }

  }

  public void SaveWindTexture()
  {

    if (windTexture == null) { windTexture = new Texture2D(verts.width, verts.width, TextureFormat.RGBAFloat, 3, true); }
    ExtractWindColors();

    windTexture.SetPixels(colors);
    windTexture.Apply(true);


  }

  public void SaveToTexture()
  {


  }




  // Only Recreate if its not the correct size;
  public override void OnGestated()
  {


    //Load();

    //ExtractColors();
    //UpdateLand();

    if (undoBuffer.Count != undoBufferSize)
    {
      undoBuffer = new List<float[]>(undoBufferSize);

      for (int i = 0; i < undoBufferSize; i++)
      {
        undoBuffer.Add(values);
      }
    }



    Save();

  }



  // binding all our information!
  public override void Bind()
  {

    life.BindPrimaryForm("_VectorBuffer", verts);

    life.BindVector3("_PaintPosition", () => this.paintPosition);
    life.BindVector3("_PaintDirection", () => this.paintDirection);
    life.BindVector2("_PaintScreenDirection", () => this.paintScreenDirection);
    life.BindFloat("_PaintSize", () => this.paintSize);
    life.BindFloat("_PaintOpacity", () => this.paintOpacity);
    life.BindFloat("_PaintStrength", () => this.paintStrength);
    life.BindFloat("_Shift", () => this.shift);
    life.BindFloat("_FN", () => this.fn);




    life.BindInt("_WhichBrush", () => this.brushType);
    life.BindInt("_TotalBrushes", () => verts.dataTypes.Length);


    life.BindFloat("_Reset", () => this.reset);
    // life.BindTexture( "_TextureReset"   , () => this.startTexture   );
    // life.BindTexture( "_UndoTexture"    , () => this.undoTexture    );
    life.BindInt("_Width", () => this.verts.width);

    // data.BindTerrainData(life);

    life.BindTexture("_HeightMap", () => WrenUtils.God.terrainData.heightmapTexture);
    life.BindVector3("_MapSize", () => WrenUtils.God.terrainData.size);



  }

  public void GetCurrentTexture()
  {

  }




  public override void WhileDebug()
  {


    //paintTipRenderer.enabled = isPainting == 1 ? true : false;

    mpb.SetInt("_Dimensions", verts.width);
    mpb.SetInt("_Count", verts.count);
    mpb.SetBuffer("_VertBuffer", verts._buffer);
    mpb.SetInt("_WhichBrush", brushType);
    mpb.SetInt("_TotalBrushes", verts.dataTypes.Length);
    mpb.SetInt("_Width", verts.width);
    // mpb.SetFloat("_Size", 100);

    mpb.SetTexture("_HeightMap", WrenUtils.God.terrainData.heightmapTexture);
    mpb.SetVector("_MapSize", WrenUtils.God.terrainData.size);

    if (debug)
    {

      if (!debugAll)
      {
        Graphics.DrawProcedural(debugMaterials[brushType], new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, (verts.count / verts.totalDataSize) * 3, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
      }
      else
      {

        for (int i = 0; i < debugMaterials.Length; i++)
        {
          mpb.SetInt("_WhichBrush", i);
          Graphics.DrawProcedural(debugMaterials[i], new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, (verts.count / verts.totalDataSize) * 3, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));
        }

      }


    }

  }







  public void MouseMove(Ray ray)
  {




    // paintPosition = data.land.Trace( ray.origin, ray.direction);
    paintTip.position = paintPosition;
    paintTip.localScale = new Vector3(paintSize, paintSize, paintSize);

    paintDirection = -(paintDirection - paintPosition);
    //print( Camera.current.transform.Inverseray.direction );
    paintDirection = paintPosition;

    RaycastHit hit;
    // Does the ray intersect any objects excluding the player layer
    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
    {
      paintPosition = hit.point;//.land.Trace( ray.origin, ray.direction);
    }
    else
    {
      paintPosition = ray.origin;
    }


    // paintPosition = data.land.Trace( ray.origin, ray.direction);
    paintTip.position = paintPosition;
    paintTip.localScale = new Vector3(paintSize, paintSize, paintSize);

    paintDirection = -(paintDirection - paintPosition);


  }




  public void MouseDown(Ray ray)
  {


    paintTipRenderer.enabled = true;


    RaycastHit hit;
    // Does the ray intersect any objects excluding the player layer
    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
    {
      paintPosition = hit.point;//.land.Trace( ray.origin, ray.direction);
    }
    else
    {
      paintPosition = ray.origin;
    }


    // paintPosition = data.land.Trace( ray.origin, ray.direction);
    paintTip.position = paintPosition;
    paintTip.localScale = new Vector3(paintSize, paintSize, paintSize);

    paintDirection = Vector3.zero;


  }


  Vector2 mousePos;
  public void WhileDown(Ray ray)
  {

    paintTipRenderer.enabled = true;


    isPainting = 1;

    //print( Camera.current.transform.Inverseray.direction );
    paintDirection = paintPosition;

    RaycastHit hit;
    // Does the ray intersect any objects excluding the player layer
    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
    {
      paintPosition = hit.point;//.land.Trace( ray.origin, ray.direction);
    }
    else
    {
      paintPosition = ray.origin;
    }


    // paintPosition = data.land.Trace( ray.origin, ray.direction);
    paintTip.position = paintPosition;
    paintTip.localScale = new Vector3(paintSize, paintSize, paintSize);

    paintDirection = -(paintDirection - paintPosition);

    // update our life
    life.YOLO();

  }


  //int getTextureID


  /*

  public void ResetToOriginal(){
    Load();
  }


  public void ResetToUndo(){
    reset = 3;
    life.YOLO();
    reset = 0;
  }



  public void ResetToFlat(){
    reset = 1;
    life.YOLO();
    reset = 0;
  }


    // getting our information back from GPU
    public void ExtractColors(){

      values = verts.GetData();

      colors =  new Color[verts.count];
      for( int i = 0; i < verts.count; i ++ ){

        // extracting height
        float h = values[ i * verts.structSize + 1 ] /  WrenUtils.God.terrainData.size.y;//height;

        // extracting flow verts
        float x = values[ i * verts.structSize + 6 ] * .5f + .5f;
        float z = values[ i * verts.structSize + 8 ] * .5f + .5f;


        float a = Mathf.Clamp( values[ i * verts.structSize + 11 ], .1f , .9999f);

        colors[i] = new Color( h,x,z,a);

      }

    }


    public void ExtractColors( float[] v){

      //int count = values.Length / verts.structSize;

      values = v;
      colors =  new Color[verts.count];
      for( int i = 0; i < verts.count; i ++ ){

        // extracting height
        float h = values[ i * verts.structSize + 1 ] / WrenUtils.God.terrainData.size.y;

        // extracting flow verts
        float x = values[ i * verts.structSize + 6 ] * .5f + .5f;
        float z = values[ i * verts.structSize + 8 ] * .5f + .5f;

        // extracting grass height
        float a = Mathf.Clamp( values[ i * verts.structSize + 11 ], .1f , .9999f);



        colors[i] = new Color( h,x,z,a);

      }

    }

    // only need to update the textures we are interested in!
     public void Save(){

      ExtractColors();
      propogateUndoBuffer();
      UpdateLand();

    }


    public void UltraSave(){

      ExtractColors();
      propogateUndoBuffer();
      UpdateLand();

      string path = "StreamingAssets/Terrain/safe";
      Saveable.Save( verts , path );

      SaveTextureAsPNG( currentTexture , Application.dataPath+"/" + path );

    }

    public void Load(){
      string path = "StreamingAssets/Terrain/safe";
      Saveable.Load( verts , path );
    }



    public void propogateUndoBuffer(){

       for( int i = undoBuffer.Count-1; i > 0; i-- ){
        undoBuffer[i] = undoBuffer[i-1];
       }

       undoBuffer[0] = values;

       currentUndoLocation = 0;

    }



    public void UpdateLand(){

      currentTexture.SetPixels(colors,0);
      currentTexture.Apply(true);

    }



    public void Undo(){
      currentUndoLocation ++;
      if( currentUndoLocation >= undoBuffer.Count-1 ){
        Debug.Log( "At Oldest");
      }else{
        MakeUndoTexture( undoBuffer[currentUndoLocation] );
      }
    }


    public void Redo(){

      currentUndoLocation --;
      if( currentUndoLocation < 0 ){
        Debug.Log( "AT NEWEST");
      }else{
        MakeUndoTexture( undoBuffer[currentUndoLocation] );
      }

    }


    public void MakeUndoTexture(float[] v){

      verts.SetData(v);

      ExtractColors();

      currentTexture.SetPixels(colors,0);
      currentTexture.Apply(true);

    }



    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
     {
         byte[] _bytes =_texture.EncodeToJPG(1000);
         System.IO.File.WriteAllBytes(_fullPath, _bytes);
         Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath + ".jpg");
     }




     public override void WhileLiving( float v ){

        while (_requests.Count > 0){
              var req = _requests.Peek();

              if (req.hasError){
                  Debug.Log("GPU readback error detected.");
                  _requests.Dequeue();
              }else if (req.done){
                  var buffer = req.GetData<float>();
                  ExtractColors( buffer.ToArray() );
                  _requests.Dequeue();
              }else{
                  break;
              }
          }
     }

  */

  public void UltraSave()
  {

    print("save");
    Saveable.Save(verts);//.Save();
    SaveWindTexture();

  }


  public void Save()
  {

    Saveable.Save(verts);//.Save();
    SaveWindTexture();

  }



}