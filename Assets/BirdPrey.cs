using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;


using WrenUtils;

[ExecuteAlways]
public class BirdPrey : MonoBehaviour
{


    public float3 velocity;
    public float3 position;

    float3 force;

    public float3 wrenPos;
    public float3 wrenForce;

    public float3 startPosition;


    void OnEnable()
    {
        transform.position = startPosition;
        //(Wander());
    }

    public string state = "wander";

    /*public string states = [
        "wander",
        "dive",
        "climb",
        "run"
    ];*/


    public float3 POI;
    public float lastPOITime;
    public float POIChangeTime = 3;

    public Transform poiRep;
    public Transform[] pois;

    public float poiDistanceForChange;


    public float avoidDistance;

    public float maxForce;
    public float maxSpeed;


    public float towardsPOIForce;
    void Update()
    {
        // Vector3 obstacleAvoidance = ObstacleAvoidance();
        //  Vector3 finalDirection = (targetDirection + obstacleAvoidance).normalized;
        // Quaternion targetRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        RaycastHit hit;
        float distance = 1000;
        if (Physics.Raycast(transform.position, transform.forward, out hit, avoidDistance))
        {
            distance = hit.distance;
        }

        force = 0;

        if (state == "wander")
        {



            if ((Time.time - lastPOITime > POIChangeTime) || (length(position - POI) < poiDistanceForChange))
            {
                POI = pois[UnityEngine.Random.Range(0, pois.Length)].position;
                lastPOITime = Time.time;
            }

            //  transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, .1f);

            force += (POI - position) * towardsPOIForce;



        }




        if (length(force) < maxForce)
        {
            force = normalize(force) * maxForce;
        }


        velocity += force;

        if (length(velocity) < maxSpeed)
        {
            velocity = normalize(velocity) * maxSpeed;
        }

        position += velocity;

        transform.position = position;



    }
}
