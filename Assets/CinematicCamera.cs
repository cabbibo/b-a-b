using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CinematicCamera))]
public class CinematicCameraEditor : Editor
{
    void OnDisable()
    {
        (target as CinematicCamera).debugArmed = false;
    }

    override public void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif



public class CinematicCamera : MonoBehaviour
{

    public CinematicCameraHandler.CameraDescriptor descriptor = new CinematicCameraHandler.CameraDescriptor();

    public bool debugArmed = false;

    void Start()
    {
        debugArmed = false;
    }
}
