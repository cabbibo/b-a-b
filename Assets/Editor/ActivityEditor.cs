using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


[CustomEditor(typeof(Activity))]
public class ActivityEditor : Editor
{

    public GameObject prefab;

    public override void OnInspectorGUI()
    {

        Activity activity = (Activity)target;



        if (GUILayout.Button("Full State Reset"))
        {
            activity.FullStateReset();
        }

        if (GUILayout.Button("Save State"))
        {
            activity.SaveState();
        }

        if (GUILayout.Button("Load State"))
        {
            activity.LoadState();
        }


        DrawDefaultInspector();

    }

}
