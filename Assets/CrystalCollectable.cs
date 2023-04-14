using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CrystalCollectable : MonoBehaviour
{

    public Carryable carryable;

    public void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.name == "CrystalCollection")
        {
            if (God.wren != null)
            {
                God.wren.carrying.DropIfCarrying(carryable);
            }
        }
    }
}
