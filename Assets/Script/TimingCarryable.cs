using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using WrenUtils;

public class TimingCarryable : MonoBehaviour
{

    public bool destroyOnDry = true;
    public bool autoRefill = true;
    public float maxAmount;
    public float currentAmount;

    public Transform respawnTransform;
    public Vector3 respawnOffset;

    public float fillSpeed;

    public bool filling;

    public bool drying;
    public float dripSpeed;

    public Carryable carryable;

    public float maxScale;

    public bool atSpawnPoint;
    public bool beingCarried;

    public ParticleSystem particles;


    // Update is called once per frame
    void Update()
    {


        // currently carrying in a location where
        // the water is fading away
        if (drying == true)
        {
            currentAmount -= dripSpeed;

            if (currentAmount < 0)
            {
                currentAmount = 0;
                OnDry();
            }

        }

        // currently at one of the ponds
        // filling up our water
        if (filling)
        {
            currentAmount += fillSpeed;

            if (currentAmount > maxAmount)
            {
                currentAmount = maxAmount;
                OnFull();
            }

        }


        transform.localScale = Vector3.one * Mathf.Max(maxScale * (currentAmount / maxAmount), .01f);

        if (carryable.BeingCarried != true && beingCarried == true)
        {
            Drop();
        }

        if (carryable.BeingCarried == true && beingCarried == false)
        {
            Pickup();
        }

    }

    void OnEnable()
    {
        Reset();
    }


    Loop fillLoop;
    Loop carryLoop;

    void Drop()
    {
        beingCarried = false;
    }

    void Pickup()
    {
        beingCarried = true;
        OnStartDrying();
    }


    public void OnStartFilling()
    {


        currentAmount = 0;
        drying = false;
        filling = true;

    }

    public void OnFull()
    {
        filling = false;
    }

    public void OnStartDrying()
    {

        drying = true;
        filling = false;
    }

    public void OnDry()
    {

        Reset();
        Destroy();

    }


    public void Reset()
    {
        currentAmount = 0;
        foreach (Wren w in God.wrens)
        {
            w.carrying.DropIfCarrying(carryable);
        }

        drying = false;

        if (respawnTransform == null)
        {
            respawnTransform = transform.parent;
        }

        if (respawnTransform)
        {
            transform.position = respawnTransform.position + respawnOffset;
        }

        transform.GetComponent<Rigidbody>().position = transform.position;
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

        if (autoRefill)
        {
            OnStartFilling();
        }

        if (beingCarried)
        {
            carryable.TryToDrop(carryable.carrier);
        }

    }

    public void Destroy()
    {
        if (destroyOnDry)
        {
            Realtime.Destroy(this.gameObject);
        }

    }

    public void OnSpawn()
    {

    }


}