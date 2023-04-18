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

    public void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.name == "CrystalCollection")
        {
            if (God.wren != null)
            {
                God.wren.carrying.DropIfCarrying(carryable);

                //can't pick up again
                /// GetComponent<Collider>().enabled = false;
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
        God.audio.Play(collectSound, 1f);
        God.audio.Play(collectSound, 2f);
        God.audio.Play(collectSound, 3f);
        collectSystem.transform.position = transform.position;
        collectSystem.Play();

        locationIndicatorSystem.Play();



    }


    public void OnDrop()
    {
        God.audio.Play(dropSound, 1f);
        God.audio.Play(dropSound, 2f);
        God.audio.Play(dropSound, 3f);



        dropSystem.transform.position = transform.position;
        dropSystem.Play();
        locationIndicatorSystem.Stop();
    }


}
