using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

public class FlyingTutorialSequence : MonoBehaviour
{
    public static UnityAction OnTutorialStart;
    public static UnityAction OnTutorialDiveFinished;
    // public static bool 

    public CinematicCameraHandler cinematicCamera;

    public CanvasGroup groupContainer;
    public CanvasGroup xToContinue;

    public TextMeshProUGUI controllerText;

    public GameObject groupSticks;
    public GameObject groupDive;
    public RectTransform progressBar;

    public bool debug;
    public int debugCamIdx = 0;

    public Gradient fadeGradient;
    public Renderer fade;

    public TutorialEnder ender;

    Coroutine tutSequence;


    void Start()
    {
        if (!Application.isEditor)
            debug = false;

        if (!ender)
            ender = FindObjectOfType<TutorialEnder>();

        tutSequence = StartCoroutine(TutorialSequence());
    }


    void Update()
    {
        if (Application.isEditor)
        {
            if (tutSequence != null && Input.GetKeyDown(KeyCode.Tab))
            {
                StopCoroutine(tutSequence);
                
                OnTutorialFinished();

                God.wren.PhaseShift(ender.transform.position + Vector3.down * 180);
            }
        }

        fade.transform.position = God.camera.transform.position;

    }

    IEnumerator TutorialSequence()
    {
        yield return null;

        OnTutorialStart?.Invoke();

        God.fade.FadeIn(3);

        groupContainer.alpha = 0;
        ShowContinue(false);
        ShowText();

        groupSticks.SetActive(false);
        groupDive.SetActive(false);
        ShowProgress(0);
        cinematicCamera.armed = false;
        SetBGFade(1);

        while (God.wren == null)
            yield return null;

        yield return new WaitForSecondsRealtime(2);

        while (God.wren.physics.onGround)
            yield return null;

        cinematicCamera.armed = true;

        while(debug)
        {
            cinematicCamera.tutorialCameraIdx = debugCamIdx;
            yield return null;
        }

        cinematicCamera.tutorialCameraIdx = 0;
        yield return new WaitForSecondsRealtime(5);

        // Sticks
        {
            groupSticks.SetActive(true);
            controllerText.text = "";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
        }
        yield return new WaitForSecondsRealtime(5);
        
        yield return CameraSequence();
        groupContainer.alpha = 0;
        groupSticks.SetActive(false);

        cinematicCamera.tutorialCameraIdx++; // 1
        yield return CameraSequence();
        cinematicCamera.tutorialCameraIdx++; // 2
        yield return CameraSequence();
        cinematicCamera.tutorialCameraIdx++; // 3
        yield return CameraSequence();
        cinematicCamera.tutorialCameraIdx++; // 4
        yield return CameraSequence();
        cinematicCamera.tutorialCameraIdx++; // 5
        float bgT = 1;
        while(bgT > 0)
        {
            SetBGFade(bgT);
            bgT -= Time.deltaTime * .2f;
            yield return null;
        }
        SetBGFade(0);
        yield return new WaitForSecondsRealtime(6);
        yield return CameraSequence();

        // Game cam
        cinematicCamera.armed = false;
        yield return StartCoroutine(FadeGroup(groupContainer, 1, 0));

        // yield return new WaitForSecondsRealtime(10);

        // // Sticks
        // {
        //     groupSticks.SetActive(true);
        //     controllerText.text = "Wings";

        //     yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
        //     yield return new WaitForSecondsRealtime(8);
        //     yield return StartCoroutine(FadeGroup(groupContainer, 1, 0));
        // }
        
        yield return new WaitForSecondsRealtime(15);

        // Dive
        {
            groupSticks.SetActive(false);
            groupDive.SetActive(true);
            controllerText.transform.parent.gameObject.SetActive(true);
            controllerText.text = "Hold to DIVE";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
            yield return new WaitForSecondsRealtime(0.25f);

            // TODO: wait for diving enough time

            float diveT = 0;
            while (diveT < 1)
            {
                if (God.input.l2 > .5f && God.input.r2 > .5f)
                    diveT += Time.deltaTime * .45f;
                else
                    diveT = Mathf.Clamp01(diveT - Time.deltaTime * 1.25f);

                ShowProgress(diveT);
                yield return null;
            }
        }
        ShowProgress(0);
        StartCoroutine(FadeGroup(groupContainer, 1, 0));

        OnTutorialFinished();



    }

    MaterialPropertyBlock bgMpr;
    void SetBGFade(float t)
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

    void ShowText(string text = null)
    {
        controllerText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(text));
        controllerText.text = text;
    }

    void ShowContinue(bool bShow)
    {
        xToContinue.gameObject.SetActive(bShow);
    }

    IEnumerator CameraSequence()
    {
        bool wait = true;
        var t = 0f;
        groupContainer.alpha = 1;
        ShowProgress(t);

        ShowContinue(false);

        yield return new WaitForSecondsRealtime(0.5f);
        bool lastX = God.input.x;
        wait = true; t = 0;
        while(wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            lastX = God.input.x;
            
            t += Time.deltaTime;
            if (t > 7)
                wait = false;
            yield return null;
        }

        ShowContinue(true);
        wait = true;
        while(wait)
        {
            if (God.input.x)
                wait = false;
            else
                yield return null;
        }
        // yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
        // while (HandleSticksProgress(ref t, speed: 1.7f, gravity: true))
        //     yield return null;

        groupContainer.alpha = 0;
        ShowContinue(false);
    }

    bool HandleSticksProgress(ref float t, float speed = 2f, bool gravity = true)
    {
        if (
            Mathf.Abs(God.input.leftX) > .1f || 
            Mathf.Abs(God.input.leftY) > .1f || 
            Mathf.Abs(God.input.rightX) > .1f || 
            Mathf.Abs(God.input.rightY) > .1f || 
            
            God.input.l2 > 0.1f || 
            God.input.r2 > 0.1f
        )
            t += Time.deltaTime * .1f * speed;
        else if (gravity)
            t = Mathf.Clamp01(t - Time.deltaTime * .05f);

        ShowProgress(t);
        Debug.Log(t);
        return t < 1;
    }

    void ShowProgress(float t = 0)
    {
        progressBar.transform.parent.gameObject.SetActive(t > 0);
        progressBar.localScale = new Vector3(Mathf.Clamp01(t * t), 1, 1);
    }

    IEnumerator FadeGroup(CanvasGroup group, float from = 0, float to = 1)
    {
        float t = 0;
        float duration = 1.5f;
        while (t < duration)
        {
            group.alpha = Mathf.Lerp(from, to, t);
            t += Time.deltaTime;
            yield return null;
        }
        group.alpha = to;
    }

    void OnTutorialFinished()
    {
        cinematicCamera.armed = false;
        ShowProgress(0);
        SetBGFade(0);
        groupContainer.alpha = 0;

        OnTutorialDiveFinished?.Invoke();
    }
}
