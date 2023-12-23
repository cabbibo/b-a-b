using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;


[CustomEditor(typeof(TerrainPainter))]
public class TerrainPainterEditor : Editor
{
  //private static bool m_editMode = false;
  //private static bool m_editMode2 = false;

  void OnSceneGUI()
  {

    /*Vector2 dpi = DPIHelper.GetSystemDPI();
    Debug.Log($"System DPI: X = {dpi.x}, Y = {dpi.y}");

    Debug.Log(dpi.x / 96.0f);*/

    TerrainPainter painter = (TerrainPainter)target;



    if (Event.current.type == EventType.KeyDown)
    {


      if (Event.current.keyCode == KeyCode.Z) { painter.fn = -1; }
      if (Event.current.keyCode == KeyCode.X) { painter.shift = -1; }
      if (Event.current.keyCode == KeyCode.C) { painter.paintStrength = -1; }

    }

    if (Event.current.type == EventType.KeyUp)
    {

      if (Event.current.keyCode == KeyCode.Z) { painter.fn = 1; }
      if (Event.current.keyCode == KeyCode.X) { painter.shift = 1; }
      if (Event.current.keyCode == KeyCode.C) { painter.paintStrength = 1; }

    }



    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

    if (Event.current.type == EventType.MouseMove)
    {
      Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
      mousePos.y = Camera.current.pixelHeight - mousePos.y;
      Ray ray = Camera.current.ScreenPointToRay(mousePos);

      painter.MouseMove(ray);


      painter.oSP = painter.paintScreenPosition;
      painter.paintScreenPosition = mousePos;

      painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;


      // painter.paintScreenDirection = Vector2.zero;


      painter.oSP = painter.paintScreenPosition;
      painter.paintScreenPosition = mousePos;

      painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;

    }

    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
    {


      //print("mouse");

      Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
      mousePos.y = Camera.current.pixelHeight - mousePos.y;
      Ray ray = Camera.current.ScreenPointToRay(mousePos);
      painter.MouseDown(ray);

      painter.paintScreenPosition = mousePos;
      painter.oSP = mousePos;

      painter.paintScreenDirection = Vector2.zero;

    }

    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
    {

      Debug.Log(painter.displayScale);
      Vector2 mousePos = Event.current.mousePosition * painter.displayScale;
      mousePos.y = Camera.current.pixelHeight - mousePos.y;
      Ray ray = Camera.current.ScreenPointToRay(mousePos);
      painter.WhileDown(ray);

      painter.oSP = painter.paintScreenPosition;
      painter.paintScreenPosition = mousePos;

      painter.paintScreenDirection = painter.paintScreenPosition - painter.oSP;

    }



    if (Event.current.type == EventType.MouseUp)
    {
      //painter.Save();

      painter.Save();
      painter.isPainting = 0;

    }


  }
  public override void OnInspectorGUI()
  {

    // Debug.Log("PRSITN");
    TerrainPainter painter = (TerrainPainter)target;



    GUILayout.Label("DISPLAY SCALE : " + painter.displayScale);
    painter.displayScale = GUILayout.HorizontalSlider(painter.displayScale, 1.0F, 2.0F);
    EditorGUILayout.Space();
    EditorGUILayout.Space();

    GUILayout.Label("PaintSize : " + painter.paintSize);
    painter.paintSize = GUILayout.HorizontalSlider(painter.paintSize, 0.0F, 100.0F);

    EditorGUILayout.Space();
    GUILayout.Label("Paint Opacity: " + painter.paintOpacity);
    painter.paintOpacity = GUILayout.HorizontalSlider(painter.paintOpacity, 0.0F, 1.0F);

    EditorGUILayout.Space();
    GUILayout.Label("Paint STRENGTH: " + painter.paintStrength);
    GUILayout.Label("Shift " + painter.shift);
    GUILayout.Label("FN " + painter.fn);

    EditorGUILayout.Space();

    EditorGUILayout.Space();


    if (GUILayout.Button("Reset To Original"))
    {
      //painter.ResetToOriginal();
      // painter.Save();
    }

    if (GUILayout.Button("Set To Current"))
    {
      painter.UltraSave();

    }

    /*if (GUILayout.Button("Reset To Flat"))
   {
     painter.ResetToFlat();
     painter.Save();
   }*/

    if (GUILayout.Button("UNDO"))
    {
      //painter.Undo();
    }

    if (GUILayout.Button("REDO"))
    {
      //painter.Redo();
    }



    painter.brushType = EditorGUILayout.Popup("Label", painter.brushType, painter.verts.dataTypes);
    EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();

    DrawDefaultInspector();

  }
}