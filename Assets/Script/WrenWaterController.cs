using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenWaterController : MonoBehaviour
{


    public Helpers.PositionGameObjectEvent dropHitEvent;
    public bool isWatering;
    public Wren wren;
    public ParticleSystemForceField particleForces;

    public ParticleSystem wateringParticles;

    public List<ParticleCollisionEvent> collisionEvents;

    public float collectingGravity;

    public bool autoCollect;

    public float maxWaterAmount;
    public float currentWaterAmount;
    private float oldWaterAmount;
    public bool inWaterArea;

    public bool inWaterableArea;

    public bool fullDraining;
    public float waterFillAmount;
    public float waterBaseDrainAmount;
    public float waterFullDrainAmount;

    public GameObject objectWatering;







    // private ParticleSystem.EmissionModule emission;
    // Start is called before the first frame update
    void Awake()
    {

        collisionEvents = new List<ParticleCollisionEvent>();
        //emission = wateringParticles.emission;

    }

    public void TriggerEnter(Collider c)
    {

        print("HELLLO");
        if (c.tag == "WaterArea")
        {
            inWaterArea = true;
        }


        if (c.tag == "WaterableArea")
        {
            objectWatering = c.gameObject;
            inWaterableArea = true;
        }
    }

    public void TriggerExit(Collider c)
    {

        print("HELLLO");
        if (c.tag == "WaterArea")
        {
            inWaterArea = false;
        }

        if (c.tag == "WaterableArea")
        {

            objectWatering = null;
            inWaterableArea = false;
        }

    }

    // Update is called once per frame
    void Update()
    {

        oldWaterAmount = currentWaterAmount;
        ParticleSystem ps = wateringParticles;
        var emission = ps.emission;
        var collision = ps.collision;


        // Only enable particle collsions when we are watering!
        collision.enabled = false;
        if (inWaterArea)
        {

            bool collectingWater = false;

            if (autoCollect)
            {
                collectingWater = true;
            }
            else
            {
                if (wren.input.ex > .5f)
                {
                    collectingWater = true;
                }
            }

            if (collectingWater)
            {

                currentWaterAmount += waterFillAmount;
                particleForces.gravity = collectingGravity;
                if (currentWaterAmount < maxWaterAmount)
                {
                    WhileCollecting();
                }
            }
            else
            {
                particleForces.gravity = 0;
            }



        }
        else
        {



            if (currentWaterAmount > 0)
            {

                OnWaterDraining();

                currentWaterAmount -= waterBaseDrainAmount;
                emission.rateOverTime = 10f;


                if (wren.input.ex > .5f)
                {
                    fullDraining = true;
                    currentWaterAmount -= waterFullDrainAmount;
                    emission.rateOverTime = 50f;
                    if (inWaterableArea)
                    {
                        isWatering = true;
                        collision.enabled = true;
                    }
                }
                else
                {
                    fullDraining = false;
                    isWatering = false;
                }

            }
            else
            {
                emission.rateOverTime = 0f;
            }



        }



        currentWaterAmount = Mathf.Clamp(currentWaterAmount, 0, maxWaterAmount);
        if (currentWaterAmount >= maxWaterAmount && oldWaterAmount < maxWaterAmount)
        {
            OnWaterFilled();
        }



    }



    void OnWaterFilled()
    {

    }

    void OnWaterDraining()
    {

    }

    void OnWaterDropping()
    {

    }

    void OnWaterDrained()
    {

    }

    void WhileCollecting()
    {

    }




    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = wateringParticles.GetCollisionEvents(other, collisionEvents);

        //Rigidbody rb = other.GetComponent<Rigidbody>();
        int i = 0;

        if (numCollisionEvents >= 1)
        {
            dropHitEvent.Invoke(collisionEvents[0].intersection, objectWatering);
        }

        while (i < numCollisionEvents)
        {

            // print("HEEEYY + " + i);
            /* if (rb)
             {
                 Vector3 pos = collisionEvents[i].intersection;
                 Vector3 force = collisionEvents[i].velocity * 10;
                // rb.AddForce(force);
             }*/
            i++;
        }
    }



}
