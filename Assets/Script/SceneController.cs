using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneController : MonoBehaviour
{

    public bool loadOnStart;
    public string[] scenes;
    public int currentSceneID = -1;
    public int oldScene = -1;

    public bool sceneLoaded = false;

    public bool useStartScene = false;
    public bool dontLoadOnStart = false;

    int loadedScene = -1;

    public int biome = 0; 
    
    // mountain
    // temple
    // desert
    // forest
    // lighthouse
    // cave
    // city


    public UnityEngine.SceneManagement.Scene currentMainScene;

    public void LoadScene(int id ){        

        HardLoad(id);
            
    }

    public void LoadSceneFromPortal(Portal portal ){

     
            // make it so we dont hurt ourselves
            God.wren.inEther = true;
            God.wren.Crash(portal.collisionPoint.position);

            God.wren.canMove = false;
            Camera.main.gameObject.GetComponent<LerpTo>().enabled = false;

            StartCoroutine(PortalAnimationOut(portal));


    }




    public float portalAnimationOutLength = 3;
    IEnumerator PortalAnimationOut( Portal portal )
    {

        float StartTime = Time.time;
        float EndTime = Time.time + portalAnimationOutLength;


        Vector3 startPoint  = God.camera.transform.position;
        Vector3 endPoint  = portal.collisionPoint.position;


        Quaternion startRot = God.camera.transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(portal.collisionPoint.forward,Vector3.up);
        while( Time.time - StartTime < portalAnimationOutLength ){
        
            float val = (Time.time - StartTime ) / portalAnimationOutLength;
            //God.fade
            God.postController._Fade = val * val;
            God.camera.transform.position = Vector3.Lerp(startPoint, endPoint  , val);///.Lerp()
            God.camera.transform.rotation = Quaternion.Slerp(startRot,endRot ,val);///.Lerp()

            yield return null;
        }

        biome = portal.biome;
        LoadScene(portal.sceneID);

    }

    public void HardLoad(int id){

        
        sceneLoaded = true;     


        if( dontLoadOnStart == false ){
            StartCoroutine(SceneSwitch( id, currentSceneID));
        }else{
            OnSceneLoaded();
            SetNewScene(id, currentSceneID);
        }

       
    }

    public void Death(){
        biome = -1;
        HardLoad(0);
    }


    //TODO: HACKY AF
    IEnumerator SceneSwitch(int newScene, int oldScene)
    {


        SceneManager.LoadScene(scenes[newScene], LoadSceneMode.Additive);
            

        // unloading old scne
        var progress2= SceneManager.UnloadSceneAsync(scenes[oldScene]);
        while (!progress2.isDone)
        {

            // Check each frame if the scene has completed.
            // For more information about yield in C# see: https://youtu.be/bsZjfuTrPSA
            yield return null;
        } 

       Scene scene =  SceneManager.GetSceneByName(scenes[oldScene]);
            

        // unloading old scene AGAIN for some reason this actually works?
    
        if( scene.isLoaded  && oldScene != newScene ){
            var progress3=  SceneManager.UnloadSceneAsync(scene);    
            while (!progress3.isDone)
            {

                // Check each frame if the scene has completed.
                // For more information about yield in C# see: https://youtu.be/bsZjfuTrPSA
                yield return null;
            } 


        }



        SetNewScene(newScene, oldScene);

            
    }


    public void SetNewScene(int newSceneID , int oldSceneID){

            Camera.main.gameObject.GetComponent<LerpTo>().enabled = true;
            God.wren.state.inInterface = false;
            God.wren.airInterface.Toggle( false ); 
            God.wren.fullInterface.Toggle( false ); 

            if( newSceneID == 0 ){
                God.wren.inEther = true;
            }else{
                God.wren.inEther = false;
            }

            God.wren.canMove = true;
            
            PlayerPrefs.SetInt("_CurrentScene",newSceneID);
            PlayerPrefs.SetInt("_CurrentBiome",biome);
            oldScene = currentSceneID;
            currentSceneID = newSceneID;
            loadedScene = currentSceneID;

            currentMainScene = SceneManager.GetSceneByName(scenes[newSceneID]);

          //  StartCoroutine(PortalAnimationIn(currentMainScene.portals[biome]));


    }


public void OnSceneFinishedLoading( WrenUtils.Scene wrenScene ){

    StartCoroutine( PortalAnimationIn( wrenScene.portals[biome]));

}

  
    public float portalAnimationInLength = 3;
    IEnumerator PortalAnimationIn( Portal portal )
    {

        float StartTime = Time.time;
        float EndTime = Time.time + portalAnimationInLength;

        float v1 = Vector3.Distance( portal.collisionPointFront.position , portal.startPoint.position );
        float v2 = Vector3.Distance( portal.collisionPointBack.position , portal.startPoint.position );

        portal.collisionPoint =  portal.collisionPointFront;

        if( v2 < v1 ){
            portal.collisionPoint = portal.collisionPointBack;
        }




        Vector3 endPoint  = portal.collisionPoint.position;
        Quaternion endRot = Quaternion.LookRotation(portal.collisionPoint.forward,Vector3.up);


        Vector3 startPoint  = God.wren.cameraWork.camTarget.position;//portal.startPoint.position + portal.startPoint.forward * -God.wren.cameraWork.groundBackAmount + portal.startPoint.up * -God.wren.cameraWork.groundUpAmount;
        Quaternion startRot =  God.wren.cameraWork.camTarget.rotation;//portal.startPoint.rotation;

        while( Time.time - StartTime < portalAnimationInLength ){
        
            float val = (Time.time - StartTime ) / portalAnimationInLength;
            //God.fade
            God.postController._Fade = (1-val );
            God.camera.transform.position = Vector3.Lerp(endPoint, startPoint  , val);///.Lerp()
            God.camera.transform.rotation = Quaternion.Slerp(endRot,startRot ,val);///.Lerp()


            yield return null;
        }


    }




    public void NewGame(){
        PlayerPrefs.DeleteAll();
        OnStart();
    }

    public void ResetSave(){
        PlayerPrefs.DeleteAll();
        currentSceneID = PlayerPrefs.GetInt("_CurrentScene",0);
        biome = PlayerPrefs.GetInt("_CurrentBiome",-1);

        int gameStarted = PlayerPrefs.GetInt("_GameStarted",0);
        if( gameStarted == 0){
            gameStarted = 1;
        }
        
    }
        

    public void HardStart(){

        if( useStartScene == false ){
            currentSceneID = PlayerPrefs.GetInt("_CurrentScene",0);
        }
    
        biome = PlayerPrefs.GetInt("_CurrentBiome",-1);

        int gameStarted = PlayerPrefs.GetInt("_GameStarted",0);
        if( gameStarted == 0){
            gameStarted = 1;
        }

        PlayerPrefs.SetInt("_GameStarted",1);
        HardLoad(currentSceneID);

    }
    
    public void OnStart(){

            currentSceneID = PlayerPrefs.GetInt("_CurrentScene",0);
            biome = PlayerPrefs.GetInt("_CurrentBiome",-1);

            int gameStarted = PlayerPrefs.GetInt("_GameStarted",0);
            if( gameStarted == 0){
                gameStarted = 1;
            }

            PlayerPrefs.SetInt("_GameStarted",1);
            LoadScene(currentSceneID); 
        
    }


    public void OnEnable(){
      //  OnStart();
       SceneManager.sceneLoaded += OnSceneLoaded;
       SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void OnDisable(){
       //OnStart();
       SceneManager.sceneLoaded -= OnSceneLoaded;
       SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }



    public UnityEvent OnSceneLoadEvent;

    void OnSceneUnloaded( UnityEngine.SceneManagement.Scene  s  ){

    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene  scene, LoadSceneMode mode)
    {
        OnSceneLoadEvent.Invoke();
    }

       void OnSceneLoaded()
    {
        OnSceneLoadEvent.Invoke();
    }
 
    bool isScene_CurrentlyLoaded(string sceneName_no_extention, out UnityEngine.SceneManagement.Scene sceneFound ) {
        
        sceneFound = SceneManager.GetSceneAt(0);
        for(int i = 0; i<SceneManager.sceneCount; ++i) {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneAt(i);

            if(scene.name == sceneName_no_extention) {
                //the scene is already loaded
                sceneFound = scene;
                return true;
            }
        }

        return false;//scene not currently loaded in the hierarchy
    }




}
