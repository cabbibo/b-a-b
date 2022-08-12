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

    public UnityEngine.SceneManagement.Scene currentMainScene;

    public void LoadScene(int id ){        
        print(";poading");
        print(id);
        God.fade.FadeOut(Color.white, 2 , () => {HardLoad(id); return 0;});
    }

    public void HardLoad(int id){


        
        sceneLoaded = true;     


        print("HARD LOADING");

        print( SceneManager.sceneCount);
        print(SceneManager.GetActiveScene().name);


        if( dontLoadOnStart == false ){
       // bool needsNewLoad
        // Only unload a scene if we have one to unload!
        if( currentMainScene.name != null ){

            //if( currentSceneID != id ){
                print("UNLOADING SCENE");
                print(scenes[currentSceneID]);
                print(currentMainScene);
                print( currentMainScene.name);

        
                //currentMainScene = SceneManager.GetSceneByName(scenes[currentSceneID]);
                SceneManager.UnloadScene(currentMainScene);
           /// }

        }else{
          print("NO SCEEN UNLOADING");
        }


        SceneManager.LoadScene(scenes[id],LoadSceneMode.Additive);

        }else{
            OnSceneLoaded();
        }


        PlayerPrefs.SetInt("_CurrentScene",id);
        oldScene = currentSceneID;
        currentSceneID = id;
        loadedScene = currentSceneID;

        currentMainScene = SceneManager.GetSceneByName(scenes[id]);
        print(scenes[currentSceneID]);
        print(currentMainScene);


    
    
        //God.fade.FadeIn(2);

       
    }


    public void NewGame(){
        PlayerPrefs.DeleteAll();
        OnStart();
    }

    public void ResetSave(){
        PlayerPrefs.DeleteAll();
        currentSceneID = PlayerPrefs.GetInt("_CurrentScene",0);
    
        int gameStarted = PlayerPrefs.GetInt("_GameStarted",0);
        if( gameStarted == 0){
            gameStarted = 1;
        }
        
    }
        

    public void HardStart(){

        print("hard STart");
        if( useStartScene == false ){
            currentSceneID = PlayerPrefs.GetInt("_CurrentScene",0);
        }

        int gameStarted = PlayerPrefs.GetInt("_GameStarted",0);
        if( gameStarted == 0){
            gameStarted = 1;
        }

        PlayerPrefs.SetInt("_GameStarted",1);
        HardLoad(currentSceneID);

    }
    
    public void OnStart(){

        print( "onStart");
            currentSceneID = PlayerPrefs.GetInt("_CurrentScene",0);
         

            int gameStarted = PlayerPrefs.GetInt("_GameStarted",0);
            if( gameStarted == 0){
                gameStarted = 1;
            }

            PlayerPrefs.SetInt("_GameStarted",1);


            LoadScene(currentSceneID);
 
 
 /*

      UnityEngine.SceneManagement.Scene sceneFound;
            bool alreadyLoaded = isScene_CurrentlyLoaded(scenes[currentSceneID],out sceneFound);

        print("LOADIO");
        print(alreadyLoaded);
    
        if( alreadyLoaded == false ){
            LoadScene(currentSceneID);
        }else{

            Scene scene = sceneFound.GetRootGameObjects()[0].GetComponent<Scene>();
            print("its me scene");
            print(scene);
            scene.SceneLoaded();
        }*/
        
    }


    public void OnEnable(){
        print("DEFAULT MAIN SCENE");
        print( currentMainScene);
       // OnStart();
        SceneManager.sceneLoaded += OnSceneLoaded;
       SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

        public void OnDisable(){
       // OnStart();

       SceneManager.sceneLoaded -= OnSceneLoaded;
       SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }



    public UnityEvent OnSceneLoadEvent;

    void OnSceneUnloaded( UnityEngine.SceneManagement.Scene  s  ){
        print("SCENE : " + s.name + " UNLOADED " );

        OnSceneLoadEvent.Invoke();
    }

   void OnSceneLoaded(UnityEngine.SceneManagement.Scene  scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        
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
