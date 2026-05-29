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

           if (GUILayout.Button("Previous Shot"))
        {
            manager.PrevShot();
        }


          if (GUILayout.Button("Reset Current Shot"))
        {
            manager.ResetCurrentShot();
        }
        base.OnInspectorGUI();



    }
}

#endif

[ExecuteAlways]
public class CameraShotManager : MonoBehaviour
{


    public bool useJustTransforms;
    public CameraShot[] shots;
    public Transform[] cameraLocations;
    public int currentShotIndex = 0;
    public int oShotIndex;
    public Transform currentLocation;


    public bool oscillate;


    public float oscillateSize;
    public float oscillateSpeed;

    public float lookForward;



    public bool getShotFromTimelinePlayback;

    public TimelinePlayback timelinePlayback;


    public new AudioSource audio;


    public void NextShot()
    {

        currentShotIndex++;

        if( useJustTransforms ){
        if (currentShotIndex >= cameraLocations.Length)
        {
            currentShotIndex = 0;
        }

        currentLocation = cameraLocations[currentShotIndex];
        }else{
            shots[currentShotIndex-1].Unset();


            if (currentShotIndex >= shots.Length)
            {
                currentShotIndex = 0;
              
            }

              if (shots.Length > 0)
                {
                    shots[currentShotIndex].Set();
                }
        }


    }

    public void PrevShot()
    {

        currentShotIndex--;

        if( useJustTransforms ){
        if (currentShotIndex < 0)
        {
            currentShotIndex = cameraLocations.Length-1;
        }

        currentLocation = cameraLocations[currentShotIndex];
        }else{


            
            shots[currentShotIndex+1].Unset();


            if (currentShotIndex < 0)
            {
                currentShotIndex = shots.Length-1;
              
            }

              if (shots.Length > 0)
                {
                    shots[currentShotIndex].Set();
                }
        }


    }

    public void ResetCurrentShot()
    {
        if( useJustTransforms ){
            currentLocation = cameraLocations[currentShotIndex];
        }else{
            shots[currentShotIndex].Unset();
            shots[currentShotIndex].Set();
        }
    }


    // Update is called once per frame
    void Update()
    {


        
        God.camera.transform.position = currentLocation.position;
        God.camera.transform.rotation = currentLocation.rotation;

        if (oscillate)
        {
            God.camera.transform.position = currentLocation.position+ God.camera.transform.right * Mathf.Sin(Time.time * oscillateSpeed) * oscillateSize;
            God.camera.transform.LookAt(currentLocation.position + currentLocation.forward * lookForward);
        }

        if( !useJustTransforms ){
            if (shots.Length > 0 && shots.Length > currentShotIndex)
            {
                
                shots[currentShotIndex].SetValues();
            }
        }

        if( getShotFromTimelinePlayback ){
            oShotIndex = currentShotIndex;
            currentShotIndex = timelinePlayback.whichTrack;

            if( currentShotIndex != oShotIndex ){
                if( useJustTransforms ){
                    currentLocation = cameraLocations[currentShotIndex];
                }else{
                    shots[oShotIndex].Unset();
                    shots[currentShotIndex].Set();
                }
            }
        }

    }

    public void PlayAudio()
    {
        if( audio != null ){
            audio.Play();
        }
    }
}
