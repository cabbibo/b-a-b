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

    public float timeTilPulseStart = 10;

    public TextMeshProUGUI textMesh;



    public float startTime;
    public bool isNowOn;

    public bool hasFired;

    public bool shouldTurnOn = false;

    public AudioClip[] nowOnClips;


    void OnEnable(){
        
        startTime = Time.time;
    }
    public override void OnStart()
    {


        textMesh.enabled = true;
        print("We have started here!");
        gameObject.SetActive(true);
        God.audio.Play(God.sounds.tutorialSectionStartSound);
        OnStartEvent.Invoke();
        hasFired = false;

                God.targetableObjects.Remove(this.transform);
        GetComponent<Collider>().enabled = true;

        isNowOn = false;
        shouldTurnOn = true;

        startTime = Time.time;


    }
    public override void OnComplete()
    {


        textMesh.enabled = false;
        print("COMPLETING GETING TO LOCATION");
        God.audio.Play(God.sounds.tutorialSuccessSound);
        OnCompleteEvent.Invoke();

                God.targetableObjects.Remove(this.transform);
    }


    public void Update()
    {

        print(Time.time - startTime - timeTilPulseStart);

        if( Time.time - startTime > timeTilPulseStart && !isNowOn && shouldTurnOn ){
            isNowOn = true;

            for( int i = 0; i < nowOnClips.Length; i++ ){
                God.audio.Play( nowOnClips[i],1,.3f);
            }
            
            God.targetableObjects.Add(this.transform);
        }


    if( isNowOn ){
        print("updating this tutorial state");

        float value = Mathf.Sin((Time.time-startTime) * timeBetweenPulses);
        value = Mathf.Clamp(value, .5f, 1);
        value -= .5f;
        value *= 2;

        textMesh.color = new Color(1, 1, 1, value);
    }else{
        
        textMesh.color = new Color(1, 1, 1, 0);
    }


    }


    public void OnTriggerEnter(Collider c)
    {

        print(c.attachedRigidbody);
        print("Tutorial Reach Location Trigger Enter");

        if (c.attachedRigidbody == God.wren.physics.rb && hasFired == false)
        {

            
                God.targetableObjects.Remove(this.transform);
            print("Tutorial Reach Location Trigger Enter has correct body");
            OnLocationReached.Invoke();
            God.audio.Play(God.sounds.tutorialSuccessSound);
            hasFired = true;

            
        GetComponent<Collider>().enabled = false;

            tutorial.ReachLocationStateHit(this);
            OnComplete();
        }

    }

    public void OnCutSceneFinished()
    {

    }




}
