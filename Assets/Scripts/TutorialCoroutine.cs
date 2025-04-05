using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;


public class TutorialCoroutine : MonoBehaviour
{


    public bool debug;
    public bool hasStarted;
    public bool hasFinished;

    public TargetManager targetManager;

    public Coroutine tutSequence;

    public int shardsPerTargetHit = 10;

    public Transform startPosition;

    public TutorialStateManager stateManager;

    public List<GameObject> preObjects = new List<GameObject>();
    public List<GameObject> postObjects = new List<GameObject>();
    public List<GameObject> duringObjects = new List<GameObject>();

    public UnityEvent preEvent = new UnityEvent();
    public UnityEvent duringEvent = new UnityEvent();
    public UnityEvent postEvent = new UnityEvent();

    public void CheckState()
    {

        if (ConditionsForCompleted())
        {
            hasFinished = true;
            hasStarted = true;
            SetPostState();
        }
        else
        {
            hasFinished = false;
            hasStarted = false;
            SetPreState();
        }

    }

    public virtual bool ConditionsForCompleted()
    {
        return hasFinished;
    }


    public virtual void SetStartState()
    {


        foreach (GameObject go in preObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in duringObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in postObjects)
        {
            go.SetActive(false);
        }

        duringEvent.Invoke();

    }

    public virtual void SetPreState()
    {


        foreach (GameObject go in preObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in duringObjects)
        {
            go.SetActive(false);
        }



        foreach (GameObject go in postObjects)
        {
            go.SetActive(false);
        }

        preEvent.Invoke();

    }

    public virtual void SetPostState()
    {

        foreach (GameObject go in duringObjects)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in preObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in postObjects)
        {
            go.SetActive(true);
        }

        postEvent.Invoke();


    }




    public virtual void OnComplete()
    {
        if (tutSequence != null)
        {
            StopCoroutine(tutSequence);
            tutSequence = null;
        }

        hasFinished = true;
        hasStarted = true;
        SetPostState();


    }



    public void JumpStartTutorial()
    {

        print("JumpSTarting");
        God.wren.PhaseShift(startPosition);
        hasStarted = true;
        hasFinished = false;
        tutSequence = StartCoroutine(TutorialSequence());
    }



    public void StartTutorial()
    {
        hasStarted = true;
        tutSequence = StartCoroutine(TutorialSequence());
    }


    public virtual IEnumerator TutorialSequence()
    {
        yield return null;

        hasFinished = true;

    }



    public void ActivatePointer()
    {
        //hitTarget.SetActive(true);
        God.wren.interfaceUtils.interfacePointer.AddPointer(targetManager.currentTarget.transform, 0, new Vector4(0, 0, 0, 1));
        God.wren.interfaceUtils.interfacePointer.TurnOnPointer(targetManager.currentTarget.transform);
    }


    public void DeactivatePointer()
    {

        print("deactivate pointer");
        if (targetManager.currentTarget != null)
        {
            God.wren.interfaceUtils.RemovePointer(targetManager.currentTarget.transform);
        }
        targetManager.EraseCurrentTarget();
        // hitTarget.SetActive(false);
    }




    public void OnTargetHit()
    {
        God.audio.PlayBasedOnWrenSpeed(God.sounds.texturalHitClips[Random.Range(0, God.sounds.texturalHitClips.Length)]);
        God.wren.shards.CollectShards(shardsPerTargetHit, Random.Range(0, 10f), God.wren.transform.position);

        print("TARGET HIT");
        God.particleSystems.Emit(God.particleSystems.smallSuccessParticleSystem, God.wren.transform.position + God.wren.transform.forward * 5f, 100);
        DeactivatePointer();
        targetManager.HitTarget(God.wren.physics.speed);
        //God.particleSystems.transform.position = God.wren.transform.position + God.wren.transform.forward * 5;
        //God.particleSystems.smallSuccessParticleSystem.Play();


    }





}
