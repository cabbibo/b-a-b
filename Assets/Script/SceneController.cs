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

        HardLoad(id);//
       // God.fade.FadeOut(Color.white, 2 , () => {HardLoad(id); return 0;});
    }

    public void HardLoad(int id){


        
        sceneLoaded = true;     





        if( dontLoadOnStart == false ){
            // bool needsNewLoad
            // Only unload a scene if we have one to unload!
           /* if( currentMainScene.name != null ){

                //if( currentSceneID != id ){
                    print("UNLOADING SCENE");
                    print(scenes[currentSceneID]);
                    print(currentMainScene);
                    print( currentMainScene.name);

            
                    currentMainScene = SceneManager.GetSceneByName(scenes[currentSceneID]);
                    SceneManager.UnloadScene(currentMainScene);
            /// }

            }else{
            print("NO SCEEN UNLOADING");
            }

            

            SceneManager.LoadScene(scenes[id],LoadSceneMode.Additive);*/

            StartCoroutine(SceneSwitch( id, currentSceneID));

        }else{
            OnSceneLoaded();
        }


        PlayerPrefs.SetInt("_CurrentScene",id);
        PlayerPrefs.SetInt("_CurrentBiome",biome);
        oldScene = currentSceneID;
        currentSceneID = id;
        loadedScene = currentSceneID;

        currentMainScene = SceneManager.GetSceneByName(scenes[id]);
 

    
    
        //God.fade.FadeIn(2);

       
    }

    public void Death(){
        biome = -1;
        HardLoad(0);
    }



    IEnumerator SceneSwitch(int newScene, int oldScene)
    {
        print("hi");
        print( "SCENE COUNT : " + SceneManager.sceneCount);
      //  print( "Loaded COUNT : " + SceneManager.loadedSceneCount);
        print( "Hmm " + newScene + "||" + oldScene);

       // AsyncOperation load = SceneManager.LoadSceneAsync(scenes[newScene], LoadSceneMode.Additive);
        //var sceneStatus = SceneManager.LoadSceneAsync("SceneName", LoadSceneMode.Additive);
      //  load.completed += (e) =˃ Debug.Log("Scene Loaded");
        /*
        var progress = SceneManager.LoadSceneAsync(scenes[newScene], LoadSceneMode.Additive);

            while (!progress.isDone)
            {

                print("HIIII");
                // Check each frame if the scene has completed.
                // For more information about yield in C# see: https://youtu.be/bsZjfuTrPSA
                yield return null;
            }
*/


        SceneManager.LoadScene(scenes[newScene], LoadSceneMode.Additive);
            
        //SceneManager.LoadScene(scenes[newScene], LoadSceneMode.Additive);
        //yield return null;
        print("hi2");
        print( "Hmm2  " + newScene + "||" + oldScene);
        print( "SCENE COUNT : " + SceneManager.sceneCount);
        print( scenes[oldScene]);
   //     print( "Loaded COUNT : " + SceneManager.loadedSceneCount);
         var progress2= SceneManager.UnloadSceneAsync(scenes[oldScene]);
        // unload.completed += (e) =˃ Debug.Log("Scene undnsn Loaded");


             while (!progress2.isDone)
            {

                print("HIIII2");
                // Check each frame if the scene has completed.
                // For more information about yield in C# see: https://youtu.be/bsZjfuTrPSA
                yield return null;
            } 

        print("OLD : " + scenes[oldScene]);
       Scene scene =  SceneManager.GetSceneByName(scenes[oldScene]);
       print( scene.isLoaded );
       print(scene.path);
       print( scene.name);
            print("NOW ITS DEAD");

            

        if( scene.isLoaded  && oldScene != newScene ){
            var progress3=  SceneManager.UnloadSceneAsync(scene);    
            while (!progress3.isDone)
            {

                print("HIIII2");
                // Check each frame if the scene has completed.
                // For more information about yield in C# see: https://youtu.be/bsZjfuTrPSA
                yield return null;
            } 


        }



        SetNewScene();

            
    }


    public void SetNewScene(){

            Camera.main.gameObject.GetComponent<LerpTo>().enabled = true;
            God.wren.state.inInterface = false;
           // God.wren.ToggleInterface(false);
            God.wren.airInterface.Toggle( false ); 
            God.wren.fullInterface.Toggle( false ); 
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

        print( "onStart");
            currentSceneID = PlayerPrefs.GetInt("_CurrentScene",0);
            biome = PlayerPrefs.GetInt("_CurrentBiome",-1);

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
        //        print("DEFAULT MAIN SCENE");
        //        print( currentMainScene);
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
        print("SCENE : " + s.name + " UNLOADED " );
        print("hi3");
      //  print( "Hmm3  " + newScene + "||" + oldScene);
        print( "SCENE COUNT : " + SceneManager.sceneCount);
      //  print( "Loaded COUNT : " + SceneManager.loadedSceneCount);

       // OnSceneLoadEvent.Invoke();
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
