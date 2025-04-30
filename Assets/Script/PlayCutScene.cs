using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using WrenUtils;

[ExecuteAlways]
public class PlayCutScene : MonoBehaviour
{
    public float      speedMultiplier = 1;
    public UnityEvent CutSceneFinished;
    public UnityEvent CutSceneStarted;

    public Camera main;

    public Transform cameraTarget;
    public Transform wrenTarget;
    public float     lerpSpeed  = 1;
    public float     slerpSpeed = 1;


    public TimelineAsset timeline;
    public bool          playOnce;
    public float         transitionInSpeed;
    public float         transitionOutSpeed;

    private LerpTo           lerpTo;
    private GlitchHit        glitch;
    public  PlayableDirector director;

    private float     tmpLerpSpeed;
    private float     tmpSlerpSpeed;
    private Transform tmpLerpTarget;


    /*
        0= crashed
        1= slowFlight
        2= normalFlight
    */
    public int birdType;


    public float FOV = 60;

    public Transform wrenCrashPosition;


    private bool played;


    // Start is called before the first frame update
    private void OnEnable()
    {

        playing = false;

        if ( director == null ) {
            // if( Camera.main == null ){ Camera.main = Camera.Camera.main; }
            director = GetComponent<PlayableDirector>();

        }

        director.played += Director_Played;
        director.stopped += Director_Stopped;

        if ( timeline != null ) {
            director.playableAsset = timeline;
        }

    }

    private void OnDisable()
    {
        director.played -= Director_Played;
        director.stopped -= Director_Stopped;
    }

    private bool transitioning;
    public  bool playing = false;


    public bool stealCameraInEditMode;

    public void OnDrawGizmosSelected()
    {

        if ( Application.isEditor && Application.isPlaying != true && stealCameraInEditMode ) {

            //print( this );
            //print( cameraTarget.position );
            Camera.main.transform.position = cameraTarget.position;
            Camera.main.transform.rotation = cameraTarget.rotation;
        }

    }

    // Update is called once per frame
    private void Update()
    {


        if ( playing ) {

//            print( "playing cutscene" );
            float fSpeed = 1;

            if ( God.input.x ) {
                AudioListener.volume = .1f;
                fSpeed *= 10;
            } else {
                AudioListener.volume = 1;
            }

            fSpeed *= speedMultiplier;

            director.playableGraph.GetRootPlayable( 0 ).SetSpeed( fSpeed );

            if ( God.wren != null && wrenTarget != null ) {
                God.wren.canMove = false;

                if ( wrenTarget != null ) {
                    God.wren.transform.position = wrenTarget.transform.position;
                    God.wren.transform.rotation = wrenTarget.transform.rotation;
                }
            }

        }


        // Move the wren to the correct position?
        if ( transitioning ) {

            print( "transitioning" );

            if ( God.wren != null ) {
                God.wren.canMove = false;

                if ( wrenCrashPosition != null ) {
                    God.wren.transform.position = wrenCrashPosition.position;
                    God.wren.transform.rotation = wrenCrashPosition.rotation;
                }

            }

        }

    }


    public void OnTransitionInComplete()
    {
        // Lets go we made it in!
        StartPlay();

    }

    public void OnTransitionOutComplete()
    {
        OnFinish();
    }

    private void Director_Stopped( PlayableDirector d )
    {
//        print( "stopped" );
        playing = false;
        AudioListener.volume = 1;
        Stop();
    }


    private void Director_Played( PlayableDirector d )
    {
//        print( "played" );
        AudioListener.volume = 1;
        playing = true;

    }

    private void StartPlay()
    {

//        print( "start play cut scene" );
        transitioning = false;
        director.Play();
        director.playableGraph.GetRootPlayable( 0 ).SetSpeed( 1 );
        playing = true;

        God.wren.canMove = false;
        AudioListener.volume = 1;

    }

    public void Stop()
    {
//        print( "Cut scene stopped pplaying" );
        AudioListener.volume = 1;
        God.cameraManager.cutSceneManager.OnCutSceneFinishedPlaying();


    }


    public void OnFinish()
    {

//        print( "finished " );
        God.instance.inCutScene = false;

        CutSceneFinished.Invoke();

        if ( God.wren != null ) {
            God.wren.canMove = true;
        }

        AudioListener.volume = 1;

    }


    public void Play()
    {

//        print( "playing cutscene" );
        AudioListener.volume = 1;

        if ( played && playOnce ) {
            print( "already played" );
        } else {

            lerpTo = God.cameraManager.lerpManager;

            God.instance.inCutScene = true;
            CutSceneStarted.Invoke();


            //evaluate to get original position
            director.time = 0;
            director.Evaluate();

            transitioning = true;

            // Our bird shouldn't be flying during 
            // cut scenes!
            if ( God.wren ) {
                if ( wrenCrashPosition != null ) {
                    God.wren.Crash( wrenCrashPosition.position );
                } else {
                    God.wren.Crash( God.wren.transform.position );
                }
            }

            God.cameraManager.cutSceneManager.SetCutScene( this );
        }
    }


    // Evalutates the animator at the end of the animation
    // to set all the proper values!
    public void SetEndValues()
    {

        print( "setting end values" );
        director.time = director.playableAsset.duration;
        director.Evaluate();


    }

    public void SetStartValues()
    {

        print( "setting start values" );
        director.time = 0;
        director.Evaluate();

    }

    public void OnEnd()
    {

        print( "end called" );

    }
}