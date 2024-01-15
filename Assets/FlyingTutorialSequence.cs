using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

public class FlyingTutorialSequence : MonoBehaviour
{
    public static UnityAction OnTutorialDiveFinished;
    // public static bool 

    public CinematicCameraHandler cinematicCamera;

    public CanvasGroup groupContainer;

    public TextMeshProUGUI controllerText;

    public GameObject groupSticks;
    public GameObject groupDive;
    public RectTransform progressBar;

    public bool debug;
    public int debugCamIdx = 0;

    Coroutine tutSequence;


    void Start()
    {
        if (!Application.isEditor)
            debug = false;

        
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
            }
        }

    }

    IEnumerator TutorialSequence()
    {
        yield return null;

        groupContainer.alpha = 0;

        groupSticks.SetActive(false);
        groupDive.SetActive(false);
        ShowProgress(0);
        cinematicCamera.armed = false;

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

        cinematicCamera.tutorialCameraIdx = 3; // below
        yield return new WaitForSecondsRealtime(5);

        // Sticks
        {
            groupSticks.SetActive(true);
            controllerText.text = "Wings";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
            yield return new WaitForSecondsRealtime(12);
            yield return StartCoroutine(FadeGroup(groupContainer, 1, 0));
        }
        groupSticks.SetActive(false);

        // Cam 1
        {
            controllerText.text = "X Continue";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
            while (God.wren.input.ex < .5f)
                yield return null;
            groupContainer.alpha = 0;
        }

        cinematicCamera.tutorialCameraIdx = 0; //eye
        yield return new WaitForSecondsRealtime(9);
        // Cam 1
        {
            controllerText.text = "X Continue";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
            while (God.wren.input.ex < .5f)
                yield return null;
            groupContainer.alpha = 0;
        }
        cinematicCamera.tutorialCameraIdx = 1;
        yield return new WaitForSecondsRealtime(6);
        // Cam 1
        {
            controllerText.text = "X Continue";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
            while (God.wren.input.ex < .5f)
                yield return null;
            groupContainer.alpha = 0;
        }

        cinematicCamera.tutorialCameraIdx = 2;
        yield return new WaitForSecondsRealtime(6);
        // Cam 1
        {
            controllerText.text = "X Continue";

            yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
            while (God.wren.input.ex < .5f)
                yield return null;
            groupContainer.alpha = 0;
        }

        cinematicCamera.armed = false;
        yield return StartCoroutine(FadeGroup(groupContainer, 1, 0));


        // // Sticks
        // {
        //     groupSticks.SetActive(true);
        //     controllerText.text = "Wings";

        //     yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
        //     yield return new WaitForSecondsRealtime(8);
        //     yield return StartCoroutine(FadeGroup(groupContainer, 1, 0));
        // }

        yield return new WaitForSecondsRealtime(8);

        // Dive
        {
            groupSticks.SetActive(false);
            groupDive.SetActive(true);
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

    void ShowProgress(float t = 0)
    {
        progressBar.gameObject.SetActive(t > 0);
        progressBar.localScale = new Vector3(Mathf.Clamp01(t), 1, 1);
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
        groupContainer.alpha = 0;
        
        OnTutorialDiveFinished?.Invoke();
    }
}
