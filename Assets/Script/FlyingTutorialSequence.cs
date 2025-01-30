using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Remoting;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

#if UNITY_EDITOR

using UnityEditor;




#endif

public class FlyingTutorialSequence : MonoBehaviour
{

    public bool speedRun;

    public float timeBetweenStates = 1f;

    public float waitTimeInFirstShots = .1f;
    public float waitTimeInFlightSpace = .1f;
    public float cameraLerpTime = 1f;

    public float minWaitTimeForXPress;

    public TutorialStateManager stateManager;

    public static UnityAction OnTutorialStart;
    public static UnityAction OnTutorialDiveFinished;
    public static UnityAction OnFreeFlightStarted;


    //  public CinematicCameraHandler cinematicCamera;

    public CanvasGroup groupContainer;
    public CanvasGroup xToContinue;

    public TextMeshProUGUI controllerText;

    public RectTransform progressBar;

    public bool debug;
    public int debugCamIdx = 0;

    public Gradient fadeGradient;
    public Renderer fade;

    public TutorialEnder ender;

    [Header("Start")]
    public GameObject activeInTutorial;
    public GameObject activeAfterTutorial;
    public GameObject cloudParticles;

    [Header("Controller")]
    public GameObject groupSticks;
    public GameObject groupDive;
    public GameObject groupLeft;
    public GameObject groupRight;
    public GameObject groupUp;
    public GameObject groupDown;
    public GameObject groupHold;
    public GameObject groupFlap;
    public GameObject groupSwoop;
    public GameObject groupRelease;
    public GameObject groupRelease2;

    public GameObject groupGentle;
    public GameObject groupBoost;
    public GameObject groupPing;


    [Header("Cameras")]
    public CinematicCamera cameraInsideCloseup;
    public CinematicCamera cameraCloseBack;
    public CinematicCamera cameraFarBack;


    enum ControllerHint { None, Dive, Left, Right, Forward, Back, Hold, Takeoff, Flap, Swoop, Release, Release2, Gentle, Boost, Ping }

    [Header("Tooltip Cards")]
    public CanvasGroup groupCard;
    public CanvasGroup groupXToContinue;
    public TextMeshProUGUI cardTitle;
    public TextMeshProUGUI cardText;
    public GameObject cardFlyOnGround;

    [Header("Ending")]
    public CanvasGroup groupEnd;


    Coroutine tutSequence;
    float _lastSequenceTime;


    public WeatherManager weatherManager;
    public TargetManager targetManager;
    public Material featherStartMaterial;
    public Material featherMainMaterial;




    public int shardsPerTargetHit = 30;

    public GameObject hitTarget;
    public float hitTargetRadius = 20;
    public float moveTowardsTargetForwardMultiplier = 100;



    public float amountProgressPerHit = .3f;



    static FlyingTutorialSequence _instance;
    public static FlyingTutorialSequence Instance
    {
        get
        {
            if (!_instance)
                _instance = FindObjectOfType<FlyingTutorialSequence>();
            return _instance;
        }
    }





    public PostParameters preSplosionPostParameters;
    public PostParameters postSplosionPostParameters;


    void Update()
    {

        fade.transform.position = God.camera.transform.position;
        CheckCheats();

    }

















    /**
          _____ _   _ _____                              
         |_   _| | | | ____|                             
           | | | |_| |  _|                               
           | | |  _  | |___                              
           |_| |_|_|_|_____| _   _   _    _              
            / \  / ___|_   _| | | | / \  | |             
           / _ \| |     | | | | | |/ _ \ | |             
          / ___ \ |___  | | | |_| / ___ \| |___          
         /_/__ \_\____|_|_|_ \___/_/__ \_\_____|__ _____ 
         / ___|| ____/ _ \| | | | ____| \ | |/ ___| ____|
         \___ \|  _|| | | | | | |  _| |  \| | |   |  _|  
          ___) | |__| |_| | |_| | |___| |\  | |___| |___ 
         |____/|_____\__\_\\___/|_____|_| \_|\____|_____|
                                                         
*/





    // STATE MACHINE FOR TUTORIAL
    IEnumerator TutorialSequence()
    {



        yield return BeginningWait();



        DoTutorialSequenceSetup();



        while (debug)
        {
            //cinematicCamera.tutorialCameraIdx = debugCamIdx;
            yield return null;
        }



        //        cinematicCamera.tutorialCameraIdx = (float)Camera.Closeup;

        groupSticks.SetActive(true);
        controllerText.text = "test";


        God.postController.SetPostParameters(preSplosionPostParameters);
        OnFirstShot();
        yield return WaitWithCheat(10);

        yield return FadeGroup(groupContainer, 0, 1);
        yield return WaitWithCheat(5);

        yield return WaitForXToContinue();
        TutorialSectionComplete();

        groupSticks.SetActive(false);

        God.postController.WormHole(OnBirdBackShown);


        yield return WaitWithCheat(10);
        yield return WaitForXToContinue();
        TutorialSectionComplete();
        OnBirdZoomOutStart();
        yield return LerpCamera(cameraCloseBack, cameraFarBack, 10);
        OnBirdZoomOutEnd();

        // yield return WaitWithCheat(waitTimeInFirstShots);
        yield return WaitForXToContinue();
        TutorialSectionComplete();

        God.postController.WormHole(DoWrenSplosition, PostSplosion);
        //DoWrenSplosition();



        yield return WaitWithCheat(3);

        yield return WaitForXToContinue();
        TutorialSectionComplete();
        God.cameraManager.cinematicManager.ReleasePriority(.1f);

        // OnRotateToFrontStart();
        //cinematicCamera.tutorialCameraIdx = (float)Camera.Front;
        // yield return WaitWithCheat(waitTimeInFirstShots);
        //yield return LerpCamera((float)Camera.Front, (float)Camera.Play, 5);
        // OnRotateToFrontEnd();

        // yield return FadeBG(1, 0);

        //God.wren.shards.SpendAllShards();

        // yield return WaitWithCheat(waitTimeInFlightSpace);



        SetBGFade(0);

        God.wren.physics.rb.isKinematic = false;
        God.wren.canMove = true;
        God.wren.interfaceUtils.showStaminaRing = true;


        TutorialSectionComplete();
        stateManager.StartFreeFlight();

        SetBGFade(0);

        /*  God.wren.parameters.LoadParamSet("wrenTutorialSequence_UpDown");
          yield return WaitWithCheat(1);
          yield return FlapSequence();
          TutorialSectionComplete();
          yield return WaitWithCheat(1);
          yield return StopSequence();

          God.wren.bird.featherMaterial = featherMainMaterial;
  */

        God.wren.bird.featherMaterial = featherMainMaterial;
        God.wren.parameters.LoadParamSet("wrenTutorialSequence_Swoop");

        yield return SwoopSequence();

        TutorialSectionComplete();

        // up down feels bad
        //  God.wren.parameters.LoadParamSet("wrenTutorialSequence_UpDown");

        God.wren.parameters.LoadParamSet("wrenTutorialSequence_Swoop");
        yield return WaitWithCheat(3);
        yield return UpDownSequence();

        TutorialSectionComplete();


        yield return WaitWithCheat(3);
        God.wren.parameters.LoadParamSet("wrenTutorialSequence_LeftRight");
        yield return LeftRightSequence();


        TutorialSectionComplete();


        OnFreeFlightStarted();
        SetBGFade(0);

        yield return WaitWithCheat(3);

        God.wren.parameters.LoadParamSet("wrenTutorialSequence");
        God.wren.interfaceUtils.showForces = true;

        yield return FreeFlightSection();



        // Start free flight
        TutorialSectionComplete();

        OnDiveInstructions();

        SetBGFade(0);
        // Dive
        yield return ControllerHintSequence(ControllerHint.Dive);

        ShowProgress(0);
        StartCoroutine(FadeGroup(groupContainer, 1, 0));

        God.postController.WormHole(OnFlightTutorialEnd);

    }










