using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalCollector : MonoBehaviour
{



    public GameObject[] possibleCrystals;

    public int crystalsCollected;
    public int oCrystalsCollected;

    public float collectionDist = 6;



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
                crystalsCollected++;
            }
        }


        if (oCrystalsCollected != crystalsCollected)
        {
            Debug.Log("Collected " + crystalsCollected + " crystals");
        }

        if (crystalsCollected == possibleCrystals.Length)
        {
            Debug.Log("All crystals collected!");
        }



    }
}
