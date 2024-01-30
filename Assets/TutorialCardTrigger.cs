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

    void OnTriggered()
    {
        FlyingTutorialSequence.Instance.OnTutorialCardTriggered(cardType);
        enabled = false;
    }

    float Radius { get {
        return radius;
    }}

    void Update()
    {
        if (followTransform)
            transform.position = followTransform.position;

        if (God.wren)
        {
            if (Vector3.Distance(God.wren.transform.position, transform.position) < Radius)
            {
                OnTriggered();
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
        Gizmos.color = Color.white;
    }
}
