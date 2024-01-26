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

    public Transform[] collectableTargets;

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
        if (c.gameObject.tag == "Collector")
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

    public Transform closestTarget;
    public float maxLength;

    public void Update()
    {


        closestTarget = null;
        maxLength = 10000;
        if (carryable.BeingCarried)
        {

            for (int i = 0; i < collectableTargets.Length; i++)
            {

                Vector3 d = collectableTargets[i].position - transform.position;
                if (d.magnitude < maxLength)
                {
                    closestTarget = collectableTargets[i];
                    maxLength = d.magnitude;

                }
            }


        }
    }


    public void LateUpdate()
    {
        if (closestTarget != null)
        {
            God.feedbackSystems.UpdateTargetLineRenderer(closestTarget);
        }
    }



    public void OnCollect()
    {
        God.audio.Play(collectSound, 1f, .3f);
        God.audio.Play(collectSound, 2f, .3f);
        God.audio.Play(collectSound, 3f, .3f);
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
