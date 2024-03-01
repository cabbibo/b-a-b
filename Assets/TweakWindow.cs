using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TweakWindow : EditorWindow
{
    private SerializedObject serializedObject;
    private SerializedProperty targetObjectProperty;
    private SerializedProperty valuesProperty;

    [System.Serializable]
    public struct Value
    {
        public GameObject targetObject;
        public string propertyName;
    }

    [SerializeField] GameObject targetObject;
    [SerializeField] List<Value> values = new List<Value>();

    [MenuItem("Window/TweakWindow")]
    public static void ShowWindow()
    {
        GetWindow<TweakWindow>("Tweak Window");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        targetObjectProperty = serializedObject.FindProperty("targetObject");
        valuesProperty = serializedObject.FindProperty("values");
    }

    private void OnGUI()
    {
        serializedObject.Update();

        using (new EditorGUILayout.VerticalScope())
        {
            EditorGUILayout.PropertyField(targetObjectProperty);

            for (int i = 0; i < valuesProperty.arraySize; i++)
            {
                SerializedProperty valueProperty = valuesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(valueProperty, new GUIContent("Values!"));
            }

            if (GUILayout.Button("Add Value"))
            {
                valuesProperty.arraySize++;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}