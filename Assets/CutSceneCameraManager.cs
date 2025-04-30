using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CutSceneCameraManager : BaseCameraManager
{
    public float stealSpeed  = 1;
    public float returnSpeed = 1;


    public PlayCutScene cutScene;

    public Vector3    startPos;
    public Quaternion startRot;

    public Vector3    targetPos;
    public Quaternion targetRot;


    // Start is called before the first frame update
    private void Start()
    {

    }


    public void Update()
    {

        if ( cutScene == null ) {
            return;
        }

        if ( cutScene.playing ) {
            transform.position = cutScene.cameraTarget.position;
            transform.rotation = cutScene.cameraTarget.rotation;


        }


    }


    public float transitionStartTime;


    public void SetCutScene( PlayCutScene cs )
    {

        if ( cutScene != null ) {
            Debug.LogError( "Already a cut scene playing ya DOINK" );
        }

        RequestPriority();


        FOV = cs.FOV;
        cutScene = cs;


        startPos = Camera.main.transform.position;
        startRot = Camera.main.transform.rotation;
        targetPos = cutScene.cameraTarget.position;
        targetRot = cutScene.cameraTarget.rotation;

        transform.position = startPos;
        transform.rotation = startRot;

        transitionStartTime = Time.time;

        StartCoroutine( TransitionIn() );

    }

    public void OnCutSceneFinishedPlaying()
    {

//        print( "CutSceneFinishedPlaying" );
        transitionStartTime = Time.time;

        startPos = Camera.main.transform.position;
        startRot = Camera.main.transform.rotation;
        targetPos = God.wren.cameraWork.camTarget.position;
        targetRot = God.wren.cameraWork.camTarget.rotation;

        StartCoroutine( TransitionOut() );
    }

    private IEnumerator TransitionIn()
    {


        while (Time.time - transitionStartTime < cutScene.transitionInSpeed) {

            float nTime = (Time.time - transitionStartTime) / cutScene.transitionInSpeed;
            transform.position = Vector3.Lerp( startPos , targetPos , nTime );
            transform.rotation = Quaternion.Lerp( startRot , targetRot , nTime );
            yield return null;
        }

        OnTransitionInComplete();
    }


    public void OnTransitionInComplete()
    {
//        print( "transition in complete" );
        cutScene.OnTransitionInComplete();
    }


    private IEnumerator TransitionOut()
    {

        while (Time.time - transitionStartTime < cutScene.transitionInSpeed) {

            //      print( "transitioning out" );
            float nTime = (Time.time - transitionStartTime) / cutScene.transitionInSpeed;
            transform.position = Vector3.Lerp( startPos , targetPos , nTime );
            transform.rotation = Quaternion.Lerp( startRot , targetRot , nTime );
            yield return null;

        }

        OnTransitionOutComplete();

    }

    public void OnTransitionOutComplete()
    {
//        print( "transition out complete" );
        // overallManager.lerpManager.transform.position = transform.position;
        // overallManager.lerpManager.transform.rotation = transform.rotation;
        ReleasePriority();
        cutScene.OnTransitionOutComplete();
        cutScene = null;

    }
}