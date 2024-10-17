using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class OverallCameraManager : MonoBehaviour
{

    public LerpTo lerpManager;
    public SlideCameraManager slideManager;
    public TargetingCameraManager targetingManager;

    public Transform cameraTransform;

    public Camera camera;
    public float totalWeight;

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

        totalWeight = w1 + w2 + w3;

        pos = (t1.position * w1 + t2.position * w2 + t3.position * w3) / totalWeight;
        fov = (fov1 * w1 + fov2 * w2 + fov3 * w3) / totalWeight;


        Vector3 forward = (t1.forward * w1 + t2.forward * w2 + t3.forward * w3) / totalWeight;
        Vector3 up = (t1.up * w1 + t2.up * w2 + t3.up * w3) / totalWeight;

        rot = Quaternion.LookRotation(forward, up);


        // TODO something to make sure that the bird is always in view?



        cameraTransform.position = pos;
        cameraTransform.rotation = rot;
        //cameraTransform.LookAt(God.wren.transform.position);

        //cameraTransform.rotation = rot;
        camera.fieldOfView = fov;



    }


}
