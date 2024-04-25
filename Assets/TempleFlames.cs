using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;

[ExecuteAlways]
public class TempleFlames : MonoBehaviour
{

    public Biome biome;

    public List<Transform> torches;
    public List<GameObject> flames;

    public List<bool> lit;

    public float pickUpRadius;
    public float dropRadius;

    public bool wrenCarryingFlame;

    public GameObject flamePrefab;
    public GameObject wrenOnFire;

    public float fireKillSpeed;
    public float fireValue;


    // Start is called before the first frame update
    void OnEnable()
    {


        print("TEMPLE FLAMES ENABLED");

        if (biome.completed == false)
        {
            for (int i = 0; i < torches.Count; i++)
            {
                flames[i].SetActive(false);
                lit[i] = false;
            }

            lit[0] = true;
        }


        if (biome.completed == true)
        {
            for (int i = 0; i < torches.Count; i++)
            {
                lit[i] = true;
            }
        }

        // wren shouldn't be on fire to start!
        wrenOnFire.SetActive(false);

        for (int i = 0; i < torches.Count; i++)
        {
            if (!lit[i])
            {
                flames[i].SetActive(false);
            }
            else
            {
                flames[i].SetActive(true);
            }
        }

    }

    public Transform debugWren;

    public Transform testTransform;

    public AnimationCurve fireScaleCurve;
    public float fireScaleMultiplier;

    // Update is called once per frame
    void Update()
    {


        if (God.wren == null) { testTransform = debugWren; } else { testTransform = God.wren.transform; }


        for (int i = 0; i < torches.Count; i++)
        {
            if (!wrenCarryingFlame)
            {
                if (lit[i] == true)
                {
                    if (Vector3.Distance(torches[i].position, testTransform.position) < pickUpRadius)
                    {
                        OnWrenPickUpFlame(i);
                    }
                }
            }
            else
            {
                // Relight anyway!
                if (lit[i] == true)
                {
                    if (Vector3.Distance(torches[i].position, testTransform.position) < pickUpRadius)
                    {
                        OnWrenPickUpFlame(i);
                    }
                }

                if (lit[i] == false)
                {
                    if (Vector3.Distance(torches[i].position, testTransform.position) < dropRadius)
                    {
                        OnLightTorch(i);
                    }
                }


            }
        }



        if (wrenCarryingFlame)
        {
            wrenOnFire.transform.position = testTransform.position;

            wrenOnFire.transform.localScale = Vector3.one * fireScaleCurve.Evaluate(1 - fireValue) * fireScaleMultiplier;

            fireValue -= fireKillSpeed;


            if (fireValue <= 0)
            {
                OnWrenDropFlame();
            }

        }

    }

    public int totalLitTorches = 0;
    public void OnLightTorch(int i)
    {
        lit[i] = true;
        flames[i].SetActive(true);

        totalLitTorches = 0;

        for (int j = 0; j < lit.Count; j++)
        {

            if (lit[j] == true)
            {
                totalLitTorches++;
            }

        }


        float amount = (float)totalLitTorches / (float)lit.Count;
        print(amount);
        biome.SetCompletion(amount);


    }

    public void OnWrenPickUpFlame(int i)
    {
        wrenCarryingFlame = true;
        wrenOnFire.SetActive(true);
        fireValue = 1;
    }

    public void OnWrenDropFlame()
    {
        wrenCarryingFlame = false;
        wrenOnFire.SetActive(false);
        fireValue = 0;
    }


}
