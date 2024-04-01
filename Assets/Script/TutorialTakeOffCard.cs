using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

public class TutorialTakeoffCard : TutorialCardTrigger
{


    public TutorialStateManager stateManager;

    void Update()
    {
        /*  if (followTransform)
              transform.position = followTransform.position;

          if (God.wren)
          {
              if (Vector3.Distance(God.wren.transform.position, transform.position) < Radius)
              {
                  OnTriggered();
              }
          }*/


        if (God.wren)
        {
            if (God.wren.state.onGround)
            {
                print("TRIGGERING TAKEOFF");
                _OnTriggered();
            }
        }

    }

    public PlayCutScene cutScene;


    /* public void OnEnable()
     {
         cutScene.CutSceneFinished += OnCutSceneFinished;
     }

     public void OnDisable()
     {
         cutScene.CutSceneFinished -= OnCutSceneFinished;
     }
 */

    public override void _OnTriggered()
    {

        OnTriggered();

        stateManager.OnFirstCrashStart();
        God.wren.shards.SpendAllShards();
        enabled = false; // dont try and trigger again


        // cutScene.Play();
        //  cutScene.OnFinished += OnCutSceneFinished;

        // on cut scene finished, play our card


    }

    public TutorialIslandEnder islandEnder;
    public GameObject portal;

    public void OnCutSceneFinished()
    {

        FlyingTutorialSequence.Instance.OnTutorialCardTriggered(cardType, target: lookAt ? transform : null, pause: pause);

        //print("CutScene Finished");
        //islandEnder.hasCrashed = true;
        //  portal.SetActive(false);
    }

    public override void OnTriggered()
    {

        // Destroy the wren
        //God.wren.shards.SpendAllShards();
        // God.wren.physics.TakeOff();
    }

    // void OnTriggerEnter(Collider c)
    // {
    //     // skip if only want on ground
    //     if (onlyOnGround && !God.wren.state.onGround) return;

    //     if (Helpers.isWrenCollision(c))
    //     {

    //         isTargeting = true;
    //         tmpStrength = God.wren.physics.rotateTowardsTargetOnGround;
    //         God.wren.cameraWork.objectTargeted = objectToTarget;

    //     }
    // }


    // void OnTriggerExit(Collider c)
    // {

    //     if (Helpers.isWrenCollision(c))
    //     {
    //         isTargeting = false;
    //         God.wren.physics.rotateTowardsTargetOnGround = tmpStrength;
    //         God.wren.cameraWork.objectTargeted = null;
    //     }

    // }
}
