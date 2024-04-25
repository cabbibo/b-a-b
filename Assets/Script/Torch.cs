using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Normal.Realtime;


public class Torch : MonoBehaviour
{

    public bool lit;


    public string firePrefab;

    public GameObject fire;


    public ParticleSystem flameParticleSystem;



    // Update is called once per frame
    void Update()
    {

    }


    public void OnEnable()
    {
        if (lit)
        {
            Light();
        }
    }

    void OnTriggerEnter(Collider c)
    {


        if (c.tag == "Fire")
        {

            print("readyToLight");
            if (lit == false)
            {
                //var tc = c.gameObject.GetComponent<TimingCarryable>();

                // if( tc ){ tc.OnDry(); }
                print("lighting");
                Light();
            }
        }

        if (c.tag == "Wren")
        {

        }

    }




    void Light()
    {
        lit = true;
        flameParticleSystem.Play();
        lit = true;
        print("MADE IT HERE");

        if (fire == null)
        {
            fire = Realtime.Instantiate(firePrefab, transform);

            fire.transform.parent = transform;
            fire.transform.position = transform.position;
            fire.GetComponent<TimingCarryable>().respawnTransform = transform;
            fire.GetComponent<TimingCarryable>().Reset();
        }

    }
}
