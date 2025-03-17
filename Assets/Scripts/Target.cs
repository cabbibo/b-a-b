using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WrenUtils;

public class Target : MonoBehaviour
{


    public int facingType;

    public float maxScale = 10;

    public bool active = false;

    public Crysplosion ringCrystal;
    public Crysplosion centerCrystal;

    public float growthSpeed;
    public float deathSpeed;

    public void OnSet()
    {

        active = true;

        if (ringCrystal != null) { ringCrystal.Reset(); }
        if (centerCrystal != null) { centerCrystal.Reset(); }

        God.audio.Play(God.sounds.crystalTogetherClip, Random.Range(.8f, 1.2f));


    }

    public void OnHit(float speed)
    {

        active = false;

        if (ringCrystal != null) { ringCrystal.Explode(speed); }
        if (centerCrystal != null) { centerCrystal.Explode(speed); }

        float v = speed / God.wren.physics.maxSpeed;

        God.audio.Play(God.sounds.crystalApartClip, Random.Range(v * .8f, v * 1.2f));


    }




    public void Erase()
    {
        active = false;
    }


    public void Update()
    {


        if (active == true)
        {

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * maxScale, growthSpeed);

        }
        else
        {

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, deathSpeed);

            if (transform.localScale.magnitude < 0.1f)
            {
                gameObject.SetActive(false);
            }

        }




        // face camera
        if (facingType == 0)
        {

            transform.LookAt(Camera.main.transform);
            //transform.Rotate(90, 0, 0);


            // backwards
        }
        else if (facingType == 1)
        {


            transform.LookAt(transform.position + Vector3.forward);
            transform.Rotate(0, 180, 0);

            // straight Up
        }
        else if (facingType == 2)
        {

            transform.LookAt(transform.position + Vector3.up);
            transform.Rotate(0, 0, 0);

        }



    }


}
