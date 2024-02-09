using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class WrenGroundRepresent : MonoBehaviour
{

    public Transform rep;
    public float upAmount = .3f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (God.wren != null)
        {

            transform.position = God.wren.physics.straightDownIntersectionPosition;
            transform.position += God.wren.physics.straightDownIntersectionNormal * upAmount;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, God.wren.physics.straightDownIntersectionNormal);
            //transform.rotation = Quaternion.LookRotation(God.wren.physics.straightDownIntersectionNormal, Vector3.forward);


            rep.localScale = Vector3.one * (God.wren.physics.distToGround * .1f + .1f);
        }

    }
}
