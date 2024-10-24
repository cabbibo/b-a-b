using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;
using IMMATERIA;

[CustomEditor(typeof(BindEditorMouseInformation))]
public class BindEditorMouseInformationEditor : Editor
{
  //private static bool m_editMode = false;
  //private static bool m_editMode2 = false;

  void OnSceneGUI()
  {

    //       Debug.Log("PRSITN444");


    BindEditorMouseInformation test = (BindEditorMouseInformation)target;



    /*if( Event.current.type== EventType.KeyDown ){

        if(Event.current.keyCode == KeyCode.Z){ test.fn = -1; }
        if(Event.current.keyCode == KeyCode.X){ test.shift = -1; }
        if(Event.current.keyCode == KeyCode.C){ test.paintStrength = -1; }

    }

    if( Event.current.type == EventType.KeyUp ){

        if(Event.current.keyCode == KeyCode.Z){ test.fn = 1; }
        if(Event.current.keyCode == KeyCode.X){ test.shift = 1; }
        if(Event.current.keyCode == KeyCode.C){ test.paintStrength = 1; }

    }*/



    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
    {


      //print("mouse");

      Vector2 mousePos = Event.current.mousePosition * test.DPI;
      mousePos.y = Camera.current.pixelHeight - mousePos.y;
      Ray ray = Camera.current.ScreenPointToRay(mousePos);
      test.MouseDown(ray);

      test.mousePosition = mousePos;
      test.oMousePosition = mousePos;

      test.screenDirection = Vector2.zero;

      test.mouseDown = true;

    }

    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
    {

      //print("mouse");
      Vector2 mousePos = Event.current.mousePosition * test.DPI;
      mousePos.y = Camera.current.pixelHeight - mousePos.y;
      Ray ray = Camera.current.ScreenPointToRay(mousePos);
      test.WhileDown(ray);


      test.mouseDown = true;

      // test.oSP = test.paintScreenPosition;
      // test.paintScreenPosition = mousePos;


      test.oMousePosition = test.mousePosition;
      test.mousePosition = mousePos;

      test.screenDirection = test.mousePosition - test.oMousePosition;

    }



    if (Event.current.type == EventType.MouseUp)
    {
      //test.Save();
      test.OnMouseUp();

      test.Save();
      test.mouseDown = false;

    }


  }
  public override void OnInspectorGUI()
  {


    // Debug.Log("PRSITN");
    BindEditorMouseInformation test = (BindEditorMouseInformation)target;
    if (GUILayout.Button("DAVID!"))
    {
      //test.ResetToOriginal();
      test.Save();
    }


    /*
            GUILayout.Label("PaintSize : " + test.paintSize);
            test.paintSize = GUILayout.HorizontalSlider(test.paintSize, 0.0F, 100.0F);

         EditorGUILayout.Space();
            GUILayout.Label("Paint Opacity: " + test.paintOpacity);
            test.paintOpacity = GUILayout.HorizontalSlider(test.paintOpacity, 0.0F, 1.0F);

              EditorGUILayout.Space();
            GUILayout.Label("Paint STRENGTH: " + test.paintStrength);
            GUILayout.Label("Shift " + test.shift);
            GUILayout.Label("FN " + test.fn);

         EditorGUILayout.Space();

         EditorGUILayout.Space();


            if (GUILayout.Button("Reset To Original"))
            {
              //test.ResetToOriginal();
             // test.Save();
            }

            if (GUILayout.Button("Set To Current"))
            {
              test.UltraSave();

            }


            if (GUILayout.Button("UNDO"))
            {
              //test.Undo();
            }

            if (GUILayout.Button("REDO"))
            {
              //test.Redo();
            }



            test.brushType = EditorGUILayout.Popup("Label", test.brushType, test.verts.dataTypes); 
            EditorGUILayout.Space();EditorGUILayout.Space();EditorGUILayout.Space();EditorGUILayout.Space();EditorGUILayout.Space();EditorGUILayout.Space();EditorGUILayout.Space();
    */


    DrawDefaultInspector();

  }
}