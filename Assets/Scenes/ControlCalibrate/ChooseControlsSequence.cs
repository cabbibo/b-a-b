using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class ChooseControlsSequence : MonoBehaviour
{
    public Canvas canvas;

    public CanvasGroup canvasTopHalf;

    public GameObject birdsContainer;

    public GameObject stepStartParent;
    public GameObject stepMoveParent;
    public GameObject stepDoneParent;
    public GameObject stepChooseParent;

    public GameObject buttonContinue;

    public GameObject controllerVertical;
    public GameObject controllerHorizontal;
    public TextMeshProUGUI moveTextHeader;
    public CanvasGroup selectionRight;
    public CanvasGroup selectionLeft;

    public Transform progressBar;

    public ParticleSystem birdLeftPS;
    public ParticleSystem birdRightPS;

    bool userChoiceSwapX;
    bool userChoiceSwapY;


    int _stepI = 0;
    StepType[] steps = new StepType[] {
        StepType.Start, StepType.MoveVertical, StepType.ChooseVertical,
        StepType.MoveHorizontal, StepType.ChooseHorizontal, StepType.Finished
    };

    float _chooseTLeft;
    float _chooseTRight;

    void OnEnable()
    {
        canvas.worldCamera = Camera.main;
    }

    void Start()
    {
        StartCoroutine(SetupBird());
        
        SetStep(steps[_stepI]);
    }

    private void Update()
    {
        int result;
        switch(steps[_stepI])
        {
            case StepType.Start:

                if (God.input.xPressed)
                    NextStep();

                break;
            case StepType.ChooseHorizontal:
                HandleLeftRightBirdMovement(1, 0);
                if (HandleChoice(out result))
                {
                    Debug.Log("Chose " + result);
                    userChoiceSwapX = result == 1;
                    NextStep();
                }
                break;
            case StepType.ChooseVertical:
                HandleLeftRightBirdMovement(0, 1);
                if (HandleChoice(out result))
                {
                    Debug.Log("Chose " + result);
                    userChoiceSwapY = result == 1;

                    NextStep();
                }

                break;
            case StepType.MoveHorizontal:

                HandleLeftRightBirdMovement(1, 0);

                if (God.input.xPressed)
                    NextStep();

                break;
            case StepType.MoveVertical:

                HandleLeftRightBirdMovement(0, 1);

                if (God.input.xPressed)
                    NextStep();

                break;

            case StepType.Finished:

                HandleFinalBirdMovement();

                if (God.input.xPressed)
                    NextStep();

                if (God.input.squarePressed)
                {
                    _stepI = 0;
                    SetStep(steps[_stepI]);
                }
                    break;
        }

        
    }

    float ControlX {  get { return God.input.left.x + God.input.right.x; } }
    float ControlY {  get { return God.input.left.y + God.input.right.y; } }

    void HandleLeftRightBirdMovement(int horizontalMovement, int verticalMovement)
    {
        var speed = 20.0f;

        var vl = birdLeftPS.velocityOverLifetime;
        var vr = birdRightPS.velocityOverLifetime;
        vl.x = speed * (float)horizontalMovement * ControlX;
        vr.x = speed * (float)horizontalMovement * -ControlX;
        vl.y = speed * (float)verticalMovement * ControlY;
        vr.y = speed * (float)verticalMovement * -ControlY;

    }
    void HandleFinalBirdMovement()
    {
        var speed = 20.0f;

        var vl = birdLeftPS.velocityOverLifetime;
        vl.x = speed * ControlX * (userChoiceSwapX ? 1 : -1);
        vl.y = speed * ControlY * (userChoiceSwapY ? 1 : -1);

    }

    IEnumerator SetupBird()
    {
        while (God.wren == null)
        {
            yield return null;
        }
        var wren = God.wren;

        var b = wren.bird;
        b.drawBodyFeather = b.drawLeftWingFeathers = b.drawRightWingFeathers = false;
        b.drawBodyFeatherPoints = b.drawLeftWingFeatherPoints = b.drawRightWingFeatherPoints = true;


        yield return new WaitForSeconds(.5f);

        wren.state.TakeOff();



    }

    enum StepType { None, Start, MoveVertical, MoveHorizontal, ChooseVertical, ChooseHorizontal, Finished }
    void SetStep(StepType stepType)
    {
        stepStartParent.SetActive(false);
        stepMoveParent.SetActive(false);
        stepDoneParent.SetActive(false);
        stepChooseParent.SetActive(false);
        birdsContainer.SetActive(false);

        buttonContinue.SetActive(false);

        controllerHorizontal.SetActive(false);
        controllerVertical.SetActive(false);

        _chooseTLeft = _chooseTRight = 0;
        UpdateHoldGraphic();

        switch (stepType)
        {
            case StepType.Start:
                stepStartParent.SetActive(true);

                buttonContinue.SetActive(true);
                break;
            case StepType.ChooseHorizontal:
                stepChooseParent.SetActive(true);

                birdsContainer.SetActive(true);
                break;
            case StepType.ChooseVertical:
                stepChooseParent.SetActive(true);

                birdsContainer.SetActive(true);
                break;
            case StepType.MoveVertical:
                stepMoveParent.SetActive(true);
                moveTextHeader.text = "Move vertically";
                controllerVertical.SetActive(true);
                birdsContainer.SetActive(true);

                buttonContinue.SetActive(true);
                break;
            case StepType.MoveHorizontal:
                stepMoveParent.SetActive(true);
                moveTextHeader.text = "Move horizontally";
                controllerHorizontal.SetActive(true);
                birdsContainer.SetActive(true);

                buttonContinue.SetActive(true);
                break;
            case StepType.Finished:
                stepDoneParent.SetActive(true);

                break;
            default:
                
                break;
        }
    }

    void NextStep()
    {
        if (_stepI == steps.Length - 1)
        {
            return;
        }
        _stepI++;
        SetStep(steps[_stepI]);
    }

    bool HandleChoice(out int result, float speed = 10f, bool gravity = true)
    {
        bool left = God.input.l2 > .01f || God.input.l1 > .01f;
        bool right = God.input.r2 > .01f || God.input.r1 > .01f;
        if (_chooseTRight > 0 && left) _chooseTRight = 0;
        if (_chooseTLeft > 0 && right) _chooseTLeft = 0;
        if (right)
            _chooseTRight += Time.unscaledDeltaTime * .1f * speed;
        else if (left)
            _chooseTLeft += Time.unscaledDeltaTime * .1f * speed;
        else if (gravity)
        {
            _chooseTLeft = Mathf.Clamp01(_chooseTLeft - Time.unscaledDeltaTime * 2.5f);
            _chooseTRight = Mathf.Clamp01(_chooseTRight - Time.unscaledDeltaTime * 2.5f);
        }

        UpdateHoldGraphic();

        result = _chooseTLeft > 0 ? -1 : (_chooseTRight > 0 ? 1 : 0);

        var t = Mathf.Max(_chooseTLeft, _chooseTRight);
        return t >= 1;
    }
    void UpdateHoldGraphic()
    {
        var t = Mathf.Max(_chooseTLeft, _chooseTRight);
        progressBar.transform.parent.gameObject.SetActive(t > 0);
        progressBar.localScale = new Vector3(Mathf.Clamp01(t * t), 1, 1);

        selectionLeft.alpha = _chooseTLeft;
        selectionRight.alpha = _chooseTRight;
    }




    // IEnumerator HoldInputToContinue()
    // {
    //     bool wait = true;
    //     var t = 0f;
    //     groupContainer.alpha = 1;
    //     UpdateHoldGraphic(t);

    //     ShowContinue(true);

    //     yield return WaitWithCheat(0.5f);

    //     bool lastX = God.input.x;
    //     wait = true;
    //     while (wait)
    //     {
    //         if (!lastX && God.input.x && Time.time - _lastSequenceTime > 3)
    //             wait = false;
    //         if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
    //             wait = false;
    //         lastX = God.input.x;
    //         // if (God.input.x)
    //         // wait = false;
    //         // else
    //         yield return null;
    //     }
    //     _lastSequenceTime = Time.unscaledTime;
    //     // yield return FadeGroup(groupContainer, 0, 1));
    //     // while (HandleSticksProgress(ref t, speed: 1.7f, gravity: true))
    //     //     yield return null;

    //     groupContainer.alpha = 0;
    //     ShowContinue(false);
    // }







}
