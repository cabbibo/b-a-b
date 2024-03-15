using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using TMPro;
using UnityEngine.UIElements;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

public class ChooseControlsSequence : MonoBehaviour
{
    public Canvas canvas;

    public CanvasGroup canvasTopHalf;

    public GameObject windowContainer;
    public GameObject testingContainer;
    public GameObject playingContainer;

    public GameObject backButton;
    public GameObject testButton;

    public GameObject birdsContainer;

    public GameObject stepStartParent;
    public GameObject stepMoveParent;
    public GameObject stepDoneParent;
    public GameObject stepChooseParent;

    public GameObject buttonContinue;

    public TimelineAsset controllerAnimVertical;
    public TimelineAsset controllerAnimTurn;
    public PlayableDirector controllerDirector;
    public TextMeshProUGUI moveTextHeader;
    public TextMeshProUGUI confirmTextHeader;
    public CanvasGroup selectionRight;
    public CanvasGroup selectionLeft;

    public Transform progressBar;

    public GameObject birdLeftParent;
    public GameObject birdRightParent;
    BirdPreview birdLeft;
    BirdPreview birdRight;
    class BirdPreview
    {
        public ParticleSystem particleSystem;
        public Transform birdParent;

        public BirdPreview(GameObject parent)
        {
            particleSystem = parent.GetComponentInChildren<ParticleSystem>();
            birdParent = parent.transform.Find("Model");
        }

        float GetTurnAngle(float y1, float y2)
        {
            if (Mathf.Approximately(y1, 0f) && Mathf.Approximately(y2, 0f))
                return 0f;
            y1 = Mathf.Clamp(y1, -1, 1);
            y2 = Mathf.Clamp(y2, -1, 1);
            return (Mathf.Atan2(y2, 1) - Mathf.Atan2(y1, 1)) / (Mathf.PI / 2f);
        }

        public void SetMovement(int axisX, int axisY)
        {
            var x = GetTurnAngle(God.input.left.y, God.input.right.y);
            var y = Mathf.Clamp(God.input.left.y + God.input.right.y, -1, 1);

            var vl = particleSystem.velocityOverLifetime;
            //vl.x = x * 2 * speed * axisX;
            vl.y = y * 30.0f * axisY;

            particleSystem.transform.Rotate(Vector3.up * x * 80.0f * axisX * Time.deltaTime, Space.Self);

            birdParent.localEulerAngles = new Vector3(
                20 + Mathf.LerpAngle(0, 20, Mathf.Abs(y)) * Mathf.Sign(y) * axisY,
                0,
                Mathf.LerpAngle(0, 20, Mathf.Abs(x)) * Mathf.Sign(x) * axisX
            );
            birdParent.Rotate(Vector3.up * x * -10 * axisX, Space.World);
        }
    }

    bool userChoiceSwapX;
    bool userChoiceSwapY;


    enum StepType { None, Start, MoveVertical, MoveHorizontal, ChooseVertical, ChooseHorizontal, ConfirmHorizontal, ConfirmVertical, Finished }
    int _stepI = 0;
    StepType[] steps = new StepType[] {
        StepType.Start,
        //StepType.MoveVertical,
        StepType.ChooseVertical,
        StepType.ConfirmVertical,
        //StepType.MoveHorizontal,
        StepType.ChooseHorizontal,
        StepType.ConfirmHorizontal,
        StepType.Finished
    };

    float _chooseTLeft;
    float _chooseTRight;

    bool ShowingWindow { get { return windowContainer.activeSelf; } set { windowContainer.SetActive(value); }}
    bool ShowingTesting { get { return testingContainer.activeSelf; } set { 
        if (value)
        {
            God.wren.PhaseShift(new Vector3(-41, 50, 42));
            God.wren.state.TakeOff();
            God.wren.physics.transform.forward = Vector3.forward;
            God.wren.physics.vel = God.wren.physics.transform.forward * 20;
            
            UpdateBirdParams();
        }
        testingContainer.SetActive(value); ShowingWindow = !value; 
    }}

    void OnEnable()
    {
        canvas.worldCamera = Camera.main;

        birdLeft = new BirdPreview(birdLeftParent);
        birdRight = new BirdPreview(birdRightParent);
    }

    void Start()
    {
        StartCoroutine(SetupBird());

        ShowScreen(false);        
    }

    void UpdateBirdParams()
    {
        God.wren.physics.swapLR = userChoiceSwapX;
        God.wren.physics.invert = !userChoiceSwapY;
    }

    void ShowScreen(bool bShow)
    {
        // canvas.enabled = bShow;
        ShowingWindow = bShow;
        if (bShow)
        {
            _stepI = 0;
            SetStep(steps[_stepI]);
        }
        playingContainer.SetActive(!bShow);
    }

