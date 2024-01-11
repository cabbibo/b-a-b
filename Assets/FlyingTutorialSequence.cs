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
    
    public CanvasGroup groupContainer;

    public TextMeshProUGUI controllerText;

    public GameObject groupSticks;
    public GameObject groupDive;
    public RectTransform progressBar;


    void Start()
    {
        StartCoroutine(TutorialSequence());

        
    }


    void Update()
    {
        
        
    }

    IEnumerator TutorialSequence()
    {
        yield return null;

        groupContainer.alpha = 0;

        groupSticks.SetActive(false);
        groupDive.SetActive(false);
        ShowProgress(0);

        while(God.wren == null)
            yield return null;

        yield return new WaitForSecondsRealtime(2);

        // Sticks
        groupSticks.SetActive(true);
        controllerText.text = "Wings";
        
        yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
        yield return new WaitForSecondsRealtime(8);
        yield return StartCoroutine(FadeGroup(groupContainer, 1, 0));

        groupSticks.SetActive(false);
        groupDive.SetActive(true);
        controllerText.text = "Hold to DIVE";

        yield return StartCoroutine(FadeGroup(groupContainer, 0, 1));
        yield return new WaitForSecondsRealtime(1);

        // TODO: wait for diving enough time

        float diveT = 0;
        while(diveT < 1)
        {
            if (God.input.l2 > .5f && God.input.r2 > .5f)
                diveT += Time.deltaTime * .5f;
            ShowProgress(diveT);
            yield return null;
        }
        ShowProgress(0);


        OnTutorialFinished();

        yield return StartCoroutine(FadeGroup(groupContainer, 1, 0));


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
        while(t < duration)
        {
            group.alpha = Mathf.Lerp(from, to, t);
            t += Time.deltaTime;
            yield return null;
        }
        group.alpha = to;
    }

    void OnTutorialFinished()
    {
        OnTutorialDiveFinished.Invoke();
    }
}
