using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CrystalCollectable : MonoBehaviour
{

    public Carryable carryable;

    public AudioClip collectSound;
    public AudioClip dropSound;

    public ParticleSystem collectSystem;
    public ParticleSystem dropSystem;

    public ParticleSystem locationIndicatorSystem;



    public CrystalCollector collector;

    public Collider c;

    public void OnEnable()
    {

        //  God.targetableObjects.Add(this.transform);
    }

    public void OnDisable()
    {

        //   God.targetableObjects.Remove(this.transform);
    }

    public void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.name == "CrystalCollection")
        {
            if (God.wren != null)
            {
                God.wren.carrying.DropIfCarrying(carryable);

                //can't pick up again
                ///  GetComponent<Collider>().enabled = false;

                //  God.targetableObjects.Remove(this.transform);
                OnDrop();
                //collector.OnCollect();
            }
        }

        if (God.IsOurWren(c))
        {
            OnCollect();
        }
    }


    public void OnCollect()
    {
        God.audio.Play(collectSound, 1f , .3f);
        God.audio.Play(collectSound, 2f , .3f);
        God.audio.Play(collectSound, 3f , .3f);
        collectSystem.transform.position = transform.position;
        collectSystem.Play();

        locationIndicatorSystem.Play();



    }


    public void OnDrop()
    {
        God.audio.Play(dropSound, 1f, .3f);
        God.audio.Play(dropSound, 2f, .3f);
        God.audio.Play(dropSound, 3f, .3f);



        dropSystem.transform.position = transform.position;
        dropSystem.Play();
        locationIndicatorSystem.Stop();
    }


}