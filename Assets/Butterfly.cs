using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class Butterfly : MonoBehaviour
{

    public ButterflySpawner bs;
    void OnTriggerEnter(Collider c)
    {

        if (God.IsOurWren(c))
        {
            bs.GotAte(this);
        }
    }

}
