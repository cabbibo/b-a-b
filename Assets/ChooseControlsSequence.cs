using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.UI;
using TMPro;

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


    int _stepI = 0;
    StepType[] steps = new StepType[] {
        StepType.Start, StepType.MoveVertical, StepType.ChooseVertical,
        StepType.MoveHorizontal, StepType.ChooseHorizontal, StepType.Finished
    };

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
        switch(steps[_stepI])
        {
            case StepType.Start:
                
                break;
            case StepType.ChooseHorizontal:
                
                break;
            case StepType.ChooseVertical:
                

                break;
            case StepType.MoveVertical:
                
                break;
            case StepType.MoveHorizontal:
                

                break;
            case StepType.Finished:
                
                break;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            NextStep();
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

        switch (stepType)
        {
            case StepType.Start:
                stepStartParent.SetActive(true);

                buttonContinue.SetActive(true);
                break;
            case StepType.ChooseHorizontal:
                stepChooseParent.SetActive(true);
                stepChooseParent.GetComponentInChildren<TextMeshProUGUI>().text = "Which bird are you?";

                birdsContainer.SetActive(true);
                break;
            case StepType.ChooseVertical:
                stepChooseParent.SetActive(true);
                stepChooseParent.GetComponentInChildren<TextMeshProUGUI>().text = "Which bird are you?";

                birdsContainer.SetActive(true);
                break;
            case StepType.MoveVertical:
                stepMoveParent.SetActive(true);
                stepMoveParent.GetComponentInChildren<TextMeshProUGUI>().text = "Move vertically";
                controllerVertical.SetActive(true);
                birdsContainer.SetActive(true);
                break;
            case StepType.MoveHorizontal:
                stepMoveParent.SetActive(true);
                stepMoveParent.GetComponentInChildren<TextMeshProUGUI>().text = "Move horizontally";
                controllerHorizontal.SetActive(true);
                birdsContainer.SetActive(true);
                break;
            case StepType.Finished:
                stepDoneParent.SetActive(true);

                buttonContinue.SetActive(true);
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




    // IEnumerator HoldInputToContinue()
    // {
    //     bool wait = true;
    //     var t = 0f;
    //     groupContainer.alpha = 1;
    //     ShowProgress(t);

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
