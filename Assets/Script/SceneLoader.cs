using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Normal.Realtime;

public class SceneLoader : MonoBehaviour
{

    public string[] scenesToLoadOnConnection;
     public Realtime _realtime;

     public bool connected = false;



    public void Awake(){
          // Get the Realtime component on this game object
            _realtime = GetComponent<Realtime>();

            // Notify us when Realtime successfully connects to the room
            _realtime.didConnectToRoom += LoadOnConnection;
    }
    public void LoadOnConnection(Realtime realtime){
        if( !connected ){
            connected= true;

            for( int i = 0; i < scenesToLoadOnConnection.Length; i++ ){
                StartCoroutine( LoadScene( scenesToLoadOnConnection[i] ));
            }
        }
    }


    IEnumerator LoadScene( string sceneName )
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
