using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WrenUtils;

public class TutorialCardTrigger : MonoBehaviour
{
    public FlyingTutorialSequence.CardType cardType;

    public Transform followTransform;
    public float radius = 1;
    public bool lookAt = true;
    public bool pause = false;

    public virtual void _OnTriggered()
    {
        OnTriggered();
        FlyingTutorialSequence.Instance.OnTutorialCardTriggered(cardType, target: lookAt ? transform : null, pause: pause);
        enabled = false;
    }

    public virtual void OnTriggered()
    {

    }

    float Radius
    {
        get
        {
            return radius;
        }
    }

    void Update()
    {
        if (followTransform)
            transform.position = followTransform.position;

        if (God.wren)
        {
            if (Vector3.Distance(God.wren.transform.position, transform.position) < Radius)
            {
                _OnTriggered();
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
        Gizmos.color = Color.white;
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
