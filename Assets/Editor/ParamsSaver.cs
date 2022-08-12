//C# Example (LookAtPointEditor.cs)
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WrenParams))]
[CanEditMultipleObjects]
public class ParamsSaver : Editor 
{
    SerializedProperty currentName;
    
    void OnEnable()
    {
       currentName = serializedObject.FindProperty("currentName");
    }

    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();


        WrenParams myScript = (WrenParams)target;
        if(GUILayout.Button("Save")){
            myScript.Save();
        }
        if(GUILayout.Button("Load")){
            myScript.Load();
        }
    }
}