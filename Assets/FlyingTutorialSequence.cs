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

    [Header("Tooltip Cards")]
    public CanvasGroup groupCard;
    public CanvasGroup groupXToContinue;
    public TextMeshProUGUI cardTitle;
    public TextMeshProUGUI cardText;


    Coroutine tutSequence;
    float _lastSequenceTime;

    void Start()
    {
        if (!Application.isEditor)
            debug = false;

        if (!ender)
            ender = FindObjectOfType<TutorialEnder>();

        tutSequence = StartCoroutine(TutorialSequence());

        groupCard.gameObject.SetActive(false);
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

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                // wind tunnel 1
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    God.wren.PhaseShift(new Vector3(-5012,183,-544));
                }
                // wind tunnel 2
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    God.wren.PhaseShift(new Vector3(-3859,287,-1337));
                }
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                ShowCard("Wings", "Use the left stick to steer and the right stick to control your wings.");
            }
        }

        fade.transform.position = God.camera.transform.position;


    }
    
    IEnumerator WaitWithCheat(float seconds)
    {
        float t = 0;
        while(t < seconds)
        {
            if (Input.GetKey(KeyCode.Space))
                break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
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

        yield return WaitWithCheat(2);

        while (God.wren.physics.onGround)
            yield return null;

        cinematicCamera.armed = true;

        while(debug)
        {
            cinematicCamera.tutorialCameraIdx = debugCamIdx;
            yield return null;
        }

        cinematicCamera.tutorialCameraIdx = 0;
        yield return WaitWithCheat(5);

        // Sticks
        {
            groupSticks.SetActive(true);
            controllerText.text = "";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
        }
        yield return WaitWithCheat(5);
        
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

        float cT = 0;
        float _prevIdx = cinematicCamera.tutorialCameraIdx;
        while(cT < 1)
        {
            cinematicCamera.tutorialCameraIdx = Mathf.Lerp(_prevIdx, _prevIdx + 2, Mathf.SmoothStep(0,1,cT));
            cT += Time.unscaledDeltaTime * .1f;
            yield return null;
        }
        // cinematicCamera.tutorialCameraIdx++; // 5
        
        yield return WaitWithCheat(0.2f);
        cinematicCamera.armed = false;
        yield return WaitWithCheat(5f);
        
        float bgT = 1;
        while(bgT > 0)
        {
            SetBGFade(bgT);
            bgT -= Time.unscaledDeltaTime * .1f;
            yield return null;
        }
        SetBGFade(0);
        yield return WaitWithCheat(6);
        // yield return CameraSequence();

        // Game cam
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
                    diveT += Time.unscaledDeltaTime * .45f;
                else
                    diveT = Mathf.Clamp01(diveT - Time.unscaledDeltaTime * 1.25f);

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

        yield return WaitWithCheat(6);

        ShowContinue(true);
        
        yield return WaitWithCheat(0.5f);

        bool lastX = God.input.x;
        wait = true;
        while(wait)
        {
            if (!lastX && God.input.x && Time.time - _lastSequenceTime > 3)
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
        float duration = 1.5f;
        float _ct = Time.unscaledTime;
        while (t < duration)
        {
            if (delay > 0 && Time.unscaledTime - _ct < delay)
            {
                yield return null;
                continue;
            }
            group.alpha = Mathf.Lerp(from, to, t);
            t += Time.unscaledDeltaTime;
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


    // Cards

    public void ShowCard(string title, string text)
    {
        groupCard.gameObject.SetActive(true);
        groupXToContinue.gameObject.SetActive(true);
        cardTitle.text = title;
        cardText.text = text;

        StartCoroutine(CardSequence());
    }

    IEnumerator CardSequence()
    {
        bool wait = true;
        groupCard.alpha = 0;
        groupXToContinue.alpha = 0;

        StartCoroutine(FadeGroup(groupCard, 0, 1));

        float t = 1;
        var prevTimescale = Time.timeScale;
        while(t > 0)
        {
            t -= Time.unscaledDeltaTime * 1.2f;
            Time.timeScale = Mathf.Lerp(.1f,1f, t);
            
            yield return null;
        }

        yield return WaitWithCheat(3);

        groupXToContinue.alpha = 1;

        bool lastX = God.input.x;
        wait = true;
        while(wait)
        {
            if (!lastX && God.input.x)
                wait = false;
            lastX = God.input.x;
            yield return null;
        }

        t = 0;
        while(t < 1)
        {
            t += Time.unscaledDeltaTime * .5f;
            groupCard.alpha = 1 - t;
            Time.timeScale = Mathf.Lerp(0.1f, 1, Mathf.Clamp01(t));
            
            yield return null;
        }

        Time.timeScale = 1;
        groupCard.alpha = 0;
        groupXToContinue.alpha = 0;
    }
}
