using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

public class CinematicCameraHandler : MonoBehaviour
{
    
    Transform BirdTransform { get { 
        return God.wren.cameraWork.transform;
    }}
    
    Transform Head { get { 
        return God.wren.bird.head.transform;
    }}

    public bool armed = false;

    [System.Serializable] public class CinematicCamera { 
        public string Name = "";
        public Vector3 posOffset = new Vector3(0,0.6f,-2);
        public Vector3 lookOffset = new Vector3(0,0,.8f);
        public float fov = 80;
        public bool orbit = true;
        [Range(0,1)] public float orbitAngle = 0;
        public float orbitRadius = 2;
        public float orbitHeight = 1f;

        public enum BodyTarget {
            None,
            Head
        }
        public BodyTarget bodyTarget = BodyTarget.None;
        public BodyTarget aimTarget = BodyTarget.None;
        public bool parentToBird = false;

    }
    public CinematicCamera[] tutorialCameras;
    public int tutorialCameraIdx;


    void LateUpdate()
    {
        if (!God.wren)
            return;

        if (armed && tutorialCameras.Length > 0)
        {
            var c = tutorialCameras[(int)Mathf.Clamp(tutorialCameraIdx, 0, tutorialCameras.Length-1)];
            
            GetCustomCameraPositions(c, out var cPos, out var tPos);

            God.camera.transform.position = cPos;
            God.camera.transform.LookAt(tPos, Vector3.up);
            God.camera.fieldOfView = c.fov;

        }

    }

    void GetCustomCameraPositions(CinematicCamera cam, out Vector3 camPos, out Vector3 camTgt)
    {
      camPos = BirdTransform.position + cam.posOffset;
      camTgt = BirdTransform.position + cam.lookOffset;


      
      if (cam.bodyTarget == CinematicCamera.BodyTarget.Head)
      {
        camPos = Head.position;
      }
      
      if (cam.aimTarget == CinematicCamera.BodyTarget.Head)
      {
        camTgt = Head.position;
      }

      if (cam.orbit)
      {
        camPos = camPos + new Vector3(
          Mathf.Sin(Mathf.Lerp(-Mathf.PI, Mathf.PI, cam.orbitAngle)) * cam.orbitRadius,
          cam.orbitHeight,
          Mathf.Cos(Mathf.Lerp(-Mathf.PI, Mathf.PI, cam.orbitAngle)) * cam.orbitRadius
        );
      }

    }
}
