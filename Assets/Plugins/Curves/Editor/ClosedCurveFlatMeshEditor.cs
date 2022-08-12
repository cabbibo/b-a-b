using UnityEngine;
using System.Collections;
using UnityEditor;
using MagicCurve;

[CustomEditor(typeof(ClosedCurveFlatMesh)), CanEditMultipleObjects]
public class ClosedCurveFlatMeshEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        ClosedCurveFlatMesh curve = (ClosedCurveFlatMesh)target;

        DrawDefaultInspector();

  
        if(GUILayout.Button("Save Mesh"))
        {

            string name = "Assets/Plugins/Curves/SavedMeshes/ClosedCurveFlatMesh_" + Random.Range(0,12141414) + ".asset";
            Debug.Log(name);

            Mesh m;
            m = curve.gameObject.GetComponent<MeshFilter>().sharedMesh;
            AssetDatabase.CreateAsset(m,name);
        }
      
    }
}