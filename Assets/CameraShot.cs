using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Video;

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
    public bool isMovieShot;
    public VideoPlayer player;
    public TimelinePlayback timelinePlayback;





    public void Set(){

  

        SetValues();
        if(isMovieShot){
            
            player.Prepare();
            //player.StepForward(); // Show first frame
            player.Pause();
        }

        God.postController.SetPostParameters(postParameters);
        this.gameObject.SetActive(true);



    }

    public void SetValues(){
        
        cameraManager.currentLocation = cameraTransform;
        cameraManager.oscillate = oscillate;
        cameraManager.oscillateSize = oscillateSize;
        cameraManager.oscillateSpeed = oscillateSpeed;
        cameraManager.lookForward = lookForward;

        sunManager.rawTimeInCycle = timeOfDay;
        sunManager.lookTarget = lightLookTarget;
        sunManager.targetPosition = lightPosition;

        if( isMovieShot ){
            print( player.isPrepared);
            player.Prepare();
          // player.time = timelinePlayback.rawTime;// videoPlayer.time = playableDirector.time; // Sync Video and Timeline
           player.frame = (long)(timelinePlayback.rawTime * 60) ; // Sync Video and Timeline
           player.StepForward(); // If the timeline is played, we will also play the video
           player.Play();
                  
        }
    }

    public void Unset(){
        this.gameObject.SetActive(false);
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
