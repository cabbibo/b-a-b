using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class OverallCameraManager : MonoBehaviour
{
    public LerpTo                 lerpManager;
    public SlideCameraManager     slideManager;
    public TargetingCameraManager targetingManager;

    public CinematicCameraManager cinematicManager;

    public PointOfInterestCameraController pointOfInterestManager;
    public CutSceneCameraManager           cutSceneManager;

    public BaseCameraManager[] cameraManagers;

    public float[]           normalizedWeights;
    public BaseCameraManager defaultPriority;

    public Transform cameraTransform;

    public Camera camera;
    public float  totalWeight;


    public BaseCameraManager currentPriority;


    public float priorityRequestSpeed        = .01f;
    public float priorityRequestSpeedDefault = .01f;


    public float priorityRequestTime;


    // Use the weights of each value to decide where our camera goes!
    public void FixedUpdate()
    {


        if ( currentPriority == null ) {
            currentPriority = defaultPriority;
        }

        if ( normalizedWeights == null ) {
            normalizedWeights = new float[cameraManagers.Length];

            for ( int i = 0; i < cameraManagers.Length; i++ ) {
                normalizedWeights[i] = cameraManagers[i].weight;
            }
        }

        if ( normalizedWeights.Length != cameraManagers.Length ) {
            normalizedWeights = new float[cameraManagers.Length];

            for ( int i = 0; i < cameraManagers.Length; i++ ) {
                normalizedWeights[i] = cameraManagers[i].weight;
            }
        }


        // calculate weights
        FadeWeights();

        // normalizeWeights
        NormalizeWeights();

        float totalFOV = 0;
        var totalPos = Vector3.zero;
        var totalForward = Vector3.zero;
        var totalUp = Vector3.zero;


        for ( int i = 0; i < cameraManagers.Length; i++ ) {

            if ( cameraManagers[i].weight > 0 ) {
                cameraManagers[i].WhileInUse();

                totalPos += cameraManagers[i].transform.position * normalizedWeights[i];
                totalFOV += cameraManagers[i].FOV * normalizedWeights[i];
                totalForward += cameraManagers[i].transform.forward * normalizedWeights[i];
                totalUp += cameraManagers[i].transform.up * normalizedWeights[i];

            }

        }


        cameraTransform.position = totalPos;
        cameraTransform.rotation = Quaternion.LookRotation( totalForward , totalUp );

        camera.fieldOfView = totalFOV;


    }

    public void NormalizeWeights()
    {

        float totalWeight = 0;

        for ( int i = 0; i < cameraManagers.Length; i++ ) {
            totalWeight += cameraManagers[i].weight;
        }

        for ( int i = 0; i < cameraManagers.Length; i++ ) {
            normalizedWeights[i] = cameraManagers[i].weight / totalWeight;
        }

    }

    public void FadeWeights()
    {

        for ( int i = 0; i < cameraManagers.Length; i++ ) {
            if ( cameraManagers[i] != currentPriority ) {
                cameraManagers[i].weight = Mathf.Lerp( cameraManagers[i].weight , 0 , priorityRequestSpeed );
            } else {
                cameraManagers[i].weight = Mathf.Lerp( cameraManagers[i].weight , 1 , priorityRequestSpeed );
            }
        }


    }


    public void RequestPriority( BaseCameraManager cm , float fadetime )
    {
        if ( currentPriority != cm ) {
            currentPriority = cm;
            priorityRequestTime = Time.time;
            priorityRequestSpeed = fadetime;
        } else {
            Debug.LogWarning( "Already have priority" );
        }
    }


    public void RequestPriority( BaseCameraManager cm )
    {
        if ( currentPriority != cm ) {
            currentPriority = cm;
            priorityRequestTime = Time.time;
            priorityRequestSpeed = priorityRequestSpeedDefault;
        } else {
            Debug.LogWarning( "Already have priority" );
        }
    }


    public void ReleasePriority( BaseCameraManager cm , float fadeTime )
    {

        if ( currentPriority == cm ) {
            // dont actullay need fade time if we want default
            currentPriority = defaultPriority;
            priorityRequestTime = Time.time;
            priorityRequestSpeed = fadeTime;
        } else {
            Debug.LogWarning( "Already released priority" );
        }

    }

    public void ReleasePriority( BaseCameraManager cm )
    {

        if ( currentPriority == cm ) {
            currentPriority = defaultPriority;
            priorityRequestTime = Time.time;
            priorityRequestSpeed = priorityRequestSpeedDefault;
        } else {
            Debug.LogWarning( "Already released priority" );
        }


    }


    public void PhaseShift()
    {

        lerpManager.transform.position = God.wren.cameraWork.camTarget.position;
        lerpManager.transform.rotation = God.wren.cameraWork.camTarget.rotation;

    }
}