    private void Update()
    {
        if (!ShowingWindow && God.input.circlePressed)
        {
            ShowScreen(true);
        }

        if (ShowingTesting && God.input.trianglePressed)
        {
            ShowingTesting = false;
            return;
        }

        if (!ShowingWindow)
            return;
        
        if (_stepI > 0 && God.input.squarePressed)
        {
            _stepI--;
            SetStep(steps[_stepI]);
        }

        int result;
        switch(steps[_stepI])
        {
            case StepType.Start:

                if (God.input.xPressed)
                    NextStep();

                break;
            case StepType.ChooseHorizontal:

                birdLeft.SetMovement(-1, 0);
                birdRight.SetMovement(1,0);

                if (HandleChoice(out result))
                {
                    userChoiceSwapX = result == 1;
                    NextStep();
                }
                break;
            case StepType.ChooseVertical:

                birdLeft.SetMovement(0, -1);
                birdRight.SetMovement(0, 1);

                if (HandleChoice(out result))
                {
                    userChoiceSwapY = result == 1;

                    NextStep();
                }

                break;
            // case StepType.MoveHorizontal:

            //     HandleLeftRightBirdMovement(true, false);

            //     if (God.input.xPressed)
            //         NextStep();

            //     break;
            // case StepType.MoveVertical:

            //     HandleLeftRightBirdMovement(false, true);

            //     if (God.input.xPressed)
            //         NextStep();

            //     break;

            case StepType.ConfirmHorizontal:
                birdLeft.SetMovement(userChoiceSwapX ? 1 : -1, 0);
                birdRight.SetMovement(userChoiceSwapX ? 1 : -1, 0);

                if (God.input.xPressed)
                    NextStep();

                if (God.input.trianglePressed)
                    ShowingTesting = true;

                break;
            case StepType.ConfirmVertical:
                birdLeft.SetMovement(0, userChoiceSwapY ? 1 : -1);
                birdRight.SetMovement(0, userChoiceSwapY ? 1 : -1);

                if (God.input.xPressed)
                    NextStep();
                
                if (God.input.trianglePressed)
                    ShowingTesting = true;

                break;

            case StepType.Finished:
                birdLeft.SetMovement(userChoiceSwapX ? 1 : -1, userChoiceSwapY ? 1 : -1);
                birdRight.SetMovement(userChoiceSwapX ? 1 : -1, userChoiceSwapY ? 1 : -1);

                if (God.input.xPressed)
                    NextStep();
                
                if (God.input.trianglePressed)
                    ShowingTesting = true;

                    break;
        }

        
    }

    void HandleLeftRightBirdMovement(bool horizontal, bool vertical)
    {
        birdLeft.SetMovement(horizontal ? -1 : 0, vertical ? 1 : 0);
        birdRight.SetMovement(horizontal ? 1 : 0, vertical ? -1 : 0);

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

        UpdateBirdParams();

    }

    void SetStep(StepType stepType)
    {
        
        stepStartParent.SetActive(false);
        stepMoveParent.SetActive(false);
        stepDoneParent.SetActive(false);
        stepChooseParent.SetActive(false);
        birdsContainer.SetActive(false);

        buttonContinue.SetActive(false);
        backButton.SetActive(stepType != StepType.Start && stepType != StepType.Finished);
        testButton.SetActive(false);

        controllerDirector.gameObject.SetActive(false);

        _chooseTLeft = _chooseTRight = 0;
        UpdateHoldGraphic();

        // Controller Animation
        switch (stepType)
        {

            case StepType.ChooseVertical:
            case StepType.MoveVertical:
            case StepType.ConfirmVertical:
                controllerDirector.gameObject.SetActive(true);
                controllerDirector.playableAsset = controllerAnimVertical;
                controllerDirector.Play();
                break;
            case StepType.ChooseHorizontal:
            case StepType.MoveHorizontal:
            case StepType.ConfirmHorizontal:
                controllerDirector.gameObject.SetActive(true);
                controllerDirector.playableAsset = controllerAnimTurn;
                controllerDirector.Play();
                break;
        }

        // Step Parent
        switch (stepType)
        {
            case StepType.Start:
                stepStartParent.SetActive(true);
                //buttonContinue.SetActive(true);
                break;
            case StepType.ChooseHorizontal:
            case StepType.ChooseVertical:
                stepChooseParent.SetActive(true);
                birdsContainer.SetActive(true);
                break;
            case StepType.MoveVertical:
            case StepType.MoveHorizontal:
                stepMoveParent.SetActive(true);
                birdsContainer.SetActive(true);
                buttonContinue.SetActive(true);
                break;
            case StepType.Finished:
            case StepType.ConfirmHorizontal:
            case StepType.ConfirmVertical:
                stepDoneParent.SetActive(true);
                testButton.SetActive(stepType != StepType.Finished);
                break;
        }

        // Text
        switch (stepType)
        {
            case StepType.MoveVertical:
                moveTextHeader.text = "Move vertically";
                break;
            case StepType.MoveHorizontal:
                moveTextHeader.text = "Move horizontally";
                break;
            case StepType.ConfirmHorizontal:
            case StepType.ConfirmVertical:
                confirmTextHeader.text = "Does this feel good?";
                break;
            case StepType.Finished:
                confirmTextHeader.text = "You're done!";
                break;
        }
    }

    void NextStep()
    {
        if (_stepI == steps.Length - 1)
        {
            ShowScreen(false);
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
