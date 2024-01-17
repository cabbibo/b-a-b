using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CinematicCameraDescriptor))]
public class CinematicCameraDescriptorEditor : Editor
{
    void OnDisable()
    {
        (target as CinematicCameraDescriptor).debugArmed = false;
    }
}
#endif



public class CinematicCameraDescriptor : MonoBehaviour
{

  public bool debugArmed = false;

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
      LookAt
  }
  public Rotation rotation = Rotation.LookAt;


    void Start()
    {
        debugArmed = false;
    }
}