    /**
  ____            _   _               _   _      _                     
 / ___|  ___  ___| |_(_) ___  _ __   | | | | ___| |_ __   ___ _ __ ___ 
 \___ \ / _ \/ __| __| |/ _ \| '_ \  | |_| |/ _ \ | '_ \ / _ \ '__/ __|
  ___) |  __/ (__| |_| | (_) | | | | |  _  |  __/ | |_) |  __/ |  \__ \
 |____/ \___|\___|\__|_|\___/|_| |_| |_| |_|\___|_| .__/ \___|_|  |___/
                                                  |_|                  
*/

    public IEnumerator BeginningWait()
    {
        yield return null;
        while (God.wren == null)
            yield return null;

        God.cameraManager.cinematicManager.SetCamera(cameraInsideCloseup.info, 1);

        while (God.wren.physics.onGround) // wait for takeoff ro we should just set this ourselves?
            yield return null;


    }


    public void DoTutorialSequenceSetup()
    {



        stateManager.SetCinematicFlightTutorialState();

        God.postController.FadeIn();

        groupContainer.alpha = 0;
        ShowContinue(false);
        ShowText();

        SetControllerHint(ControllerHint.None);
        ShowProgress(0);


        //cinematicCamera.mode = CinematicCameraHandler.Mode.Disabled;

        float bgT = 1f;
        SetBGFade(0);

        God.wren.bird.featherMaterial = featherStartMaterial;
        God.wren.bird.ResetFeatherValues();


        God.wren.bird.drawSkeleton = false;
        God.wren.canMove = false;
        God.wren.physics.rb.isKinematic = true;
        God.wren.interfaceUtils.showStaminaRing = false;
        God.wren.interfaceUtils.showForces = false;



        // set up weather manager so we get sun in right direction!
        weatherManager.sunManager.auto = false;
        weatherManager.sunManager.rawTimeInCycle = weatherManager.sunManager.daySpeed * .98f;




        OnBirdAllSetUp();



    }




    public void TutorialSectionComplete()
    {
        StartCoroutine(FadeGroup(groupContainer, 1, 0));
        ShowProgress(0);
        God.audio.Play(God.sounds.texturalHitClips);
        God.audio.Play(God.sounds.tuiCallClips);
        WaitWithCheat(3);

    }








































    Vector3 tv1;





















    /**
  _   _          ___         ____                        ____                                       
 | | | |_ __    / _ \ _ __  |  _ \  _____      ___ __   / ___|  ___  __ _ _   _  ___ _ __   ___ ___ 
 | | | | '_ \  | | | | '__| | | | |/ _ \ \ /\ / / '_ \  \___ \ / _ \/ _` | | | |/ _ \ '_ \ / __/ _ \
 | |_| | |_) | | |_| | |    | |_| | (_) \ V  V /| | | |  ___) |  __/ (_| | |_| |  __/ | | | (_|  __/
  \___/| .__/   \___/|_|    |____/ \___/ \_/\_/ |_| |_| |____/ \___|\__, |\__,_|\___|_| |_|\___\___|
       |_|                                                             |_|                          
*/


    public bool placeUpOrDown;
    IEnumerator UpDownSequence()
    {
        float t = 0;


        SetControllerHint(ControllerHint.Forward);
        StartCoroutine(FadeGroup(groupContainer, 0, 1));

        PlaceHitTarget();
        // ActivatePointer();
        //  hitTarget.SetActive(true);

        while (t < 1)
        {



            // hitTarget.transform.LookAt(hitTarget.transform.position + Vector3.forward);


            tv1 = God.wren.transform.position - targetManager.currentTarget.transform.position;

            // turn wren towards target in xz plane
            float upOrDown = Vector3.Dot(God.wren.transform.up, tv1);
            float leftOrRight = Vector3.Dot(God.wren.transform.right, Vector3.forward);


            God.wren.physics.AddForce(God.wren.transform.right * leftOrRight * moveTowardsTargetForwardMultiplier, God.wren.transform.position + God.wren.transform.forward);




            if (upOrDown > 0)
            {
                SetControllerHint(ControllerHint.Forward);
            }
            else
            {
                SetControllerHint(ControllerHint.Back);
            }


            if (tv1.magnitude < hitTargetRadius)
            {
                t += amountProgressPerHit;


                // only place new if not finished
                if (t < 1)
                {

                    // place next target
                    OnTargetHit();

                    // canHit = true;

                    if (upOrDown > 0)
                    {
                        PlaceHitTarget();
                    }
                    else
                    {
                        PlaceHitTarget();
                    }

                }
                else
                {
                    DeactivatePointer();
                    targetManager.EraseAllTargets();
                }

            }
            else
            {

                if (Vector3.Dot(God.wren.transform.forward, tv1) > 0)
                {

                    if (upOrDown > 0)
                    {
                        DeactivatePointer();
                        PlaceHitTarget();
                    }
                    else
                    {

                        DeactivatePointer();
                        PlaceHitTarget();
                    }
                }

                // fade it out if we want to make it be constant 
                //t = Mathf.Clamp01(t - Time.unscaledDeltaTime * 1.25f);
            }

            ShowProgress(t);
            // Set back to normal;

            if (Input.GetKeyDown(KeyCode.Space))
                break;

            yield return null;


        }


        DeactivatePointer();
        targetManager.EraseAllTargets();
        StartCoroutine(FadeGroup(groupContainer, 1, 0));

        //hitTarget.SetActive(false);
        // after wee have completed
        // God.wren.physics.lockX = false;
    }










