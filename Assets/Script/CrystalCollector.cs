using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class CrystalCollector : MonoBehaviour
{



    public GameObject[] possibleCrystals;
    public Rigidbody[] possibleCrystalsRB;
    public Collider[] possibleCrystalsColliders;
    public bool[] isCollected;




    public int crystalsCollected;
    public int oCrystalsCollected;

    public float collectionDist = 6;

    public float attractForce = .01f;

    public float insideDrag = 10;



    // Update is called once per frame
    void Update()
    {
        oCrystalsCollected = crystalsCollected;
        crystalsCollected = 0;


        for (int i = 0; i < possibleCrystals.Length; i++)
        {

            float d = Vector3.Distance(possibleCrystals[i].transform.position, transform.position);

            if (d < collectionDist)
            {

                possibleCrystalsColliders[i].enabled = false;

                //print("collected");
                possibleCrystalsRB[i].drag = insideDrag;
                possibleCrystalsRB[i].AddForce((possibleCrystals[i].transform.position - transform.position) * attractForce);
                crystalsCollected++;
            }
            else
            {
                possibleCrystalsColliders[i].enabled = true;
            }
        }


        if (oCrystalsCollected != crystalsCollected)
        {
            Debug.Log("Collected " + crystalsCollected + " crystals");
            if (crystalsCollected > oCrystalsCollected)
            {
                OnCollect();

            }
            else
            {
                OnLose();
            }

            if (crystalsCollected == possibleCrystals.Length)
            {
                OnAllCollect();
            }


        }



    }


    public void OnCollect()
    {

        God.particleSystems.largeSuccessParticleSystem.transform.position = transform.position;
        God.particleSystems.largeSuccessParticleSystem.Emit(100);
        God.audio.Play(God.sounds.smallSuccessSound);


    }

    public void OnLose()
    {
        print("should this be happening ever ?");
        print("proably across network....");
    }

    public void OnAllCollect()
    {

        God.particleSystems.largeSuccessParticleSystem.transform.position = transform.position;
        God.particleSystems.largeSuccessParticleSystem.Emit(3000);

        God.audio.Play(God.sounds.largeSuccessSound);
        print("something big");
    }

}

