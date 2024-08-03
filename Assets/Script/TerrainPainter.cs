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




  // Makes it so we have the right scale for our windows display
  public float displayScale;
  public int brushType;


  public string terrainPath;

  public bool painting;

  public PaintVerts verts;
  // how many undos we get!

  public int undoBufferSize;
  public int currentUndoLocation;

  public List<float[]> undoBuffer;


  // String for saving safeness;
  string safeName
  {
    get
    {
      return WrenUtils.God.islandData.name + "_" + gameObject.name;
    }

  }


  //Textur informs the start
  public Texture2D startTexture;
  public Texture2D undoTexture;
  public Texture2D currentTexture;



  // getting position and direction
  // These are all asigned via script
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

  public Material debugMaterial;



  private Color[] colors;
  private float[] values;


  public float reset;

  public Transform paintTip;
  private MaterialPropertyBlock mpb;



  public RenderTexture saveRenderTexture;

  Texture2D dataTexture;



  MeshRenderer paintTipRenderer;

  // to get our data back from gpu
  Queue<AsyncGPUReadbackRequest> _requests = new Queue<AsyncGPUReadbackRequest>();

  public override void Create()
  {


    if (verts.dataTypes.Length > 4)
    {
      print("TOO MANY DATA TYPES");
    }

    if (mpb == null) { mpb = new MaterialPropertyBlock(); }

    paintTipRenderer = paintTip.GetComponent<MeshRenderer>();

    if (undoBuffer == null)
    {
      undoBuffer = new List<float[]>();
    }



    startTexture = new Texture2D(verts.width, verts.width);

    if (undoTexture == null)
    {
      undoTexture = new Texture2D(startTexture.width, startTexture.height);
      Graphics.CopyTexture(startTexture, undoTexture);
    }

    // rebuild shouldn't override;
    if (currentTexture == null)
    {

      currentTexture = new Texture2D(verts.width, verts.width, TextureFormat.RGBAFloat, -1, true);
      Graphics.CopyTexture(startTexture, currentTexture);

    }




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

  // Dont save mips
  public void SaveTexture()
  {

    currentTexture = new Texture2D(verts.width, verts.width, TextureFormat.RGBAFloat, -1, true);
    ExtractColors();

    currentTexture.SetPixels(colors);
    currentTexture.Apply(true);

  }





  // Only Recreate if its not the correct size;
  public override void OnGestated()
  {


    Load();

    ExtractColors();


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
    life.BindTexture("_TextureReset", () => this.startTexture);
    life.BindTexture("_UndoTexture", () => this.undoTexture);
    life.BindInt("_Width", () => this.verts.width);

    life.BindTexture("_HeightMap", () => WrenUtils.God.terrainData.heightmapTexture);
    life.BindVector3("_MapSize", () => WrenUtils.God.terrainData.size);


    life.BindVector3("_MapOffset", () => WrenUtils.God.terrainOffset);



  }

  public void GetCurrentTexture()
  {

  }


  public bool showValues = true;


  public int debugDrawMultiplier = 1;


  public override void WhileDebug()
  {
    DrawValues();
  }
  public void DrawValues()
  {
    //paintTipRenderer.enabled = isPainting == 1 ? true : false;



    mpb.SetBuffer("_VertBuffer", verts._buffer);

    mpb.SetInt("_Dimensions", verts.width);
    mpb.SetInt("_Count", verts.count);
    mpb.SetInt("_WhichBrush", brushType);
    mpb.SetInt("_TotalBrushes", verts.dataTypes.Length);
    mpb.SetInt("_Width", verts.width);

    mpb.SetTexture("_HeightMap", WrenUtils.God.terrainData.heightmapTexture);
    mpb.SetVector("_MapSize", WrenUtils.God.terrainData.size);
    mpb.SetVector("_MapOffset", WrenUtils.God.terrainOffset);

    mpb.SetInt("_ShownBrushes", debugDrawMultiplier);


    Graphics.DrawProcedural(debugMaterial, new Bounds(transform.position, Vector3.one * 100000), MeshTopology.Triangles, (verts.count * debugDrawMultiplier) * 3, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Debug"));

  }





  public void OnGUIDisable()
  {
    paintTip.gameObject.SetActive(false);
  }
  public void OnGUIEnable()
  {
    paintTip.gameObject.SetActive(true);
  }

  public void MouseMove(Ray ray)
  {


    print(WrenUtils.God.terrainOffset);

    // paintPosition = data.land.Trace( ray.origin, ray.direction);
    paintTip.position = paintPosition;
    paintTip.localScale = new Vector3(paintSize, paintSize, paintSize);
    paintTip.rotation = Quaternion.LookRotation(paintDirection);



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


    if (isPainting == 0) return;
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

    if (isPainting == 0) return;

    paintTipRenderer.enabled = true;


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




  public void ResetToOriginal()
  {
    print("TEST");
    Load();
  }


  public void ResetToUndo()
  {
    reset = 3;
    life.YOLO();
    reset = 0;
  }



  public void ResetToFlat()
  {
    reset = 1;
    life.YOLO();
    reset = 0;
  }


  // getting our information back from GPU
  public void ExtractColors()
  {

    values = verts.GetData();
    UnpackDataIntoColors(values);

  }

  // METHOD for doing it async
  public void ExtractColors(float[] v)
  {

    UnpackDataIntoColors(v);

  }
  public virtual void UnpackDataIntoColors(float[] v)
  {

    values = v;
    colors = new Color[verts.count];
    for (int i = 0; i < verts.count; i++)
    {


      float x = values[i * verts.structSize + 0];
      float y = values[i * verts.structSize + 1];
      float z = values[i * verts.structSize + 2];
      float w = values[i * verts.structSize + 3];


      colors[i] = new Color(x, y, z, w);

    }
  }


  public Texture2D GenerateTexture()
  {

    ExtractColors();

    Texture2D t = new Texture2D(verts.width, verts.width, TextureFormat.RGBAFloat, -1, true);

    t.SetPixels(colors, 0);
    t.Apply(true);

    return t;

  }



  // only need to update the textures we are interested in!
  public void Save()
  {

    ExtractColors();
    propogateUndoBuffer();
    UpdateLand();

  }


  public void UltraSave()
  {

    ExtractColors();
    propogateUndoBuffer();
    UpdateLand();

    SaveTexture();

    print("ULTRA SAVE");
    string path = "StreamingAssets/Terrain/" + safeName;
    Saveable.Save(verts, path);

    // saving to new folder

    SaveTextureAsEXR(currentTexture, Application.dataPath + "/Terrains/Data/" + safeName);

    // SaveTextureAsPNG(currentTexture, Application.dataPath + "/" + path);
    //SaveCompressedTexture(currentTexture, Application.dataPath + "/" + path);
    //SaveRenderTexture(currentTexture);
    //SaveTextureAsAsset(currentTexture, Application.dataPath + "/Resources/Terrains/Data/" + safeName);

  }

  public void SaveRenderTexture(Texture2D t)
  {

    if (t.width != saveRenderTexture.width || t.height != saveRenderTexture.height)
    {
      Debug.LogError("Render Texture Size Mismatch");
    }

    Graphics.Blit(t, saveRenderTexture);

  }

  public Texture2D LoadTexture()
  {

    print("LOADING");
    print(safeName);
    string path = "StreamingAssets/Terrain/" + safeName;
    path = Application.dataPath + "/" + path + ".jpg";

    if (System.IO.File.Exists(path))
    {


      byte[] fileData = System.IO.File.ReadAllBytes(path);


      print(fileData.Length);

      Texture2D texture = new Texture2D(2, 2);
      ImageConversion.LoadImage(texture, fileData); //..this will auto-resize the texture dimensions.

      // texture = (Texture2D)Resources.Load(path);
      return texture;
    }
    else
    {
      Debug.LogError("File not found at " + path);
      return null;
    }
  }



  public void Load()
  {
    string path = "StreamingAssets/Terrain/" + safeName;
    print(path);
    Saveable.Load(verts, path);
  }



  public void propogateUndoBuffer()
  {

    for (int i = undoBuffer.Count - 1; i > 0; i--)
    {
      undoBuffer[i] = undoBuffer[i - 1];
    }

    undoBuffer[0] = values;

    currentUndoLocation = 0;

  }



  public void UpdateLand()
  {

    currentTexture.SetPixels(colors, 0);
    currentTexture.Apply(true);

  }



  public void Undo()
  {
    currentUndoLocation++;
    if (currentUndoLocation >= undoBuffer.Count - 1)
    {
      Debug.Log("At Oldest");
    }
    else
    {
      MakeUndoTexture(undoBuffer[currentUndoLocation]);
    }
  }


  public void Redo()
  {

    currentUndoLocation--;
    if (currentUndoLocation < 0)
    {
      Debug.Log("AT NEWEST");
    }
    else
    {
      MakeUndoTexture(undoBuffer[currentUndoLocation]);
    }

  }


  public void MakeUndoTexture(float[] v)
  {

    verts.SetData(v);

    ExtractColors();

    currentTexture.SetPixels(colors, 0);
    currentTexture.Apply(true);

  }

  public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
  {
    byte[] _bytes = _texture.EncodeToJPG(1000);
    System.IO.File.WriteAllBytes(_fullPath + ".jpg", _bytes);
    Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath + ".jpg");
  }



  public static void SaveTextureAsEXR(Texture2D _texture, string _fullPath)
  {
    byte[] _bytes = ImageConversion.EncodeToEXR(_texture, Texture2D.EXRFlags.OutputAsFloat);
    System.IO.File.WriteAllBytes(_fullPath + ".exr", _bytes);
    Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath + ".exr");
  }


  public static void SaveCompressedTexture()
  {

  }


  // Extracting colors using request
  public override void WhileLiving(float v)
  {

    while (_requests.Count > 0)
    {

      print("SOMETHING NOT RIGHT");
      var req = _requests.Peek();

      if (req.hasError)
      {
        Debug.Log("GPU readback error detected.");
        _requests.Dequeue();
      }
      else if (req.done)
      {
        var buffer = req.GetData<float>();
        ExtractColors(buffer.ToArray());
        _requests.Dequeue();
      }
      else
      {
        break;
      }
    }


    DrawValues();
  }







}