    /**
    ______        _____   ___  ____    ____  _____ ___  _   _ _____ _   _  ____ _____ 
   / ___\ \      / / _ \ / _ \|  _ \  / ___|| ____/ _ \| | | | ____| \ | |/ ___| ____|
   \___ \\ \ /\ / / | | | | | | |_) | \___ \|  _|| | | | | | |  _| |  \| | |   |  _|  
    ___) |\ V  V /| |_| | |_| |  __/   ___) | |__| |_| | |_| | |___| |\  | |___| |___ 
   |____/  \_/\_/  \___/ \___/|_|     |____/|_____\__\_\\___/|_____|_| \_|\____|_____|
                                                                                      
*/



    /// top

    public bool divingOrNot;
    public float hitTime;

    public bool canPlace;

    public float maxHeightAboveBelow = 4f;
    IEnumerator SwoopSequence()
    {
        float t = 0;

        //PlaceSwoopTarget(-1);
        canPlace = true;
        divingOrNot = true;

        SetControllerHint(ControllerHint.Swoop);
        StartCoroutine(FadeGroup(groupContainer, 0, 1));

        // ActivatePointer();

        divingOrNot = true;

        float heightDiff = 10;//God.wren.transform.position.y - targetManager.currentTarget.transform.position.y;
        float oHeightDiff = 10;//heightDiff;


        while (t < 1)
        {


            //  hitTarget.transform.LookAt(hitTarget.transform.position + Vector3.up);

            // turn wren towards target in xz plane
            // float upOrDown = Vector3.Dot(God.wren.transform.up, tv1);
            float leftOrRight = Vector3.Dot(God.wren.transform.right, Vector3.forward);


            God.wren.physics.AddForce(God.wren.transform.right * leftOrRight * moveTowardsTargetForwardMultiplier, God.wren.transform.position + God.wren.transform.forward);



            // cant connect if not placed!
            if (!canPlace)
            {


                tv1 = God.wren.transform.position - targetManager.currentTarget.transform.position;
                oHeightDiff = heightDiff;
                heightDiff = God.wren.transform.position.y - targetManager.currentTarget.transform.position.y;

                if (heightDiff > 0)
                {
                    SetControllerHint(ControllerHint.Swoop);
                }
                else
                {
                    SetControllerHint(ControllerHint.Back);
                }

                // make sure we arent *too* high up or *too* low

                float fHeight = targetManager.currentTarget.transform.position.y;

                if (fHeight > God.wren.transform.position.y + maxHeightAboveBelow)
                {
                    fHeight = Mathf.Lerp(fHeight, God.wren.transform.position.y + maxHeightAboveBelow, .1f);
                }
                else if (fHeight < God.wren.transform.position.y - maxHeightAboveBelow)
                {
                    fHeight = Mathf.Lerp(fHeight, God.wren.transform.position.y - maxHeightAboveBelow, .1f);
                }


                // if (divingOrNot)
                // {
                targetManager.currentTarget.transform.position = new Vector3(
                    God.wren.transform.position.x,
                   fHeight,
                    God.wren.transform.position.z
                );






                targetManager.currentTarget.transform.position += Vector3.Scale(God.wren.transform.forward, new Vector3(1, 0, 1)).normalized * 1.2f * Mathf.Abs(targetManager.currentTarget.transform.position.y - God.wren.transform.position.y);

                //}



                if (heightDiff < 0 && oHeightDiff > 0)
                {
                    OnTargetHit();
                    t += amountProgressPerHit;
                    canPlace = true;
                    divingOrNot = false;

                }
                else if (heightDiff > 0 && oHeightDiff < 0)
                {
                    OnTargetHit();
                    t += amountProgressPerHit;
                    canPlace = true;
                    divingOrNot = true;
                }

            }


            if (canPlace)
            {
                if (Vector3.Dot(God.wren.transform.forward, Vector3.up) > .4f && !divingOrNot)
                {

                    PlaceSwoopTarget(1);
                    canPlace = false;
                }

                if (Vector3.Dot(God.wren.transform.forward, Vector3.up) < -.4f && divingOrNot)
                {
                    PlaceSwoopTarget(-1);
                    canPlace = false;
                }

            }

            ShowProgress(t);
            // Set back to normal;


            if (Input.GetKeyDown(KeyCode.Space))
                break;


            yield return null;


        }


        DeactivatePointer();
        targetManager.EraseAllTargets();
        StartCoroutine(FadeGroup(groupContainer, 1, 0));



        // after wee have completed
        // God.wren.physics.lockX = false;
    }


















