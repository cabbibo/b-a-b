using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                //  possibleCrystalsColliders[i].enabled = false;


                //print("collected");
                //possibleCrystalsRB[i].AddForce((possibleCrystals[i].transform.position - transform.position) * attractForce);
                crystalsCollected++;
            }
            else
            {
                //possibleCrystalsColliders[i].enabled = true;
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

    }

    public void OnLose()
    {
        print("should this be happening ever ?");
        print("proably across network....");
    }

    public void OnAllCollect()
    {
        print("something big");
    }

}

