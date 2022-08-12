using UnityEngine;
using System.Collections;
using UnityEditor;
using MagicCurve;

[CustomEditor(typeof(CurvesOnCurve)), CanEditMultipleObjects]
public class CurvesOnCurveEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        CurvesOnCurve curve = (CurvesOnCurve)target;

        DrawDefaultInspector();

  
        if(GUILayout.Button("Save Mesh"))
        {

            string name = "Assets/Plugins/Curves/SavedMeshes/CurvesOnCurve_" + Random.Range(0,12141414) + ".asset";
            Debug.Log(name);

            Mesh m;
            m = curve.gameObject.GetComponent<MeshFilter>().sharedMesh;
            AssetDatabase.CreateAsset(m,name);
        }
      
    }
}