using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

using WrenUtils;
public class ReachLocationTutorialState2 : TutorialState
{
    public UnityEvent OnLocationReached;
    public Tutorial tutorial;


    public float pulseSpeed = 1;
    public float timeBetweenPulses = 10;

    public TextMeshProUGUI textMesh;



    public bool hasFired;
    public override void OnStart()
    {


        textMesh.enabled = true;
        print("We have started here!");
        gameObject.SetActive(true);
        God.audio.Play(God.sounds.tutorialSectionStartSound);
        OnStartEvent.Invoke();
        hasFired = false;


    }
    public override void OnComplete()
    {


        textMesh.enabled = false;
        print("COMPLETING GETING TO LOCATION");
        God.audio.Play(God.sounds.tutorialSuccessSound);
        OnCompleteEvent.Invoke();
        StartCoroutine(TurnOff(6));
    }


    public void Update()
    {

        print("updating this tutorial state");

        float value = Mathf.Sin(Time.time * timeBetweenPulses);
        value = Mathf.Clamp(value, .5f, 1);
        value -= .5f;
        value *= 2;

        textMesh.color = new Color(1, 1, 1, value);


    }

    IEnumerator TurnOff(float time)
    {
        yield return new WaitForSeconds(time);
        Finish();
    }

    public void Finish()
    {

        print("I have finished");
        //gameObject.SetActive(false);
    }

    public void OnTriggerEnter(Collider c)
    {

        print(c.attachedRigidbody);
        print("Tutorial Reach Location Trigger Enter");

        if (c.attachedRigidbody == God.wren.physics.rb && hasFired == false)
        {
            print("Tutorial Reach Location Trigger Enter has correct body");
            OnLocationReached.Invoke();
            God.audio.Play(God.sounds.tutorialSuccessSound);
            hasFired = true;

            tutorial.ReachLocationStateHit(this);
            OnComplete();
        }

    }

    public void OnCutSceneFinished()
    {

    }




}
