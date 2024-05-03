using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

using WrenUtils;


/*

    Chooses start scene to load from state!

*/
public class SceneController : MonoBehaviour
{

    public string[] scenes;
    //public int God.state.currentSceneID = -1;
    public int oldScene = 0;
    public int newScene = 0;
    int loadedScene = -1;

    public bool sceneLoaded = false;
    public bool useStartScene = false;

    public bool autoLoad = true;

    public MenuController menuController;





    // STEP ONE UNLOAD EVERYTHING
    public void OnEnable()
    {
        // Add our listeners
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;


        // Unloading all the scenes!
        for (int i = 0; i < scenes.Length; i++)
        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(scenes[i]);

            if (scene != null)
            {
                if (scene.name != "BaseScene" && scene.name != null)
                {
                    SceneManager.UnloadScene(scene);
                }
            }
        }
    }



    public void OnDisable()
    {
        // Remove our listeners
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }





    public void OnRoomConnection()
    {

        loadedFromPortal = false;

        print("1) ROOM CONNECTION ");

        if (autoLoad)
        {
            HardLoad(God.state.currentSceneID);
        }
        else
        {
            menuController.gameObject.SetActive(true);
        }


    }



    public void NewGame()
    {
        God.state.ResetAll();
        HardStart();
    }

    public void ResetSave()
    {

        God.state.ResetAll();

    }


    public void HardStart()
    {

        God.state.OnGameStart();
        HardLoad(God.state.currentSceneID);

    }











    public UnityEngine.SceneManagement.Scene currentMainScene;
    public bool loadedFromPortal;

    public void LoadSceneFromPortal(Portal portal)
    {

        // make it so we dont hurt ourselves
        God.wren.inEther = true;
        God.wren.Crash(portal.collisionPoint.position);

        God.wren.canMove = false;
        Camera.main.gameObject.GetComponent<LerpTo>().enabled = false;

        loadedFromPortal = true;
        // Lerps out of scene via a portal
        StartCoroutine(PortalAnimationOut(portal));


    }






    public void HardLoad(int id)
    {
        print("2) HARD LOAD!");

        sceneLoaded = true;
        StartCoroutine(SceneSwitch(id, God.state.currentSceneID));

    }

    public void Death()
    {
        God.state.SetCurrentBiome(-1);
        HardLoad(God.state.currentSceneID);
    }


    //TODO: HACKY AF
    IEnumerator SceneSwitch(int NS, int OS)
    {

        print("3) SCENE SWITCH");


        newScene = NS;
        oldScene = OS;

        //        print(newScene + " " + oldScene);
        if (newScene == oldScene)
        {
            //          print("same scene");
            // yield break;
        }

        UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(scenes[oldScene]);

        //        print(scene);
        if (scene != null)
        {

            if (scene.isLoaded)
            {
                print("3.5) UNLOAD SCENE");
                // unloading old scne
                var progress2 = SceneManager.UnloadSceneAsync(scene);

                //            print(progress2);

                if (progress2 != null)
                {
                    while (!progress2.isDone)
                    {

                        // Check each frame if the scene has completed.
                        // For more information about yield in C# see: https://youtu.be/bsZjfuTrPSA
                        yield return null;
                    }
                }

            }

        }


        print("4) LOAD NEW SCENE");

        SceneManager.LoadScene(scenes[newScene], LoadSceneMode.Additive);

        // Set the animation stuff when new scene is loaded



    }


    public void SetNewScene(int newSceneID, int oldSceneID)
    {

        print("6) SET NEW SCENE");

        oldScene = God.state.currentSceneID;

        // figure out if we are loadign from a portal or not
        God.state.SetCurrentScene(newSceneID);

        loadedScene = God.state.currentSceneID;

        currentMainScene = SceneManager.GetSceneByName(scenes[newSceneID]);

        // Animation In plays when we load the new scene



    }





    public UnityEvent OnSceneLoadEvent;

    void OnSceneUnloaded(UnityEngine.SceneManagement.Scene s)
    {

    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {

        print("5) On SCENE LOADED");

        GameObject[] rootObjects = scene.GetRootGameObjects();

        WrenUtils.Scene wrenScene = rootObjects[0].GetComponent<WrenUtils.Scene>();

        SetNewScene(newScene, oldScene);

        wrenScene.SceneLoaded(newScene, loadedFromPortal);

        //print( biome );
        // Only animate in if we have animation!
        if (God.state.currentBiomeID >= 0 && God.state.currentBiomeID < wrenScene.portals.Length && God.wren != null)
        {
            //            print("ANIMATION IN!");
            StartCoroutine(PortalAnimationIn(wrenScene.portals[God.state.currentBiomeID]));
        }
        else
        {

        }

        OnSceneLoadEvent.Invoke();
    }

    void OnSceneLoaded()
    {
        OnSceneLoadEvent.Invoke();
    }

    bool isScene_CurrentlyLoaded(string sceneName_no_extention, out UnityEngine.SceneManagement.Scene sceneFound)
    {

        sceneFound = SceneManager.GetSceneAt(0);
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneAt(i);

            if (scene.name == sceneName_no_extention)
            {
                //the scene is already loaded
                sceneFound = scene;
                return true;
            }
        }

        return false;//scene not currently loaded in the hierarchy
    }
















    /*



        ANIMATIONS IN AND OUT


    */









    /*

    Animating out of the portal

    */

    public float portalAnimationOutLength = 3;
    IEnumerator PortalAnimationOut(Portal portal)
    {
        print("portal Animation out");

        float StartTime = Time.time;
        float EndTime = Time.time + portalAnimationOutLength;


        Vector3 startPoint = God.camera.transform.position;
        Vector3 endPoint = portal.collisionPoint.position;


        Quaternion startRot = God.camera.transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(portal.collisionPoint.forward, Vector3.up);
        while (Time.time - StartTime < portalAnimationOutLength)
        {

            float val = (Time.time - StartTime) / portalAnimationOutLength;
            //God.fade
            God.postController._Fade = val * val;
            God.camera.transform.position = Vector3.Lerp(startPoint, endPoint, val);///.Lerp()
            God.camera.transform.rotation = Quaternion.Slerp(startRot, endRot, val);///.Lerp()

            yield return null;
        }

        God.state.SetCurrentBiome(portal.biome);
        HardLoad(portal.sceneID);

    }

    public float portalAnimationInLength = 3;
    IEnumerator PortalAnimationIn(Portal portal)
    {

        print("8) PORTAL ANIMATION IN");



        //        print("portal animation in!!!");

        float StartTime = Time.time;
        float EndTime = Time.time + portalAnimationInLength;

        float v1 = Vector3.Distance(portal.collisionPointFront.position, portal.startPoint.position);
        float v2 = Vector3.Distance(portal.collisionPointBack.position, portal.startPoint.position);

        portal.collisionPoint = portal.collisionPointFront;

        if (v2 < v1)
        {
            portal.collisionPoint = portal.collisionPointBack;
        }


        Vector3 endPoint = portal.collisionPoint.position;
        Quaternion endRot = Quaternion.LookRotation(portal.collisionPoint.forward, Vector3.up);


        Vector3 startPoint = God.wren.cameraWork.camTarget.position;//portal.startPoint.position + portal.startPoint.forward * -God.wren.cameraWork.groundBackAmount + portal.startPoint.up * -God.wren.cameraWork.groundUpAmount;
        Quaternion startRot = God.wren.cameraWork.camTarget.rotation;//portal.startPoint.rotation;

        God.wren.FullReset();
        while (Time.time - StartTime < portalAnimationInLength)
        {

            float val = (Time.time - StartTime) / portalAnimationInLength;
            //God.fade
            God.postController._Fade = (1 - val);
            God.camera.transform.position = Vector3.Lerp(endPoint, startPoint, val);///.Lerp()
            God.camera.transform.rotation = Quaternion.Slerp(endRot, startRot, val);///.Lerp()
  //         print(val);


            yield return null;
        }

        God.wren.FullReset();



    }











    /*

    DEMO ENDER STUFF

*/

    /*

        Ending the animation for the demo!

    */

    IEnumerator DemoAnimationOut(Portal portal)
    {

        print("demo animation out");

        float StartTime = Time.time;
        float EndTime = Time.time + portalAnimationOutLength;


        Vector3 startPoint = God.camera.transform.position;
        Vector3 endPoint = portal.collisionPoint.position;


        Quaternion startRot = God.camera.transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(portal.collisionPoint.forward, Vector3.up);
        while (Time.time - StartTime < portalAnimationOutLength)
        {

            float val = (Time.time - StartTime) / portalAnimationOutLength;
            //God.fade
            //  God.postController._Fade = val * val;
            God.camera.transform.position = Vector3.Lerp(startPoint, endPoint, val);///.Lerp()
            God.camera.transform.rotation = Quaternion.Slerp(startRot, endRot, val);///.Lerp()

            yield return null;
        }

        God.state.SetCurrentBiome(portal.biome);
        OnDemoEnd();

    }


    public void EndDemo(Portal portal)
    {


        print("end demo");
        // make it so we dont hurt ourselves
        God.wren.inEther = true;
        God.wren.Crash(portal.collisionPoint.position);

        God.wren.canMove = false;
        Camera.main.gameObject.GetComponent<LerpTo>().enabled = false;

        StartCoroutine(DemoAnimationOut(portal));

    }


    public GameObject demoEnder;
    public void OnDemoEnd()
    {
        demoEnder.SetActive(true);
    }



}
