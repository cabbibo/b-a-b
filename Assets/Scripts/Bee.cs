using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee : MonoBehaviour
{

    public Transform dropOffPoint;


    // Information for getting the bee to follow the wren
    public float pickUpRadius;
    public float followSpeed;
    public Vector3 vel;

    public bool followingWren;
    public bool droppedOff;

    public bool lockedOnTrunk;

    public Vector3 positionOnTrunk;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
