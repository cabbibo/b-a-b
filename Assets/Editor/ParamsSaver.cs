//C# Example (LookAtPointEditor.cs)

using UnityEngine;
using UnityEditor;

[CustomEditor( typeof(WrenParams) )]
[CanEditMultipleObjects]
public class ParamsSaver : Editor
{
    private SerializedProperty currentName;

    private void OnEnable()
    {
        currentName = serializedObject.FindProperty( "currentName" );
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();


        var myScript = (WrenParams)target;

        if ( GUILayout.Button( "Load" ) ) {
            myScript.LoadPhysics();
        }

        if ( GUILayout.Button( "Next Param Set" ) ) {
            myScript.NextParam();
        }

        if ( GUILayout.Button( "Prev Param Set" ) ) {
            myScript.NextParam();
        }

        if ( GUILayout.Button( "Save As ScriptableOBject" ) ) {
            myScript.SaveCurrentAsScriptableObject();
        }
    }
}