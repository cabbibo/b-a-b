using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WrenUtils;

public class LoadingPlatform : MonoBehaviour
{




    public bool sceneIsLoaded;
    public string sceneName;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnCollisionEnter( Collision c ){

        print( "colliding");
        print(c.collider.attachedRigidbody);
        
        if( c.collider.attachedRigidbody == God.wren.physics.rb ){
            print("GOD WREN HIT");

            if( sceneIsLoaded == false ){
                //StartCoroutine(LoadAsyncScene());
            }

        }

    }


     IEnumerator LoadAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        print("LoadingScene");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        print("LoadedScene ");
    }

}
