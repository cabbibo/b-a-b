using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Events;

// Activities are small things to do across that map
// They can be redone over and over again, and you get crystals for doing them
// Each one has a trigger that you fly through to enter it
public class Activity : MonoBehaviour
{

    public int id;

    public bool discovered;
    public bool started;
    public bool completed;

    public string activityName;

    public Slide[] discoverSlides;
    public Slide[] startSlides;
    public Slide[] completeSlides;

    public Slide currentSlide;

    public bool inSlide;

    public Transform wrenPosition;


    public void Update()
    {
        if (inSlide)
        {
            if (God.input.x)
            {
                NextSlide();
            }
        }

    }

    public bool insideActivity;
    // Different Cameras for explaining the task
    // public Slide[] slides;

    public float releaseTime;

    public void OnTriggerEnter(Collider c)
    {
        //print("triggerEnter");
        //print(c.gameObject.name);

        if (!God.IsOurWren(c))
        {
            return;
        }

        if (insideActivity)
        {
            return;
        }
        print("triggerEnter2");
        print(c.gameObject.name);
        print(Time.time - releaseTime);

        // Cant accidently retrigger immediately
        if (Time.time - releaseTime > 3)
        {

            if (!started && !completed)
            {
                insideActivity = true;
                DoDiscover();
            }
        }

    }




    public void OnTriggerExit(Collider c)
    {
        //print("triggerExit");
        //print(c.gameObject.name);

        if (!God.IsOurWren(c))
        {
            return;
        }


        print("triggerExit2");
        print(c.gameObject.name);

        insideActivity = false;
        /*   if (discovered && !started && !completed)
           {
               discovered = false;
           }*/


    }

    public void DoDiscover()
    {

        print("DO DISCOVER");

        discovered = true;

        SetSlide(discoverSlides[0]);

        God.wren.Crash(wrenPosition.position);

        God.wren.physics.rb.isKinematic = true;
        //  God.wren.physics.rb.velocity = Vector3.zero;
        God.wren.physics.rb.position = wrenPosition.position;
        God.wren.physics.rb.rotation = wrenPosition.rotation;

    }

    public void SetSlide(Slide s)
    {
        currentSlide = s;
        inSlide = true;
        s.Set();
    }

    public void EndSlide(Slide s)
    {
        inSlide = false;
        s.Release();
        Release();
    }

    public void Release()
    {
        print("RELEASE");

        releaseTime = Time.time;
        God.wren.physics.rb.isKinematic = false;

    }



    public void NextSlide()
    {
        if (currentSlide == null)
        {
            Debug.LogError("No current slide");
            return;
        }

        for (int i = 0; i < discoverSlides.Length; i++)
        {
            if (discoverSlides[i] == currentSlide)
            {
                if (i < discoverSlides.Length - 1)
                {
                    SetSlide(discoverSlides[i + 1]);
                    return;
                }
                else
                {
                    EndSlide(currentSlide);
                }
            }
        }


        for (int i = 0; i < startSlides.Length; i++)
        {
            if (startSlides[i] == currentSlide)
            {
                if (i < startSlides.Length - 1)
                {
                    SetSlide(startSlides[i + 1]);
                    return;
                }
                else
                {
                    EndSlide(currentSlide);
                }
            }
        }

        for (int i = 0; i < completeSlides.Length; i++)
        {
            if (completeSlides[i] == currentSlide)
            {
                if (i < completeSlides.Length - 1)
                {
                    SetSlide(completeSlides[i + 1]);
                    return;
                }
                else
                {
                    EndSlide(currentSlide);
                }
            }
        }

    }






}
