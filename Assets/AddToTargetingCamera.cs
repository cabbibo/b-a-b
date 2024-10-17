using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class AddToTargetingCamera : MonoBehaviour
{

    public Vector2 nearFar = new Vector2(10, 100);
    public float FOV = 60;
    public float distanceFromBird = 10;

    public void OnEnable()
    {

        God.cameraManager.targetingManager.AddTarget(transform, nearFar, distanceFromBird, FOV);

    }
}
