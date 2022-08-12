using UnityEngine;
using System.Collections;
using UnityEditor;
using MagicCurve;

[CustomEditor(typeof(RibbonAlongCurve)), CanEditMultipleObjects]
public class RibbonAlongCurveEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        RibbonAlongCurve curve = (RibbonAlongCurve)target;

        DrawDefaultInspector();

  
        if(GUILayout.Button("Save Mesh"))
        {

            string name = "Assets/Plugins/Curves/SavedMeshes/RibbonAlongCurve_" + Random.Range(0,12141414) + ".asset";
            Debug.Log(name);

            Mesh m;
            m = curve.gameObject.GetComponent<MeshFilter>().sharedMesh;
            AssetDatabase.CreateAsset(m,name);
        }
      
    }
}