    /**
_     _____ _____ _____    ___  ____    ____  ___ ____ _   _ _____   ____  _____ ___  _   _ _____ _   _  ____ _____ 
| |   | ____|  ___|_   _|  / _ \|  _ \  |  _ \|_ _/ ___| | | |_   _| / ___|| ____/ _ \| | | | ____| \ | |/ ___| ____|
| |   |  _| | |_    | |   | | | | |_) | | |_) || | |  _| |_| | | |   \___ \|  _|| | | | | | |  _| |  \| | |   |  _|  
| |___| |___|  _|   | |   | |_| |  _ <  |  _ < | | |_| |  _  | | |    ___) | |__| |_| | |_| | |___| |\  | |___| |___ 
|_____|_____|_|     |_|    \___/|_| \_\ |_| \_\___\____|_| |_| |_|   |____/|_____\__\_\\___/|_____|_| \_|\____|_____|
                                                                                                                    
*/





    public bool placeLeftOrRight;
    IEnumerator LeftRightSequence()
    {
        float t = 0;




        SetControllerHint(ControllerHint.Forward);
        StartCoroutine(FadeGroup(groupContainer, 0, 1));


        PlaceLRHitTarget();


        while (t < 1)
        {



            tv1 = God.wren.transform.position - targetManager.currentTarget.transform.position;

            targetManager.currentTarget.transform.position = new Vector3(targetManager.currentTarget.transform.position.x, God.wren.transform.position.y, targetManager.currentTarget.transform.position.z);

            // turn wren towards target in xz plane
            float upOrDown = Vector3.Dot(God.wren.transform.up, tv1);
            float leftOrRight = Vector3.Dot(God.wren.transform.right, tv1);

            if (leftOrRight > 0)
            {
                SetControllerHint(ControllerHint.Left);
            }
            else
            {
                SetControllerHint(ControllerHint.Right);
            }


            if (tv1.magnitude < hitTargetRadius)
            {
                t += amountProgressPerHit;

                if (t < 1)
                {
                    OnTargetHit();
                    PlaceLRHitTarget();

                }


            }
            else
            {


                // maybe need some new way here?
                if (Vector3.Dot(God.wren.transform.forward, tv1.normalized) > .4f)
                {

                    print("PLACING");
                    DeactivatePointer();
                    PlaceLRHitTarget();
                }


            }

            ShowProgress(t);


            if (Input.GetKeyDown(KeyCode.Space))
                break;
            yield return null;


        }

        DeactivatePointer();
        targetManager.EraseAllTargets();
        StartCoroutine(FadeGroup(groupContainer, 1, 0));


        //  hitTarget.SetActive(false);

    }

    // set back to normal params























    /**
          _____ ____  _____ _____   _____ _     ___ ____ _   _ _____   ____  _____ ____ _____ ___ ___  _   _ 
         |  ___|  _ \| ____| ____| |  ___| |   |_ _/ ___| | | |_   _| / ___|| ____/ ___|_   _|_ _/ _ \| \ | |
         | |_  | |_) |  _| |  _|   | |_  | |    | | |  _| |_| | | |   \___ \|  _|| |     | |  | | | | |  \| |
         |  _| |  _ <| |___| |___  |  _| | |___ | | |_| |  _  | | |    ___) | |__| |___  | |  | | |_| | |\  |
         |_|   |_| \_\_____|_____| |_|   |_____|___\____|_| |_| |_|   |____/|_____\____| |_| |___\___/|_| \_|
                                                                                                             
*/


    public float maxDistanceFreeflightFromTarget = 50;


    IEnumerator FreeFlightSection()
    {
        float t = 0;


        God.wren.physics.showDebugForces = true;



        SetControllerHint(ControllerHint.Gentle);
        StartCoroutine(FadeGroup(groupContainer, 0, 1));

        PlaceFreeFlightTarget();
        // ActivatePointer();

        while (t < 1)
        {


            tv1 = God.wren.transform.position - targetManager.currentTarget.transform.position;

            if (tv1.magnitude > maxDistanceFreeflightFromTarget)
            {
                targetManager.currentTarget.transform.position = God.wren.transform.position - tv1.normalized * maxDistanceFreeflightFromTarget;
            }

            if (tv1.magnitude < hitTargetRadius)
            {

                print("htittt");
                t += .1f;

                OnTargetHit();
                PlaceFreeFlightTarget();

            }
            else
            {

                if (Vector3.Dot(God.wren.transform.forward, tv1) > .4f)
                {
                    DeactivatePointer();
                    PlaceFreeFlightTarget();
                }


                // TODO do we need helpers here?
                //if (Vector3.Dot(God.wren.transform.forward, tv1) > .4f) { PlaceLRHitTarget(); }
            }

            ShowProgress(t);



            yield return null;


        }


        DeactivatePointer();
        targetManager.EraseAllTargets();
        StartCoroutine(FadeGroup(groupContainer, 1, 0));



    }

    // set back to normal params

























    /**
          _____ _        _    ____    ____  _____ ___  _   _ _____ _   _  ____ _____ 
         |  ___| |      / \  |  _ \  / ___|| ____/ _ \| | | | ____| \ | |/ ___| ____|
         | |_  | |     / _ \ | |_) | \___ \|  _|| | | | | | |  _| |  \| | |   |  _|  
         |  _| | |___ / ___ \|  __/   ___) | |__| |_| | |_| | |___| |\  | |___| |___ 
         |_|   |_____/_/   \_\_|     |____/|_____\__\_\\___/|_____|_| \_|\____|_____|
                                                                                     
*/




    // Only tells you to stop once you are out of stamina 


    public bool staminaLowHit;
    IEnumerator FlapSequence()
    {


        float t = 0;

        SetControllerHint(ControllerHint.Flap);
        StartCoroutine(FadeGroup(groupContainer, 0, 1));

        bool flapStart = false;

        while (t < 1)
        {

            //            print(flapStart);

            if (staminaLowHit)
            {

                SetControllerHint(ControllerHint.Release2);
            }
            else
            {
                SetControllerHint(ControllerHint.Flap);
            }

            if (God.wren.stats.stamina < .3f)
            {
                if (staminaLowHit == false)
                {
                    t += .3f;
                    staminaLowHit = true;
                }
            }

            if (God.wren.stats.stamina > .95f)
            {
                staminaLowHit = false;
            }





            // print(flapStart);

            if (God.input.l2 > .5f && God.input.r2 > .5f)
            {
                if (flapStart == false)
                {

                    StartCoroutine(FadeGroup(groupContainer, 1, 0));
                    flapStart = true;
                }
            }
            else
            {

                if (flapStart)
                {
                    // t += .1f;
                    // DO GOOD FLAP FEEDBACK here

                    StartCoroutine(FadeGroup(groupContainer, 0, 1));
                    flapStart = false;
                }
            }




            ShowProgress(t);
            if (Input.GetKeyDown(KeyCode.Space))
                break;
            yield return null;


        }


        DeactivatePointer();
        targetManager.EraseAllTargets();
        StartCoroutine(FadeGroup(groupContainer, 1, 0));

    }
















