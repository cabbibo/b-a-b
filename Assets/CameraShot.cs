using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CameraShot : MonoBehaviour
{


    public CameraShotManager cameraManager;
    public bool oscillate;


    public float oscillateSize;
    public float oscillateSpeed;

    public float lookForward;


    public Transform cameraTransform;
    public Transform lightPosition;
    public Transform lightLookTarget;

    public float timeOfDay;

    public SunManager sunManager;

    public PostParameters postParameters;




    public void Set(){

        cameraManager.currentLocation = cameraTransform;
        cameraManager.oscillate = oscillate;
        cameraManager.oscillateSize = oscillateSize;
        cameraManager.oscillateSpeed = oscillateSpeed;
        cameraManager.lookForward = lookForward;

        sunManager.rawTimeInCycle = timeOfDay;
        sunManager.lookTarget = lightLookTarget;
        sunManager.targetPosition = lightPosition;


        God.postController.SetPostParameters(postParameters);



    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
