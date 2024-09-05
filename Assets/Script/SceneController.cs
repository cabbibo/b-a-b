using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Crest;

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
    // int loadedScene = -1;

    public bool loadedFromPortal;
    public bool sceneLoaded = false;

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

        newScene = NS;
        oldScene = OS;

        if (newScene == oldScene)
        {
            // yield break;
        }

        UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(scenes[oldScene]);

        if (scene != null)
        {

            if (scene.isLoaded)
            {
                // unloading old scne
                var progress2 = SceneManager.UnloadSceneAsync(scene);



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


        SceneManager.LoadScene(scenes[newScene], LoadSceneMode.Additive);

        // Set the animation stuff when new scene is loaded



    }


    public void SetNewScene(int newSceneID, int oldSceneID)
    {






    }





    public UnityEvent OnSceneLoadEvent;

    void OnSceneUnloaded(UnityEngine.SceneManagement.Scene s)
    {

    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {


        GameObject[] rootObjects = scene.GetRootGameObjects();

        WrenUtils.Scene wrenScene = rootObjects[0].GetComponent<WrenUtils.Scene>();

        God.state.SetCurrentScene(newScene);


        LerpTo lt = Camera.main.GetComponent<LerpTo>();
        lt.enabled = true;


        if (wrenScene != null)
        {
            God.currentScene = wrenScene;
            wrenScene.SceneLoaded(newScene, loadedFromPortal);
        }

        print(God.state);
        print(wrenScene.portals);

        // Only animate in if we have animation!
        if (God.state.currentBiomeID >= 0 && God.state.currentBiomeID < wrenScene.portals.Length && God.wren != null)
        {
            print("starting portal animation in");
            StartCoroutine(PortalAnimationIn(wrenScene.portals[God.state.currentBiomeID]));
        }
        else
        {
            print("starting base animation in");
            StartCoroutine(BaseAnimationIn());
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

        float StartTime = Time.time;

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
        God.state.SetCurrentQuest(portal.questID);
        HardLoad(portal.sceneID);

    }

    IEnumerator PortalAnimationIn(Portal portal)
    {

        float StartTime = Time.time;

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

        while (Time.time - StartTime < fadeInLength)
        {

            float val = (Time.time - StartTime) / fadeInLength;
            //God.fade
            God.postController._Fade = (1 - val);
            God.camera.transform.position = Vector3.Lerp(endPoint, startPoint, val);///.Lerp()
            God.camera.transform.rotation = Quaternion.Slerp(endRot, startRot, val);///.Lerp()


            yield return null;
        }

        OnFadedIn();


    }


    public float fadeInLength = 3;
    IEnumerator BaseAnimationIn()
    {

        float StartTime = Time.time;



        Vector3 endPoint = God.wren.cameraWork.camTarget.position;
        Quaternion endRot = God.wren.cameraWork.camTarget.rotation;


        Vector3 startPoint = God.wren.cameraWork.camTarget.position;//portal.startPoint.position + portal.startPoint.forward * -God.wren.cameraWork.groundBackAmount + portal.startPoint.up * -God.wren.cameraWork.groundUpAmount;
        Quaternion startRot = God.wren.cameraWork.camTarget.rotation;//portal.startPoint.rotation;


        while (Time.time - StartTime < fadeInLength)
        {


            float val = (Time.time - StartTime) / fadeInLength;
            //God.fade
            God.postController._Fade = (1 - val);
            God.camera.transform.position = Vector3.Lerp(endPoint, startPoint, val);///.Lerp()
            God.camera.transform.rotation = Quaternion.Slerp(endRot, startRot, val);///.Lerp()

            yield return null;
        }



        OnFadedIn();


    }

    public void OnFadedIn()
    {

        God.wren.canMove = true;
    }











    /*

    DEMO ENDER STUFF

*/

    /*

        Ending the animation for the demo!

    */

    IEnumerator DemoAnimationOut(Portal portal)
    {


        float StartTime = Time.time;


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
