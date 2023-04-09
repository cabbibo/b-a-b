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

    public ParticleSystem particleSystem;


    void OnEnable()
    {
        transform.position = startPosition;
        position = startPosition;
        velocity = 0;
        stamina = 1;
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

    public float stamina;
    public float staminaDrainRate;
    public float staminaRebuildRate;

    public float wrenChaseForce;
    public float wrenChaseDistance;

    public Transform tmpWren;
    public float avoidForce;


    public LineRenderer lr;

    public float distance;

    public float distanceFromGround;
    public float distanceFromGroundForce;

    public float dropForce;

    public float noiseForce;
    public float noiseSize;

    // Unity.Mathematics.Random random = new Unity.Mathematics.Random(0x6E624EB7u);
    void Update()
    {
        // Vector3 obstacleAvoidance = ObstacleAvoidance();
        //  Vector3 finalDirection = (targetDirection + obstacleAvoidance).normalized;
        // Quaternion targetRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);



        force = 0;

        RaycastHit hit;
        distance = 1000;
        if (Physics.Raycast(transform.position, transform.forward, out hit, avoidDistance))
        {
            distance = hit.distance;
        }

        if (hit.collider != null)
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hit.point);
            force += float3(hit.normal) * (avoidDistance - hit.distance) * avoidForce;
        }
        else
        {

            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position);
        }

        hit = new RaycastHit();
        distance = 1000;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distanceFromGround))
        {
            distance = hit.distance;
        }


        if (hit.collider != null)
        {
            lr.SetPosition(2, transform.position);
            lr.SetPosition(3, hit.point);
            force += float3(hit.normal) * (distanceFromGround - hit.distance) * distanceFromGroundForce;
        }
        else
        {
            force -= float3(0, 1, 0) * dropForce;
            lr.SetPosition(2, transform.position);
            lr.SetPosition(3, transform.position);
        }




        if (state == "wander")
        {



            if ((Time.time - lastPOITime > POIChangeTime) || (length(position - POI) < poiDistanceForChange))
            {

                //  print(UnityEngine.Random.Range(0, .99999f));

                // int idx = (int)(random.NextFloat() * (float)pois.Length);
                POI = pois[UnityEngine.Random.Range(0, pois.Length)].position;
                lastPOITime = Time.time;
            }

            force += float3(0, noise.snoise(position * noiseSize) * noiseForce, 0);

            //  transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, .1f);

            float3 dir = normalize(POI - position);
            force += dir * towardsPOIForce;
            lr.SetPosition(4, transform.position);
            lr.SetPosition(5, POI);

        }





        Wren w = God.ClosestWren(position);//
        float3 wPos = tmpWren.position;
        if (w != null)
        {
            wPos = float3(w.transform.position);
        }

        float3 d = wPos - position;

        if (length(d) < wrenChaseDistance)
        {
            stamina -= staminaDrainRate;
            if (stamina < 0)
            {
                stamina = 0;
            }

            if (stamina > 0)
            {
                force -= normalize(d) * wrenChaseForce;
            }

        }

        stamina += staminaRebuildRate;
        if (stamina > 1)
        {
            stamina = 1;
        }




        if (length(force) > maxForce)
        {
            force = normalize(force) * maxForce;
        }


        velocity += force;

        if (length(velocity) > maxSpeed)
        {
            velocity = normalize(velocity) * maxSpeed;
        }

        position += float3(transform.forward) * length(velocity);

        transform.position = position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity, Vector3.up), .03f);




    }



    void OnTriggerEnter(Collider c)
    {

        print("HELLO");
        if (God.IsOurWren(c))
        {
            Eat(c);
        }
    }

    public void Eat(Collider c)
    {

        print("HELLO2");
        particleSystem.transform.position = transform.position;
        particleSystem.transform.LookAt(Camera.main.transform.position);
        particleSystem.Play();
    }

}
