using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


[CustomEditor(typeof(Waterfall))]
public class WaterfallEditor : Editor
{

    public void OnSceneGUI()
    {

        /*Vector2 dpi = DPIHelper.GetSystemDPI();
        Debug.Log($"System DPI: X = {dpi.x}, Y = {dpi.y}");

        Debug.Log(dpi.x / 96.0f);*/


        Waterfall placer = (Waterfall)target;


        // What is this?
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {

            Vector2 mousePos = Event.current.mousePosition * placer.displayScale;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Ray ray = Camera.current.ScreenPointToRay(mousePos);
            placer.MouseDown(ray);

        }


    }


    public override void OnInspectorGUI()
    {

        Waterfall placer = (Waterfall)target;



        GUILayout.Label("DISPLAY SCALE : " + placer.displayScale);
        placer.displayScale = GUILayout.HorizontalSlider(placer.displayScale, 1.0F, 2.0F);
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Reset"))
        {
            placer.Reset();
        }

        if (GUILayout.Button("Regenerate"))
        {
            placer.Rengenerate();
        }


        DrawDefaultInspector();

    }



}

