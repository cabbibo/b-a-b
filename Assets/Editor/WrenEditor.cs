
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Wren))]
[CanEditMultipleObjects]
public class WrenEditor: Editor 
{


int paramID;
int oParamID;
    public override void OnInspectorGUI()
    {
        
        

      /*  Wren wren = (Wren)target;
        if(GUILayout.Button("Save Params As New")){
           wren.parameters.SaveNewParamSet();
        }

        if(GUILayout.Button("Save Current")){
           wren.parameters.Save();
        }

        wren.parameters.paramID = EditorGUILayout.Popup(wren.parameters.paramID,wren.parameters.paramFiles);*/
        DrawDefaultInspector();



    }

}