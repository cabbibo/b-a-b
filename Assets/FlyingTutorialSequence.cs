using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

public class FlyingTutorialSequence : MonoBehaviour
{
    public static UnityAction OnTutorialStart;
    public static UnityAction OnTutorialDiveFinished;
    

    public CinematicCameraHandler cinematicCamera;

    public CanvasGroup groupContainer;
    public CanvasGroup xToContinue;

    public TextMeshProUGUI controllerText;

    public RectTransform progressBar;

    public bool debug;
    public int debugCamIdx = 0;

    public Gradient fadeGradient;
    public Renderer fade;

    public TutorialEnder ender;

    [Header("Controller")]
    public GameObject groupSticks;
    public GameObject groupDive;
    public GameObject groupLeft;
    public GameObject groupRight;
    public GameObject groupUp;
    public GameObject groupDown;

    enum ControllerHint { None, Dive, Left, Right, Up, Down }

    [Header("Tooltip Cards")]
    public CanvasGroup groupCard;
    public CanvasGroup groupXToContinue;
    public TextMeshProUGUI cardTitle;
    public TextMeshProUGUI cardText;


    Coroutine tutSequence;
    float _lastSequenceTime;

    static FlyingTutorialSequence _instance;
    public static FlyingTutorialSequence Instance { 
        get {
            if (!_instance)
                _instance = FindObjectOfType<FlyingTutorialSequence>();
            return _instance;
        }
    }

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
                TryShowCard(CardType.ActivityRings);
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                _cardShown = new Dictionary<CardType, bool>();
            }
        }

        fade.transform.position = God.camera.transform.position;


    }

    void SetControllerHint(ControllerHint hint)
    {
        groupLeft.SetActive(hint == ControllerHint.Left);
        groupRight.SetActive(hint == ControllerHint.Right);
        groupUp.SetActive(hint == ControllerHint.Up);
        groupDown.SetActive(hint == ControllerHint.Down);
        groupDive.SetActive(hint == ControllerHint.Dive);

        controllerText.transform.parent.gameObject.SetActive(hint != ControllerHint.None);
        switch(hint)
        {
            case ControllerHint.Dive:
                controllerText.text = "Hold to DIVE";
                break;
            case ControllerHint.Left:
                controllerText.text = "Both sticks to TURN LEFT";
                break;
            case ControllerHint.Right:
                controllerText.text = "Both sticks to TURN RIGHT";
                break;
            case ControllerHint.Up:
                controllerText.text = "Both sticks up to FLY DOWN";
                break;
            case ControllerHint.Down:
                controllerText.text = "Both sticks down to FLY UP";
                break;
        }
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

    IEnumerator LerpCamera(float from, float to)
    {
        float cT = 0;
        while(cT < 1)
        {
            cinematicCamera.tutorialCameraIdx = Mathf.Lerp(from, to, Mathf.SmoothStep(0,1,cT));
            cT += Time.unscaledDeltaTime * .1f;
            yield return null;
        }
    }

    enum Camera { 
        Closeup = 0, 
        TopClose = 1, 
        TopFar = 2, 
        Front = 3, 
        Behind = 4, 
        Play = 5 
    }

    IEnumerator TutorialSequence()
    {
        yield return null;

        OnTutorialStart?.Invoke();

        God.fade.FadeIn(3);

        groupContainer.alpha = 0;
        ShowContinue(false);
        ShowText();

        SetControllerHint(ControllerHint.None);
        ShowProgress(0);
        cinematicCamera.armed = false;
        
        float bgT = 1f;
        SetBGFade(bgT);

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

        cinematicCamera.tutorialCameraIdx = (float)Camera.Closeup;

        yield return WaitWithCheat(5);

        // Sticks
        {
            groupSticks.SetActive(true);
            controllerText.text = "";

            yield return FadeGroup(groupContainer, 0, 1);
        }
        
        yield return WaitWithCheat(5f);
        yield return WaitForXToContinue();

        groupSticks.SetActive(false);
        cinematicCamera.tutorialCameraIdx = (float)Camera.TopClose;

        yield return WaitWithCheat(5f);
        yield return WaitForXToContinue();

        yield return LerpCamera((float)Camera.TopClose, (float)Camera.TopFar); 
        
        yield return WaitWithCheat(5f);
        yield return WaitForXToContinue();
        
        cinematicCamera.tutorialCameraIdx = (float)Camera.Front;
        yield return WaitWithCheat(.3f);
        yield return LerpCamera((float)Camera.Front, (float)Camera.Play);
        
        yield return WaitWithCheat(1f);
        
        cinematicCamera.armed = false;

        yield return WaitWithCheat(0.5f);
        
        while(bgT > 0)
        {
            SetBGFade(bgT);
            bgT -= Time.unscaledDeltaTime * .1f;
            yield return null;
        }
        SetBGFade(0);

        StartCoroutine(FadeGroup(groupContainer, 1, 0));

        // Left
        yield return ControllerHintSequence(ControllerHint.Left);
        yield return WaitWithCheat(0.5f);

        // Right
        yield return ControllerHintSequence(ControllerHint.Right);
        yield return WaitWithCheat(0.5f);

        // Up
        yield return ControllerHintSequence(ControllerHint.Up);
        yield return WaitWithCheat(0.5f);

        // Down
        yield return ControllerHintSequence(ControllerHint.Down);   
        yield return WaitWithCheat(0.5f);

        // Space to fly
        yield return WaitWithCheat(7);

        // Dive
        yield return ControllerHintSequence(ControllerHint.Dive);   
       
        ShowProgress(0);
        StartCoroutine(FadeGroup(groupContainer, 1, 0));

        OnTutorialFinished();
    }

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
        switch(hint)
        {
            case ControllerHint.Left:
                return God.input.leftX < -.5f && God.input.rightX < -.5f;
            case ControllerHint.Right:
                return God.input.leftX > .5f && God.input.rightX > .5f;
            case ControllerHint.Up:
                return God.input.leftY > .5f && God.input.rightY > .5f;
            case ControllerHint.Down:
                return God.input.leftY < -.5f && God.input.rightY < -.5f;
            case ControllerHint.Dive:
                return God.input.l2 > .5f && God.input.r2 > .5f;
        }
        return false;
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

    IEnumerator WaitForXToContinue()
    {
        bool wait = true;
        var t = 0f;
        groupContainer.alpha = 1;
        ShowProgress(t);

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
        // yield return FadeGroup(groupContainer, 0, 1));
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
        float duration = 0.7f;
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

        // TryShowCard(CardType.RevealIsland, true, 1);
    }


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
        ActivityRings, ActivityWindTunnel, ActivitySpeedGate, ActivityButterflies, ActivityBigBird
    }

    private Dictionary<CardType, bool> _cardShown = new Dictionary<CardType, bool>();

    public void GetCardInfo(CardType type, out string title, out string text)
    {
        title = "";
        text = "";
        switch(type)
        {
            case CardType.RevealIsland:
                title = "Bird Island";
                text = "Welcome to Bird Island. Fly around freely and explore.\nWhen you're ready, go to the gate at the top of the mountain";
                break;
            
            case CardType.FlyCloseToGround:
                title = "Close to Ground";
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
                text = "Follow the giant bird. Get close to hear its rumble.";
                break;
        }
    }
    public void TryShowCard(CardType cardType, float delay = 0, Transform target = null, bool pause = false)
    {
        if (_cardShown == null)
            _cardShown = new Dictionary<CardType, bool>();
            
        if (_cardShown.ContainsKey(cardType) && _cardShown[cardType])
            return;

        string title, text;
        GetCardInfo(cardType, out title, out text);

        cardTitle.text = title;
        cardText.text = text;

        _cardShown[cardType] = true;

        StartCoroutine(ShowCardSequence(delay, target, pause));
    }

    IEnumerator ShowCardSequence(float delay = 0, Transform target = null, bool pause = false)
    {
        const float TIMESCALE_LOW = 0.001f;

        bool wait = true;
        groupCard.alpha = 0;
        groupXToContinue.alpha = 0;

        groupCard.gameObject.SetActive(true);
        groupXToContinue.gameObject.SetActive(true);

        if (target)
            God.wren.cameraWork.objectTargeted = target;
        Debug.Log(target + ", " + God.wren.cameraWork.objectTargeted);
        if (delay > 0)
            yield return WaitWithCheat(delay);

        StartCoroutine(FadeGroup(groupCard, 0, 1));

        float t = 1;
        var prevTimescale = Time.timeScale;
        while(pause && t > 0)
        {
            t -= Time.unscaledDeltaTime * 1.2f;
            Time.timeScale = Mathf.Lerp(TIMESCALE_LOW,prevTimescale, t);
            
            yield return null;
        }

        groupXToContinue.alpha = 1;

        yield return WaitWithCheat(0.4f);

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
        while(pause && t < 1)
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
    }
}
