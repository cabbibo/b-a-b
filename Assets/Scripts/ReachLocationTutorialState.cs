using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using WrenUtils;
public class ReachLocationTutorialState : TutorialState
{
    public UnityEvent OnLocationReached;
    public Tutorial tutorial;
    public PlayCutScene playCutScene;


    public bool hasFired;
    public override void OnStart()
    {
        gameObject.SetActive(true);
        playCutScene.enabled = true;
        playCutScene.SetStartValues();
        God.audio.Play(God.sounds.tutorialSectionStartSound);
        OnStartEvent.Invoke();
        hasFired = false;
        // God.targetableObjects.Add(this.transform);
    }
    public override void OnComplete()
    {

        print("COMPLETING GETING TO LOCATION");
        God.audio.Play(God.sounds.tutorialSuccessSound);
        OnCompleteEvent.Invoke();
        //God.targetableObjects.Remove(this.transform);
        StartCoroutine(TurnOff(6));
    }

    IEnumerator TurnOff(float time)
    {
        yield return new WaitForSeconds(time);
        Finish();
    }

    public void Finish()
    {
        //gameObject.SetActive(false);
    }

    public void OnTriggerEnter(Collider c)
    {

        //print(c.attachedRigidbody);
        //print("triggy enter");

        if (c.attachedRigidbody == God.wren.physics.rb && hasFired == false)
        {
            // print("lets go");
            OnLocationReached.Invoke();
            God.audio.Play(God.sounds.tutorialSuccessSound);
            hasFired = true;
            playCutScene.Play();
        }

    }

    public void OnCutSceneFinished()
    {

        tutorial.ReachLocationStateHit(this);
    }




}
