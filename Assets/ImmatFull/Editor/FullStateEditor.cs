using UnityEngine;
using UnityEditor;
using IMMATERIA;

[CustomEditor(typeof(FullState))]
public class FullStateEditor : Editor
{
    public override void OnInspectorGUI()
    {


        FullState fullState = (FullState)target;
        if (GUILayout.Button("SaveState"))
        {
            fullState.UpdateState();
        }

        if (GUILayout.Button("LoadState"))
        {
            fullState.LoadState();
        }



        if (GUILayout.Button("ResetState"))
        {
            fullState.ResetAll();
        }




        DrawDefaultInspector();


    }





}