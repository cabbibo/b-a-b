using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WrenUtils;

public class InterfaceTutorial : MonoBehaviour
{


    public Renderer fade;

    public CanvasGroup groupContainer;
    public CanvasGroup xToContinue;

    public TextMeshProUGUI controllerText;

    public RectTransform progressBar;

    public bool debug;
    public int debugCamIdx = 0;

    public Gradient fadeGradient;



    float _lastSequenceTime;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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



    public IEnumerator WaitForXToContinue()
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
            yield return null;
        }

        _lastSequenceTime = Time.unscaledTime;

        groupContainer.alpha = 0;
        ShowContinue(false);
    }



    public IEnumerator FadeGroup(CanvasGroup group, float from = 0, float to = 1, float delay = 0)
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


    public IEnumerator FadeBG(float from = 0, float to = 1)
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






    public IEnumerator WaitWithCheat(float seconds)
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

    public IEnumerator LerpCamera(float from, float to, float time)
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


    public IEnumerator LerpCamera(CinematicCamera from, CinematicCamera to, float time)
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

    public void ShowProgress(float t = 0)
    {
        progressBar.transform.parent.gameObject.SetActive(t > 0);
        progressBar.localScale = new Vector3(Mathf.Clamp01(t * t), 1, 1);
    }


    public void ShowText(string text = null)
    {
        controllerText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(text));
        controllerText.text = text;
    }

    public void ShowContinue(bool bShow)
    {
        xToContinue.gameObject.SetActive(bShow);
    }



    public void TutorialSectionComplete()
    {
        StartCoroutine(FadeGroup(groupContainer, 1, 0));
        ShowProgress(0);
        God.audio.Play(God.sounds.texturalHitClips);
        God.audio.Play(God.sounds.tuiCallClips);
        WaitWithCheat(3);

    }





    /**
    ____ ___ ____   _   _ _____ _     ____  _____ ____  ____  
   | __ )_ _/ ___| | | | | ____| |   |  _ \| ____|  _ \/ ___| 
   |  _ \| | |  _  | |_| |  _| | |   | |_) |  _| | |_) \___ \ 
   | |_) | | |_| | |  _  | |___| |___|  __/| |___|  _ < ___) |
   |____/___\____| |_| |_|_____|_____|_|   |_____|_| \_\____/ 
                                                              
*/


    public enum ControllerHint { None, Dive, Left, Right, Forward, Back, Hold, Takeoff, Flap, Swoop, Release, Release2, Gentle, Boost, Ping }

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

    public IEnumerator ControllerHintSequence(ControllerHint hint)
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

    public bool TestControllerHint(ControllerHint hint)
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


    public bool HandleSticksProgress(ref float t, float speed = 2f, bool gravity = true)
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




    public void SetControllerHint(ControllerHint hint)
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



    public void FadeFullGroupCoroutine(float start, float end)
    {
        StartCoroutine(FadeGroup(groupContainer, start, end));
    }




}
