using UnityEditor;
using UnityEngine;
using IMMATERIA;

// Specify that the custom editor should be used for the Form class and all derived classes
[CustomEditor(typeof(Form), true)]
public class FormEditor : Editor
{
    public override void OnInspectorGUI()
    {


        // Add your custom editor functionality here
        // For example, you could draw a button that performs a specific action when clicked
        if (GUILayout.Button("SAVE"))
        {
            Saveable.Save((Form)target);
        }


        // Add your custom editor functionality here
        // For example, you could draw a button that performs a specific action when clicked
        if (GUILayout.Button("LOAD"))
        {
            Saveable.Load((Form)target);
        }


        // Call the base class method to draw the default inspector
        base.OnInspectorGUI();


    }
}