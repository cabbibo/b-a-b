using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(CameraShotManager))]
public class CameraShotManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CameraShotManager manager = (CameraShotManager)target;
        if (GUILayout.Button("Next Shot"))
        {
            manager.NextShot();
        }
        base.OnInspectorGUI();



    }
}

#endif

[ExecuteAlways]
public class CameraShotManager : MonoBehaviour
{

    public Transform[] cameraLocations;
    public int currentShotIndex = 0;

    public Transform currentLocation;


    public void NextShot()
    {

        currentShotIndex++;
        if (currentShotIndex >= cameraLocations.Length)
        {
            currentShotIndex = 0;
        }

        currentLocation = cameraLocations[currentShotIndex];


    }


    // Update is called once per frame
    void Update()
    {

        God.camera.transform.position = currentLocation.position;
        God.camera.transform.rotation = currentLocation.rotation;
    }
}
