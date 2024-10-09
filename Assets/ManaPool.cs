using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class ManaPool : MonoBehaviour
{

    public bool collectOffGround;

    public float forwardSamplePosition;


    public int crystalsCollectedWhileInside;
    public float crystalType;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (isInside)
        {

            if (collectOffGround == false)
            {
                God.wren.shards.CollectShards(crystalsCollectedWhileInside, crystalType, transform.position);
            }
            else
            {
                // raycast down and get the ground position and ground tag
                RaycastHit hit;
                if (Physics.Raycast(God.wren.transform.position + Vector3.Scale(God.wren.transform.forward, new Vector3(1, 0, 1) * forwardSamplePosition), Vector3.down, out hit))
                {
                    if (hit.collider.tag == "ManaPool")
                    {
                        God.wren.shards.CollectShards(crystalsCollectedWhileInside, crystalType, hit.point);
                    }
                }
            }
        }

    }

    public bool isInside;

    public void OnTriggerEnter(Collider c)
    {
        if (God.IsOurWren(c))
        {
            isInside = true;
        }


    }

    public void OnTriggerExit(Collider c)
    {
        if (God.IsOurWren(c))
        {
            isInside = false;
        }
    }


}
