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


        if (God.wren.state.onGround)
        {
            print("TRIGGERING TAKEOFF");
            _OnTriggered();
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
        God.wren.shards.SpendAllShards();
        cutScene.Play();
        //  cutScene.OnFinished += OnCutSceneFinished;
        enabled = false;

        // on cut scene finished, play our card


    }

    public void OnCutSceneFinished()
    {

        print("CutScene Finished");
        FlyingTutorialSequence.Instance.OnTutorialCardTriggered(cardType, target: lookAt ? transform : null, pause: pause);
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
