using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class LookAtLocation : MonoBehaviour
{



    public Transform objectToTarget;

    public float minStrength;
    public float maxStrength;
    public float radius;

    public bool onlyOnGround;

    public bool isTargeting;

    public float tmpStrength;

    public LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isTargeting)
        {
            God.feedbackSystems.UpdateTargetLineRenderer(objectToTarget);
        }

    }

    void OnTriggerEnter(Collider c)
    {
        // skip if only want on ground
        if (onlyOnGround && !God.wren.state.onGround) return;

        if (Helpers.isWrenCollision(c))
        {

            isTargeting = true;
            tmpStrength = God.wren.physics.rotateTowardsTargetOnGround;
            God.wren.cameraWork.objectTargeted = objectToTarget;

        }
    }


    void OnTriggerExit(Collider c)
    {

        if (Helpers.isWrenCollision(c))
        {
            isTargeting = false;
            God.wren.physics.rotateTowardsTargetOnGround = tmpStrength;
            God.wren.cameraWork.objectTargeted = null;
        }

    }
}
