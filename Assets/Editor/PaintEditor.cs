 using UnityEngine;
 using System.Collections;
 using System.Reflection;
 using UnityEditor;
 
 [CustomEditor(typeof(Painter))]
 public class PainterEditor : Editor {
     //private static bool m_editMode = false;
     //private static bool m_editMode2 = false;
     
     void OnSceneGUI()
     {

//       Debug.Log("PRSITN444");


         Painter test = (Painter)target;
         
        if(Event.current.control)
{
   test.paintStrength = -1;
}else{
    test.paintStrength = 1;
}

 HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

             if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
             {

              //print("mouse");
                 Vector2 mousePos = Event.current.mousePosition * 1;
                 mousePos.y = Camera.current.pixelHeight - mousePos.y;
                 Ray ray = Camera.current.ScreenPointToRay(mousePos);
                 test.WhileDown(ray);

             }



             if (Event.current.type == EventType.MouseUp)
             {
              //test.Save();
 Debug.Log("Event Type : "  + Event.current.type );
 
             }
         
         
     }
     public override void OnInspectorGUI()
     {

       
      // Debug.Log("PRSITN");
        Painter test = (Painter)target;


        GUILayout.Label("PaintSize : " + test.paintSize);
        test.paintSize = GUILayout.HorizontalSlider(test.paintSize, 0.0F, 100.0F);
       
     EditorGUILayout.Space();
        GUILayout.Label("Paint Opacity: " + test.paintOpacity);
        test.paintOpacity = GUILayout.HorizontalSlider(test.paintOpacity, 0.0F, 1.0F);

          EditorGUILayout.Space();
        GUILayout.Label("Paint STRENGTH: " + test.paintStrength);
        test.paintStrength = GUILayout.HorizontalSlider(test.paintStrength, -1.0F, 1.0F);
        
     EditorGUILayout.Space();
     
     EditorGUILayout.Space();


        if (GUILayout.Button("Reset To Original"))
        {
          //test.ResetToOriginal();
         // test.Save();
        }

        if (GUILayout.Button("Set To Current"))
        {
          //test.UltraSave();
        }

         /*if (GUILayout.Button("Reset To Flat"))
        {
          test.ResetToFlat();
          test.Save();
        }*/

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

        DrawDefaultInspector ();
       
     }
 }