    /**
          ____ _____ ___  ____    ____  _____ ___  _   _ _____ _   _  ____ _____ 
         / ___|_   _/ _ \|  _ \  / ___|| ____/ _ \| | | | ____| \ | |/ ___| ____|
         \___ \ | || | | | |_) | \___ \|  _|| | | | | | |  _| |  \| | |   |  _|  
          ___) || || |_| |  __/   ___) | |__| |_| | |_| | |___| |\  | |___| |___ 
         |____/ |_| \___/|_|     |____/|_____\__\_\\___/|_____|_| \_|\____|_____|
                                                                                 
*/






    IEnumerator StopSequence()
    {
        float t = 0;

        SetControllerHint(ControllerHint.Hold);
        StartCoroutine(FadeGroup(groupContainer, 0, 1));

        while (t < 1)
        {
            if (God.wren.physics.rb.velocity.magnitude < 2)
            {
                t += .01f;
            }

            ShowProgress(t);
            if (Input.GetKeyDown(KeyCode.Space))
                break;
            yield return null;
        }

        StartCoroutine(FadeGroup(groupContainer, 1, 0));

        DeactivatePointer();
        targetManager.EraseAllTargets();
        ShowProgress(0);
    }





























    /**
  _     _ _   _   _      _                     
 | |   (_) | | | | | ___| |_ __   ___ _ __ ___ 
 | |   | | | | |_| |/ _ \ | '_ \ / _ \ '__/ __|
 | |___| | | |  _  |  __/ | |_) |  __/ |  \__ \
 |_____|_|_| |_| |_|\___|_| .__/ \___|_|  |___/
                          |_|                  
*/





    public void StartTutorial()
    {
        tutSequence = StartCoroutine(TutorialSequence());
    }







    public void ActivatePointer()
    {
        //hitTarget.SetActive(true);
        God.wren.interfaceUtils.interfacePointer.AddPointer(targetManager.currentTarget.transform, 0, new Vector4(0, 0, 0, 1));
        God.wren.interfaceUtils.interfacePointer.TurnOnPointer(targetManager.currentTarget.transform);
    }


    public void DeactivatePointer()
    {
        if (targetManager.currentTarget != null)
        {
            God.wren.interfaceUtils.RemovePointer(targetManager.currentTarget.transform);
        }
        targetManager.EraseCurrentTarget();
        // hitTarget.SetActive(false);
    }






    void OnBirdAllSetUp()
    {
        God.wren.canMove = false;
        God.audio.Play(God.sounds.smallSuccessSound);
    }



    public void OnTargetHit()
    {
        God.audio.PlayBasedOnWrenSpeed(God.sounds.texturalHitClips[Random.Range(0, God.sounds.texturalHitClips.Length)]);
        God.wren.shards.CollectShards(shardsPerTargetHit, Random.Range(0, 10f), hitTarget.transform.position);

        print("TARGET HIT");
        God.particleSystems.Emit(God.particleSystems.smallSuccessParticleSystem, God.wren.transform.position + God.wren.transform.forward * 5f, 100);
        DeactivatePointer();
        targetManager.HitTarget(God.wren.physics.speed);
        //God.particleSystems.transform.position = God.wren.transform.position + God.wren.transform.forward * 5;
        //God.particleSystems.smallSuccessParticleSystem.Play();


    }





