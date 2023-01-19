﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using WrenUtils;

[ExecuteAlways]
public class PlayCutScene : MonoBehaviour
{


    public float speedMultiplier = 1;
    public UnityEvent CutSceneFinished;
    public UnityEvent CutSceneStarted;
    
    public Camera main;

    public Transform cameraTarget;
    public Transform wrenTarget;
    public float lerpSpeed = 1;
    public float slerpSpeed = 1;


    public TimelineAsset timeline;
    public bool playOnce;
    public float transitionInSpeed;
    public float transitionOutSpeed;
    
    LerpTo lerpTo;
    GlitchHit glitch;
    PlayableDirector director;

    float tmpLerpSpeed;
    float tmpSlerpSpeed;
    Transform tmpLerpTarget;

    


    

    bool played;


    // Start is called before the first frame update
    void Awake()
    {

        
       // if( Camera.main == null ){ Camera.main = Camera.Camera.main; }
        director = GetComponent<PlayableDirector>();
        director.played += Director_Played;
        director.stopped += Director_Stopped;



        if( timeline != null ){
            director.playableAsset = timeline;
        }

        lerpTo = Camera.main.gameObject.GetComponent<LerpTo>();
        glitch = Camera.main.gameObject.GetComponent<GlitchHit>();

       // print(lerpTo);
        
        tmpLerpTarget = lerpTo.target;
    
    }

    float transitionStartTime;
    bool transitioning;

    bool transitionIn;

    public bool playing = false;


    public bool stealCameraInEditMode;
    Vector3 targetPos;
    Quaternion targetRot;

    Vector3 startPos;
    Quaternion startRot;

    // Update is called once per frame
    void Update()
    {
        if( Application.isEditor && Application.isPlaying != true && stealCameraInEditMode ){
            Camera.main.transform.position =  cameraTarget.position;
            Camera.main.transform.rotation =  cameraTarget.rotation;
        }
        if( playing ){

           
     

            float fSpeed = 1;
            if(God.input.x){fSpeed *= 10; }

            fSpeed *= speedMultiplier;
            director.playableGraph.GetRootPlayable(0).SetSpeed(fSpeed);

            if( God.wren != null && wrenTarget != null ){
                God.wren.transform.position  = wrenTarget.transform.position;
                God.wren.transform.rotation  = wrenTarget.transform.rotation;
            }
            
        }

        if( transitioning ){


            if( transitionIn ){
                if( transitionInSpeed == 0 ){
                    StartPlay();
                }else{

                    print("Evaluating");
                    float v = Time.time - transitionStartTime;
                    v /= transitionInSpeed;
                    if( v >= 1 ){
                        StartPlay();
                    }else{
                        print("transitioning");
                        float fV = v * v * (3 - 2 * v);

                        print( fV);
                        print(startPos);
                        print(targetPos);
                        Camera.main.transform.position = Vector3.Lerp( startPos , targetPos , fV );
                        Camera.main.transform.rotation = Quaternion.Slerp( startRot , targetRot , fV );
                    }
                }
            }else{
                 if( transitionOutSpeed == 0 ){
                    transitioning = false;
                    OnFinish();
                }else{
                    float v = Time.time - transitionStartTime;
                    v /= transitionOutSpeed;
                    if( v >= 1 ){
                        transitioning = false;
                        OnFinish();
                    }else{
                        float fV = v * v * (3 - 2 * v);
                        Camera.main.transform.position = Vector3.Lerp( startPos , targetPos , fV );
                        Camera.main.transform.rotation = Quaternion.Slerp( startRot , targetRot , fV );
                    }
                }
            }
        
        }
        
    }


    
    private void Director_Stopped( PlayableDirector d ){
        print( "Director stoppped on :  " + this.gameObject.name );
        playing = false;
        Stop();
    }


    private void Director_Played( PlayableDirector d ){

        //glitch.StartGlitch();

        //print("playStart");
        
        playing = true;

    }

    void StartPlay(){
//        print("Evaluating2");
        transitioning = false;
        director.Play();
        director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        playing = true;
        lerpTo.enabled = true;
        God.wren.canMove = false;
 //       print("lerping enabled");
    }

    public void Stop(){

        startPos = Camera.main.transform.position;
        startRot = Camera.main.transform.rotation;


        lerpTo.target = tmpLerpTarget;

      
        targetPos = lerpTo.target.position;
        targetRot = lerpTo.target.rotation;// Quaternion.LookRotation( lerpTo.lookTarget.position - lerpTo.target.position);

        lerpTo.enabled = false;
    
        
        transitionStartTime = Time.time;
        transitioning = true;
        transitionIn = false;


    }


    public void OnFinish(){

        God.instance.inCutScene = false;
        lerpTo.enabled = true;

        print( lerpTo );
        print( lerpTo.target );
        print( Camera.main );
        print( tmpLerpTarget );

        lerpTo.lerpSpeed = tmpLerpSpeed;
        lerpTo.slerpSpeed = tmpSlerpSpeed;
        lerpTo.target = tmpLerpTarget;
        Camera.main.transform.position = lerpTo.target.position;
        Camera.main.transform.LookAt( lerpTo.lookTarget );
        CutSceneFinished.Invoke();
        God.wren.canMove = true;

    }


    public void Play(){
        
        if( played && playOnce ){

        }else{

            
            God.instance.inCutScene = true;
            CutSceneStarted.Invoke();
            lerpTo.enabled = false;
          

            

            tmpSlerpSpeed = lerpTo.slerpSpeed;
            tmpLerpSpeed = lerpTo.lerpSpeed;
            tmpLerpTarget = lerpTo.target;

            lerpTo.lerpSpeed = lerpSpeed;
            lerpTo.slerpSpeed = slerpSpeed;
            lerpTo.target = cameraTarget;


            transitionStartTime = Time.time;
            transitioning = true;
            transitionIn = true;

            startPos = Camera.main.transform.position;
            startRot = Camera.main.transform.rotation;


            //evaluate to get original position
            director.time = 0;
            director.Evaluate();



            targetPos= cameraTarget.position;
            targetRot = cameraTarget.rotation;
            
            Camera.main.transform.position = startPos;
            Camera.main.transform.rotation = startRot;

            // Our bird shouldn't be flying during 
            // cut scenes!
            if( God.wren ){
                God.wren.Crash(God.wren.transform.position );
            }
        }
    }


    // Evalutates the animator at the end of the animation
    // to set all the proper values!
    public void SetEndValues(){

        startPos= Camera.main.transform.position;
        startRot = Camera.main.transform.rotation;
        
        director.time = director.playableAsset.duration;
        director.Evaluate();

        Camera.main.transform.position = startPos;
        Camera.main.transform.rotation = startRot;
    
    }

    public void SetStartValues(){


        startPos= Camera.main.transform.position;
        startRot = Camera.main.transform.rotation;
    

        director.time = 0;
        director.Evaluate();

        Camera.main.transform.position = startPos;
        Camera.main.transform.rotation = startRot;
    

    }

    public void OnEnd(){

    }
}
