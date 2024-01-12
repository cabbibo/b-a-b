using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class TransportWren : MonoBehaviour
{

    public void OnEnable()
    {

        lastSpawnTime = Time.time;

    }

    public float timeBetweenSpawns = 1;

    float lastSpawnTime;

    public void Update()
    {

        if (Time.time - lastSpawnTime > timeBetweenSpawns)
        {
            lastSpawnTime = Time.time;
            God.wren.PhaseShift(transform.position);
        }
    }


}
