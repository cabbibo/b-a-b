using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverallCameraManager : MonoBehaviour
{

    public LerpTo lerpManager;
    public SlideCameraManager slideManager;
    public TargetingCameraManager targetingManager;

    public Transform cameraTransform;

    public Camera camera;

    // Use the weights of each value to decide where our camera goes!
    public void LateUpdate()
    {

        Transform t1 = lerpManager.transform;
        Transform t2 = slideManager.transform;
        Transform t3 = targetingManager.transform;

        float w1 = lerpManager.weight;
        float w2 = slideManager.weight;
        float w3 = targetingManager.weight;

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        float fov1 = lerpManager.FOV;
        float fov2 = slideManager.FOV;
        float fov3 = targetingManager.FOV;


        float fov = 0;

        // Camera gets straight set to most important weight!
        if (w1 > w2 && w1 > w3)
        {
            pos = t1.position;
            rot = t1.rotation;
            fov = fov1;
        }

        if (w2 > w1 && w2 > w3)
        {
            pos = t2.position;
            rot = t2.rotation;
            fov = fov2;
        }

        if (w3 > w1 && w3 > w2)
        {
            pos = t3.position;
            rot = t3.rotation;
            fov = fov3;
        }


        cameraTransform.position = pos;
        cameraTransform.rotation = rot;
        camera.fieldOfView = fov;



    }


}
