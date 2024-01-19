using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;


public class CinematicCameraHandler : MonoBehaviour
{

  Transform BirdTransform
  {
    get
    {
      return God.wren.cameraWork.transform;
    }
  }

  Transform Head
  {
    get
    {
      return God.wren.bird.head.transform;
    }
  }
  Vector3 EyePositionRight
  {
    get
    {
      return God.wren.bird.head.transform.TransformPoint(-.0562f, .0113f, .031f);
    }
  }

  public bool armed = false;

  [System.Serializable]
  public class CinematicCamera
  {
    public string Name = "";
    public Vector3 posOffset = new Vector3(0, 0.6f, -2);
    public Vector3 lookOffset = new Vector3(0, 0, .8f);
    public float fov = 80;
    public bool orbit = true;
    [Range(0, 1)] public float orbitAngle = 0;
    public float orbitRadius = 2;
    public float orbitHeight = 1f;

    public enum BodyTarget
    {
      None,
      Head
    }
    public BodyTarget bodyTarget = BodyTarget.None;
    public BodyTarget aimTarget = BodyTarget.None;
    public bool parentToBird = false;

  }
  public CinematicCamera[] tutorialCameras;
  public CinematicCameraDescriptor[] _cams;
  public int tutorialCameraIdx;

  void Start()
  {
    _cams = GetComponentsInChildren<CinematicCameraDescriptor>();
  }

  void LateUpdate()
  {
    if (!God.wren)
      return;

    if (armed && _cams.Length > 0)
    {
      var c = _cams[(int)Mathf.Clamp(tutorialCameraIdx, 0, _cams.Length - 1)];


#if UNITY_EDITOR
      if (Application.isEditor)
      {
        foreach (var ct in GetComponentsInChildren<CinematicCameraDescriptor>())
        {
          if (ct.debugArmed)
          {
            c = ct;
          }
        }
      }


#endif

      GetCustomCameraPositions(c, out var cPos, out var tPos);

    }

  }

  Transform GetTarget(CinematicCameraDescriptor.BodyTarget target)
  {
    switch (target)
    {
      case CinematicCameraDescriptor.BodyTarget.Head:
      case CinematicCameraDescriptor.BodyTarget.Eye:
        return Head;
    }
    return BirdTransform;
  }

  void GetCustomCameraPositions(CinematicCameraDescriptor cam, out Vector3 pos, out Vector3 lookPos)
  {

    var bodyTarget = GetTarget(cam.bodyTarget);
    var aimTarget = GetTarget(cam.aimTarget);

    pos = cam.posOffset;
    lookPos = aimTarget.position + cam.aimPosOffset;
    if (cam.aimTarget == CinematicCameraDescriptor.BodyTarget.Eye)
      lookPos = EyePositionRight + cam.aimPosOffset;

    if (cam.orbit)
    {
      var y = cam.orbitRadius * Mathf.Sin(cam.orbitHeight * Mathf.Deg2Rad);
      var r = cam.orbitRadius * Mathf.Cos(cam.orbitHeight * Mathf.Deg2Rad);
      var a = (cam.orbitAngle + .25f) % 1;
      pos += new Vector3(
        r * Mathf.Cos(Mathf.Lerp(-Mathf.PI, Mathf.PI, a)),
        y,
        r * Mathf.Sin(Mathf.Lerp(-Mathf.PI, Mathf.PI, a))
      );
    }

    // Position
    if (cam.parentToBird)
    {
      God.camera.transform.position = bodyTarget.TransformPoint(pos);
      God.camera.transform.rotation = bodyTarget.rotation;
      lookPos = aimTarget.TransformPoint(cam.aimPosOffset);
    }
    else
    {
      God.camera.transform.position = bodyTarget.position + pos;
      God.camera.transform.rotation = bodyTarget.rotation;
    }
    // Rotation
    if (cam.rotation == CinematicCameraDescriptor.Rotation.UseBodyRotation)
      God.camera.transform.rotation = bodyTarget.rotation;
    else if (cam.rotation == CinematicCameraDescriptor.Rotation.UseAimRotation)
      God.camera.transform.rotation = aimTarget.rotation;
    else if (cam.rotation == CinematicCameraDescriptor.Rotation.LookAt)
      God.camera.transform.LookAt(lookPos, Vector3.up);
    else
      God.camera.transform.LookAt(lookPos, bodyTarget.up);

    God.camera.transform.Rotate(cam.rotationOffset, Space.Self);

    // Etc
    God.camera.fieldOfView = cam.fov;

    cam.transform.position = God.camera.transform.position;
    cam.transform.rotation = God.camera.transform.rotation;

    Debug.DrawLine(cam.transform.position, lookPos);

  }

}