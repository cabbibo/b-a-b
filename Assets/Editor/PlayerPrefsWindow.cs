using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerPrefsWindow : EditorWindow
{


    public GUISkin skin;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Player Prefs Window")]
    static void Init()
    {

      // skin = (GUISkin)Resources.Load("PlayerPrefsSkin");

        Debug.Log("Hiii");
        // Get existing open window or if none, make a new one:
        PlayerPrefsWindow window = (PlayerPrefsWindow)EditorWindow.GetWindow(typeof(PlayerPrefsWindow));
        window.Show();

        Debug.Log( GameObject.Find("God") );
       //GameObject g = GameObject.Find("God");

       //god = g.GetComponent<God>();
       //data = g.GetComponent<Data>();
       //events = data.inputEvents;
       //state = data.state;

       //skin = (GUISkin)Resources.Load("GlobalEditSkin");
       
       // Debug.Log(skin);


       

    }

     public void Update()
 {
     // This is necessary to make the framerate normal for the editor window.
     Repaint();
 }



         // Window has been selected
 void OnFocus() {
     // Remove delegate listener if it has previously
     // been assigned.
     SceneView.duringSceneGui -= this.OnSceneGUI;
     // Add (or re-add) the delegate.
     SceneView.duringSceneGui += this.OnSceneGUI;
 }
 
 void OnDestroy() {
     // When the window is destroyed, remove the delegate
     // so that it will no longer do any drawing.
     SceneView.duringSceneGui -= this.OnSceneGUI;
 }


     void OnSceneGUI(SceneView sceneView){

         
       //skin = (GUISkin)Resources.Load("PlayerPrefsSkin");
      /*Assign();
      
        if(god != null ){

        if( lockToGameCamera ) LockCamera(sceneView);
        if( doInputEvents ){

          HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
          DoInputEvents(sceneView);
        }
      }*/
        
    }


    public bool tutorialComplete;   
    public int currentLevel;
    public bool gameStarted;
    void OnGUI()
    {

   
        
        EditorGUILayout.Space();
        GUILayout.Label("Tutorial Complete");
        tutorialComplete = EditorGUILayout.Toggle(tutorialComplete,GUILayout.Width(30));
        
        EditorGUILayout.Space();
        GUILayout.Label("Game Started");
        gameStarted = EditorGUILayout.Toggle(gameStarted,GUILayout.Width(30));

        
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Current Level");
        currentLevel = EditorGUILayout.IntField(currentLevel);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
       // GUILayout.Toggle(false);
        if(GUILayout.Button("ResetPreferences")){

            PlayerPrefs.SetInt("_CurrentScene",currentLevel);
            
            int tComplete = tutorialComplete?1:0;
            PlayerPrefs.SetInt("_TutorialComplete",tComplete);
            
            int gStarted = gameStarted?1:0;
            PlayerPrefs.SetInt("_GameStarted",gStarted);

        }
 
    }
}