    void ShowText(string text = null)
    {
        controllerText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(text));
        controllerText.text = text;
    }

    void ShowContinue(bool bShow)
    {
        xToContinue.gameObject.SetActive(bShow);
    }






    /**
          ____  _        _    ____ _____   _____  _    ____   ____ _____ _____   _   _ _____ _     ____  _____ ____  ____  
         |  _ \| |      / \  / ___| ____| |_   _|/ \  |  _ \ / ___| ____|_   _| | | | | ____| |   |  _ \| ____|  _ \/ ___| 
         | |_) | |     / _ \| |   |  _|     | | / _ \ | |_) | |  _|  _|   | |   | |_| |  _| | |   | |_) |  _| | |_) \___ \ 
         |  __/| |___ / ___ \ |___| |___    | |/ ___ \|  _ <| |_| | |___  | |   |  _  | |___| |___|  __/| |___|  _ < ___) |
         |_|   |_____/_/   \_\____|_____|   |_/_/   \_\_| \_\\____|_____| |_|   |_| |_|_____|_____|_|   |_____|_| \_\____/ 
                                                                                                                           
*/

    void PlaceSwoopTarget(int upDown)
    {
        targetManager.SetTarget(God.wren.transform.position + Vector3.forward * 50 + Vector3.up * (float)upDown * 20, 2);
        ActivatePointer();
    }



    void PlaceHitTarget()
    {
        targetManager.SetTarget(God.wren.transform.position + Vector3.forward * 100 + Vector3.up * (placeUpOrDown ? 10 : -10), 1);
        placeUpOrDown = !placeUpOrDown;
        ActivatePointer();
    }


    void PlaceLRHitTarget()
    {
        Vector3 flatWrenForward = Vector3.Scale(God.wren.transform.forward, new Vector3(1, 0, 1));
        Vector3 flatWrenRight = Vector3.Scale(God.wren.transform.right, new Vector3(1, 0, 1));

        float leftOrRight = Vector3.Dot(flatWrenForward, Vector3.right);

        if (leftOrRight > 0)
        {
            targetManager.SetTarget(God.wren.transform.position + flatWrenForward * 100 + flatWrenRight * -20, 0);
        }
        else
        {
            targetManager.SetTarget(God.wren.transform.position + flatWrenForward * 100 + flatWrenRight * 20, 0);
        }


        placeLeftOrRight = !placeLeftOrRight;
        ActivatePointer();

    }



    void PlaceFreeFlightTarget()
    {
        targetManager.SetTarget(God.wren.transform.position + 100 * (Vector3.Scale(God.wren.transform.forward * 3 + Random.onUnitSphere, new Vector3(1f, .2f, 1f))), 0);
        ActivatePointer();
    }













    /**
    ____ ___ ____   _   _ _____ _     ____  _____ ____  ____  
   | __ )_ _/ ___| | | | | ____| |   |  _ \| ____|  _ \/ ___| 
   |  _ \| | |  _  | |_| |  _| | |   | |_) |  _| | |_) \___ \ 
   | |_) | | |_| | |  _  | |___| |___|  __/| |___|  _ < ___) |
   |____/___\____| |_| |_|_____|_____|_|   |_____|_| \_\____/ 
                                                              
*/



    IEnumerator ControllerHintSequence(ControllerHint hint)
    {
        SetControllerHint(hint);

        StartCoroutine(FadeGroup(groupContainer, 0, 1));
        yield return WaitWithCheat(0.25f);

        float t = 0;
        while (t < 1)
        {
            if (TestControllerHint(hint))
                t += Time.unscaledDeltaTime * .45f;
            else
                t = Mathf.Clamp01(t - Time.unscaledDeltaTime * 1.25f);

            ShowProgress(t);
            yield return null;
        }

        ShowProgress(0);
        SetControllerHint(ControllerHint.None);
    }

    bool TestControllerHint(ControllerHint hint)
    {
        switch (hint)
        {
            case ControllerHint.Left:
                return God.input.left.x < -.5f && God.input.right.x < -.5f;
            case ControllerHint.Right:
                return God.input.left.x > .5f && God.input.right.x > .5f;
            case ControllerHint.Forward:
                return God.input.left.y > .5f && God.input.right.y > .5f;
            case ControllerHint.Back:
                return God.input.left.y < -.5f && God.input.right.y < -.5f;
            case ControllerHint.Dive:
                return God.input.l2 > .5f && God.input.r2 > .5f;
            case ControllerHint.Hold:
                return God.input.l3 && God.input.r3;
        }
        return false;
    }

    MaterialPropertyBlock bgMpr;
    public void SetBGFade(float t)
    {
        if (bgMpr == null)
            bgMpr = new MaterialPropertyBlock();
        fade.gameObject.SetActive(t > 0);
        if (Mathf.Approximately(t, 0))
            return;
        fade.GetPropertyBlock(bgMpr);
        bgMpr.SetColor("_Color", fadeGradient.Evaluate(t));
        fade.SetPropertyBlock(bgMpr);
    }


    IEnumerator WaitForXToContinue()
    {
        bool wait = true;
        var t = 0f;
        groupContainer.alpha = 1;
        ShowProgress(t);

        ShowContinue(true);

        //yield return WaitWithCheat(0.5f);

        bool lastX = false;// = God.input.x;
        wait = true;
        while (wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
                wait = false;
            lastX = God.input.x;
            // if (God.input.x)
            // wait = false;
            // else
            yield return null;
        }

        _lastSequenceTime = Time.unscaledTime;
        // yield return FadeGroup(groupContainer, 0, 1));
        // while (HandleSticksProgress(ref t, speed: 1.7f, gravity: true))
        //     yield return null;

        groupContainer.alpha = 0;
        ShowContinue(false);
    }

    bool HandleSticksProgress(ref float t, float speed = 2f, bool gravity = true)
    {
        if (
            Mathf.Abs(God.input.left.x) > .1f ||
            Mathf.Abs(God.input.left.y) > .1f ||
            Mathf.Abs(God.input.right.x) > .1f ||
            Mathf.Abs(God.input.right.y) > .1f ||

            God.input.l2 > 0.1f ||
            God.input.r2 > 0.1f
        )
            t += Time.unscaledDeltaTime * .1f * speed;
        else if (gravity)
            t = Mathf.Clamp01(t - Time.unscaledDeltaTime * .05f);

        ShowProgress(t);
        Debug.Log(t);
        return t < 1;
    }

    void ShowProgress(float t = 0)
    {
        progressBar.transform.parent.gameObject.SetActive(t > 0);
        progressBar.localScale = new Vector3(Mathf.Clamp01(t * t), 1, 1);
    }

    IEnumerator FadeGroup(CanvasGroup group, float from = 0, float to = 1, float delay = 0)
    {
        float t = 0;
        float duration = 0.5f;
        float _ct = Time.unscaledTime;
        while (t < duration)
        {
            if (delay > 0 && Time.unscaledTime - _ct < delay)
            {
                yield return null;
                continue;
            }
            group.alpha = Mathf.Lerp(from, to, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        group.alpha = to;
    }


    IEnumerator FadeBG(float from = 0, float to = 1)
    {
        float t = 0;
        float duration = 1;
        while (t < duration)
        {

            t += Time.unscaledDeltaTime;
            float nTime = t / duration;

            SetBGFade(Mathf.Lerp(from, to, nTime));
            yield return null;

        }
    }














    void SetControllerHint(ControllerHint hint)
    {
        groupLeft.SetActive(hint == ControllerHint.Left);
        groupRight.SetActive(hint == ControllerHint.Right);
        groupUp.SetActive(hint == ControllerHint.Forward);
        groupDown.SetActive(hint == ControllerHint.Back);
        groupDive.SetActive(hint == ControllerHint.Dive);
        groupHold.SetActive(hint == ControllerHint.Hold);
        groupFlap.SetActive(hint == ControllerHint.Flap);
        groupSwoop.SetActive(hint == ControllerHint.Swoop);
        groupRelease.SetActive(hint == ControllerHint.Release);
        groupRelease2.SetActive(hint == ControllerHint.Release2);
        groupGentle.SetActive(hint == ControllerHint.Gentle);
        groupBoost.SetActive(hint == ControllerHint.Boost);
        groupPing.SetActive(hint == ControllerHint.Ping);

        controllerText.transform.parent.gameObject.SetActive(hint != ControllerHint.None); // turns it off if we arent using any text 


        switch (hint)
        {
            case ControllerHint.Dive:
                controllerText.text = "Hold to DIVE";
                break;
            case ControllerHint.Left:
                controllerText.text = "Push Sticks LEFT to TURN LEFT";
                break;
            case ControllerHint.Right:
                controllerText.text = "Push Sticks RIGHT to TURN RIGHT";
                break;
            case ControllerHint.Forward:
                controllerText.text = "Push Sticks FORWARD to FLY DOWN";
                break;
            case ControllerHint.Back:
                controllerText.text = "Push Sticks BACK to FLY UP";
                break;
            case ControllerHint.Hold:
                controllerText.text = "PRESS sticks to BRAKE";
                break;
            case ControllerHint.Flap:
                controllerText.text = "Tap R2 & L2 to FLAP";
                break;
            case ControllerHint.Swoop:
                controllerText.text = "Press R2 & L2 to SWOOP";
                break;
            case ControllerHint.Release:
                controllerText.text = "Release R2 & L2 to GLIDE";
                break;
            case ControllerHint.Release2:
                controllerText.text = "Release R2 & L2 to REGAIN STAMINA";
                break;
            case ControllerHint.Gentle:
                controllerText.text = "Use All Sticks GENTLY";
                break;
            case ControllerHint.Boost:
                controllerText.text = "Press O to BOOST";
                break;
            case ControllerHint.Ping:
                controllerText.text = "Press Triangle to PING";
                break;
                /* case ControllerHint.y:
                     controllerText.text = "PRESS sticks to HOLD";
                     break;*/
                /*            case ControllerHint.TakeOff:
                                controllerText.text = "PRESS X to TAKE OFF";
                                break;*/
        }
    }

    IEnumerator WaitWithCheat(float seconds)
    {
        float t = 0;
        while (t < seconds)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator LerpCamera(float from, float to, float time)
    {
        float cT = 0;
        while (cT < time)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                break;
            //cinematicCamera.tutorialCameraIdx = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, cT / time));
            cT += Time.unscaledDeltaTime;
            yield return null;
        }
        //cinematicCamera.tutorialCameraIdx = to;
    }


    IEnumerator LerpCamera(CinematicCamera from, CinematicCamera to, float time)
    {
        float cT = 0;
        while (cT < time)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                break;
            //cinematicCamera.tutorialCameraIdx = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, cT / time));
            God.cameraManager.cinematicManager.LerpCamera(from.info, to.info, cT / time);


            cT += Time.unscaledDeltaTime;
            yield return null;
        }

        God.cameraManager.cinematicManager.SetCamera(to.info, 1);
    }

    enum Camera
    {
        Closeup = 0,
        TopClose = 1,
        TopFar = 2,
        Front = 3,
        Behind = 4,
        Play = 5
    }













    /**
_______     _______ _   _ _____ ____  
| ____\ \   / / ____| \ | |_   _/ ___| 
|  _|  \ \ / /|  _| |  \| | | | \___ \ 
| |___  \ V / | |___| |\  | | |  ___) |
|_____|  \_/  |_____|_| \_| |_| |____/ 
                                        
*/


    public void OnFirstShot() { }
    public void OnBirdBackShown()
    {
        God.cameraManager.cinematicManager.SetCamera(cameraCloseBack.info, 1);
    }
    public void OnBirdZoomOutStart() { }
    public void OnBirdZoomOutEnd() { }
    public void OnRotateToFrontStart() { }
    public void OnRotateToFrontEnd() { }

    public void OnPushLeftInstructions() { }
    public void OnPushRightInstructions() { }
    public void OnHoldInstructions() { }
    public void OnDiveInstructions() { }





    public void DoWrenSplosition()
    {

        God.wren.bird.Explode();
        God.wren.shards.SpendAllShards();
        God.wren.bird.drawSkeleton = true;
        God.audio.Play(God.sounds.texturalHitClips);

    }

    public void PostSplosion()
    {

        God.postController.SetPostParameters(postSplosionPostParameters);
    }



    public void OnFlightTutorialEnd()
    {
        stateManager.StartTransition();
    }
































































    /*



    CARDS FUNCTIONS



    */


    // Cards

    // public static bool 
    public void OnTutorialCardTriggered(CardType cardType, Transform target = null, bool pause = true)
    {
        TryShowCard(cardType, target: target, pause: pause);
    }

    public enum CardType
    {
        None,

        RevealIsland,
        FlyCloseToGround,

        // activities
        ActivityRings, ActivityWindTunnel, ActivitySpeedGate, ActivityButterflies, ActivityBigBird,

        // takeoff
        TakeOff,

        // custom
        CycleThroughTriggers = 100,
        TutorialEnd = 101
    }

    private Dictionary<CardType, bool> _cardShown = new Dictionary<CardType, bool>();

    public void GetCardInfo(CardType type, out string title, out string text)
    {
        title = "";
        text = "";
        switch (type)
        {
            case CardType.RevealIsland:
                title = "Bird Island";
                text = "Welcome to Bird Island. Fly around freely and explore.";
                break;

            case CardType.FlyCloseToGround:
                title = "Ground";
                text = "Fly close to the ground to gain speed.";
                break;

            case CardType.ActivityRings:
                title = "Rings";
                text = "Rings give you a boost when you fly through them.";
                break;
            case CardType.ActivityWindTunnel:
                title = "Wind Tunnel";
                text = "Take a ride on a wind tunnel. Tuck your wings (L and R triggers) to go faster.";
                break;
            case CardType.ActivitySpeedGate:
                title = "Speed Gate";
                text = "Fly through a speed gate as fast as you can.";
                break;
            case CardType.ActivityButterflies:
                title = "Butterflies";
                text = "Collect butterflies to eat.";
                break;
            case CardType.ActivityBigBird:
                title = "Big Bird";
                text = "Follow the giant bird. Get close to hear its song.";
                break;
            case CardType.CycleThroughTriggers:
                title = "Activities";
                text = "Find activities around the map.";
                break;
            case CardType.TutorialEnd:
                title = "Gate";
                text = "Enter the mountain gate to finish the demo.";
                break;
            case CardType.TakeOff:
                title = "Take Off";
                text = "Press X to Take Off.";
                break;
        }
    }
    public void TryShowCard(CardType cardType, float delay = 0, Transform target = null, bool pause = false)
    {
        if (_cardShown == null)
            _cardShown = new Dictionary<CardType, bool>();

        if (_showingCard || _cardShown.ContainsKey(cardType) && _cardShown[cardType])
            return;

        SetCardInfo(cardType);

        _cardShown[cardType] = true;

        if (cardType == CardType.RevealIsland)
            StartCoroutine(ShowTutorialStartCardSequence());
        else
            StartCoroutine(ShowCardSequence(delay, target, pause));
    }

    void SetCardInfo(CardType type)
    {
        GetCardInfo(type, out string title, out string text);
        cardTitle.text = title;
        cardText.text = text;
        cardFlyOnGround.SetActive(type == CardType.FlyCloseToGround);
    }

    bool _showingCard = false;
    IEnumerator ShowCardSequence(float delay = 0, Transform target = null, bool pause = false)
    {
        const float TIMESCALE_LOW = 0.001f;

        _showingCard = true;

        bool wait = true;
        groupCard.alpha = 0;
        groupXToContinue.alpha = 0;

        groupCard.gameObject.SetActive(true);
        groupXToContinue.gameObject.SetActive(true);

        if (target)
            God.wren.cameraWork.objectTargeted = target;

        if (delay > 0)
            yield return WaitWithCheat(delay);

        StartCoroutine(FadeGroup(groupCard, 0, 1));

        float t = 1;
        var prevTimescale = Time.timeScale;
        while (pause && t > 0)
        {
            t -= Time.unscaledDeltaTime * 1.2f;
            Time.timeScale = Mathf.Lerp(TIMESCALE_LOW, prevTimescale, t);

            yield return null;
        }

        groupXToContinue.alpha = 1;

        yield return WaitWithCheat(0.4f);

        bool lastX = God.input.x;
        wait = true;
        while (wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            lastX = God.input.x;
            yield return null;
        }

        t = 0;
        while (pause && t < 1)
        {
            t += Time.unscaledDeltaTime * .5f;
            groupCard.alpha = 1 - t;
            Time.timeScale = Mathf.Lerp(TIMESCALE_LOW, prevTimescale, Mathf.Clamp01(t));

            yield return null;
        }

        if (target)
            God.wren.cameraWork.objectTargeted = null;

        Time.timeScale = 1;
        groupCard.alpha = 0;
        groupXToContinue.alpha = 0;

        _showingCard = false;
    }

    IEnumerator ShowTutorialStartCardSequence()
    {
        _showingCard = true;

        groupCard.alpha = 0;
        groupXToContinue.alpha = 0;
        // cinematicCamera.mode = CinematicCameraHandler.Mode.Disabled;
        groupXToContinue.gameObject.SetActive(true);

        SetCardInfo(CardType.RevealIsland);

        groupCard.gameObject.SetActive(true);
        groupXToContinue.gameObject.SetActive(true);

        God.wren.physics.rb.isKinematic = true;

        StartCoroutine(FadeGroup(groupCard, 0, 1));

        yield return WaitWithCheat(2.5f);

        groupXToContinue.alpha = 1;

        bool wait = true, lastX = God.input.x;
        while (wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            lastX = God.input.x;
            yield return null;
        }

        SetCardInfo(CardType.CycleThroughTriggers);

        //   cinematicCamera.mode = CinematicCameraHandler.Mode.Activities;

        groupXToContinue.alpha = 0;
        yield return WaitWithCheat(2.5f);
        groupXToContinue.alpha = 1;

        wait = true;
        while (wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            lastX = God.input.x;
            yield return null;
        }

        SetCardInfo(CardType.FlyCloseToGround);

        groupXToContinue.alpha = 0;
        yield return WaitWithCheat(2.5f);
        groupXToContinue.alpha = 1;

        wait = true;
        while (wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            lastX = God.input.x;
            yield return null;
        }

        SetCardInfo(CardType.TutorialEnd);

        //   cinematicCamera.mode = CinematicCameraHandler.Mode.TutorialEnd;

        groupXToContinue.alpha = 0;
        yield return WaitWithCheat(1.5f);
        groupXToContinue.alpha = 1;

        wait = true;
        while (wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            lastX = God.input.x;
            yield return null;
        }

        God.wren.physics.rb.isKinematic = false;

        //  cinematicCamera.mode = CinematicCameraHandler.Mode.Disabled;
        groupCard.alpha = 0;
        groupXToContinue.alpha = 0;

        _showingCard = false;
    }

    public void TryEndDemo(System.Action<bool> cb = null)
    {
        StartCoroutine(EndDemoSequence(cb));
    }
    IEnumerator EndDemoSequence(System.Action<bool> cb = null)
    {
        groupEnd.alpha = 1;

        God.wren.physics.rb.isKinematic = true;

        bool wait = true;
        while (wait)
        {
            if (God.input.squarePressed)
            {
                wait = false;
            }
            if (God.input.xPressed)
            {
                wait = false;
                cb?.Invoke(true);
            }
            yield return null;
        }
        // cancel
        God.wren.physics.rb.isKinematic = false;
        groupEnd.alpha = 0;
        cb?.Invoke(false);
    }


    /*


    CHEATS


    */



    public void CheckCheats()
    {
        if (Application.isEditor)
        {

            // tab to autocomplete
            if (tutSequence != null && Input.GetKeyDown(KeyCode.Tab))
            {
                StopCoroutine(tutSequence);

                stateManager.StartTransition();

                God.wren.PhaseShift(ender.transform.position + Vector3.down * 180);
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                // wind tunnel 1
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    God.wren.PhaseShift(new Vector3(-5012, 183, -544));
                }
                // wind tunnel 2
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    God.wren.PhaseShift(new Vector3(-3859, 287, -1337));
                }
                // portal
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    God.wren.PhaseShift(new Vector3(-5150, 507, -659));
                }
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                TryShowCard(CardType.ActivityRings);
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                _cardShown = new Dictionary<CardType, bool>();
            }
        }
    }

}




