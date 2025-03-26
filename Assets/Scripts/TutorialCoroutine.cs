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

    public TargetManager targetManager;

    protected Coroutine tutSequence;

    public int shardsPerTargetHit = 10;

    public Transform startPosition;

    public TutorialStateManager stateManager;


    public virtual void JumpStartTutorial()
    {

        print("JumpSTarting");
        God.wren.PhaseShift(startPosition);
        tutSequence = StartCoroutine(TutorialSequence());
    }



    public virtual void StartTutorial()
    {
        tutSequence = StartCoroutine(TutorialSequence());
    }


    public virtual IEnumerator TutorialSequence()
    {

        yield return null;
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
