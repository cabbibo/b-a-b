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

  [System.Serializable]
  public class CameraDescriptor
  {
      public float fov = 80;

      [Header("Orbit")]
      public bool orbit = true;
      [Range(0,1)] public float orbitAngle = 0;
      public float orbitRadius = 2;
      [Range(-90,90)]
      public float orbitHeight = 1f;

      public enum BodyTarget {
          None,
          Body,
          Head,
          Eye
      }
      public bool parentToBird = false;

      [Header("Targets")]
      public BodyTarget bodyTarget = BodyTarget.None;
      public Vector3 posOffset = new Vector3(0,0.6f,-2);
      public BodyTarget aimTarget = BodyTarget.None;
      public Vector3 aimPosOffset = new Vector3(0,0,.8f);
      public Vector3 rotationOffset = new Vector3(0,0,.8f);

      public enum Rotation {
          None,
          UseBodyRotation,
          UseAimRotation,
          LookAt,
          WorldUp
      }
      public Rotation rotation = Rotation.LookAt;


      public static CameraDescriptor Lerp(CameraDescriptor a, CameraDescriptor b, float t)
      {
          return new CameraDescriptor() {
              fov = Mathf.Lerp(a.fov, b.fov, t),
              orbit = t > .5f ? b.orbit : a.orbit,
              orbitAngle = Mathf.LerpAngle(a.orbitAngle, b.orbitAngle, t),
              orbitRadius = Mathf.Lerp(a.orbitRadius, b.orbitRadius, t),
              orbitHeight = Mathf.Lerp(a.orbitHeight, b.orbitHeight, t),
              parentToBird = t > .5f ? b.parentToBird : a.parentToBird,
              bodyTarget = a.bodyTarget,
              posOffset = Vector3.Lerp(a.posOffset, b.posOffset, t),
              aimTarget = a.aimTarget,
              aimPosOffset = Vector3.Lerp(a.aimPosOffset, b.aimPosOffset, t),
              rotationOffset = Vector3.Lerp(a.rotationOffset, b.rotationOffset, t),
              rotation = a.rotation
          };
      }
  }

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

  public CinematicCamera[] _cams;
  public float tutorialCameraIdx;
  public TutorialCardTrigger[] startTriggers;
  public Transform endTransform;

  public enum Mode
  {
    Disabled,
    Cinematic,
    Activities,
    TutorialEnd
  }
  Mode _mode = Mode.Disabled;
  float _lastModeChangeT;
  public Mode mode { get { return _mode; } set { _mode = value; _lastModeChangeT = Time.unscaledTime; } }

  void Start()
  {
    _cams = GetComponentsInChildren<CinematicCamera>();
  }

  void LateUpdate()
  {
    if (!God.wren)
      return;

    if (mode == Mode.Cinematic && _cams.Length > 0)
    {
      var c1 = _cams[(int)Mathf.Clamp(Mathf.Floor(tutorialCameraIdx), 0, _cams.Length - 1)];
      var c2 = _cams[(int)Mathf.Clamp(Mathf.Ceil(tutorialCameraIdx), 0, _cams.Length - 1)];


#if UNITY_EDITOR
      if (Application.isEditor)
      {
        foreach (var ct in GetComponentsInChildren<CinematicCamera>())
        {
          if (ct.debugArmed)
          {
            c1 = ct;
          }
        }
      }


#endif
      if (c1 == c2)
        GetCustomCameraPositions(c1.descriptor, out var cPos, out var tPos);
      else
        GetCustomCameraPositions(CameraDescriptor.Lerp(c1.descriptor, c2.descriptor, tutorialCameraIdx % 1), out var cPos, out var tPos);
      
      
      
    } else if (mode == Mode.Activities)
    {
      // orbit camera
      float radius = 120;
      var height = 15f;
      var y = radius * Mathf.Sin(height * Mathf.Deg2Rad);
      var r = radius * Mathf.Cos(height * Mathf.Deg2Rad);
      var a = (Time.time * .01f) % 1;
      var pos = new Vector3(
        r * Mathf.Cos(Mathf.Lerp(-Mathf.PI, Mathf.PI, a)),
        y,
        r * Mathf.Sin(Mathf.Lerp(-Mathf.PI, Mathf.PI, a))
      );
      float secPerTrigger = 3;
      var t = startTriggers[Mathf.FloorToInt(((Time.unscaledTime-_lastModeChangeT) / secPerTrigger) % startTriggers.Length)];
      God.camera.transform.position = t.transform.position + pos;
      God.camera.transform.rotation = Quaternion.LookRotation(t.transform.position - God.camera.transform.position, Vector3.up);
      God.camera.fieldOfView = 60;
    
    } else if (mode == Mode.TutorialEnd)
    {
      God.camera.transform.position = endTransform.position;
      God.camera.transform.rotation = endTransform.rotation;
      God.camera.fieldOfView = 70;
    }

  }

  Transform GetTarget(CameraDescriptor.BodyTarget target)
  {
    switch (target)
    {
      case CameraDescriptor.BodyTarget.Head:
      case CameraDescriptor.BodyTarget.Eye:
        return Head;
    }
    return BirdTransform;
  }

  void GetCustomCameraPositions(CameraDescriptor cam, out Vector3 pos, out Vector3 lookPos)
  {

    var bodyTarget = GetTarget(cam.bodyTarget);
    var aimTarget = GetTarget(cam.aimTarget);

    pos = cam.posOffset;
    lookPos = aimTarget.position + cam.aimPosOffset;
    if (cam.aimTarget == CameraDescriptor.BodyTarget.Eye)
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
    // Quaternion rotation = Quaternion.identity;
    // switch(cam.rotation)
    // {
    //   case CameraDescriptor.Rotation.UseBodyRotation:
    //     rotation = bodyTarget.rotation;
    //     break;
    //   case CameraDescriptor.Rotation.UseAimRotation:
    //     rotation = aimTarget.rotation;
    //     break;
    //   case CameraDescriptor.Rotation.LookAt:
    //     rotation = Quaternion.LookRotation(lookPos, Vector3.up);
    //     break;
    //   case CameraDescriptor.Rotation.WorldUp:
    //     rotation = Quaternion.identity;
    //     break;
    //     case CameraDescriptor.Rotation.None:
    //     rotation = Quaternion.LookRotation(lookPos, bodyTarget.up);
    //     break;
    // }
    // God.camera.transform.rotation = rotation;
    
    Quaternion rotation = Quaternion.identity;
    switch (cam.rotation)
    {
      case CameraDescriptor.Rotation.UseBodyRotation:
        rotation = bodyTarget.rotation;
        break;
      case CameraDescriptor.Rotation.UseAimRotation:
        rotation = aimTarget.rotation;
        break;
      case CameraDescriptor.Rotation.LookAt:
        rotation = Quaternion.LookRotation(lookPos - God.camera.transform.position, Vector3.up);
        break;
      case CameraDescriptor.Rotation.WorldUp:
        // use forward of body target but keep level with horizon
        rotation = Quaternion.LookRotation(lookPos - God.camera.transform.position, Vector3.up);
        rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        break;
      default:
        rotation = Quaternion.LookRotation(lookPos - God.camera.transform.position, bodyTarget.up);
        break;
    }
    God.camera.transform.rotation = rotation;

    God.camera.transform.Rotate(cam.rotationOffset, Space.Self);

    // Etc
    God.camera.fieldOfView = cam.fov;

    // cam.transform.position = God.camera.transform.position;
    // cam.transform.rotation = God.camera.transform.rotation;

    // Debug.DrawLine(cam.transform.position, lookPos);

  }

}