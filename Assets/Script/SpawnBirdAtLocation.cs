﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;
public class SpawnBirdAtLocation : MonoBehaviour
{

    public Transform resetLocation;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    void Spawn()
    {


        print("spawn");
        if (God.wren != null)
        {

            God.wren.bird.ResetAtLocation(resetLocation.position);

        }
        else
        {
            print("whoops");
        }
    }

}
