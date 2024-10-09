using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


// Gets positions that are static based on 'Slides'
public class SlideCameraManager : MonoBehaviour
{


    const float TIMESCALE_LOW = 0.001f;

    // The importances of this thing
    public float weight;


    // Update is called once per frame
    void Update()
    {
        // Will be doing orbiting and stuff here


    }


    public bool inSlide = false;

    // Sets the Slide!
    public void SetSlide(Slide slide)
    {
        inSlide = true;
        transform.position = slide.transform.position;
        transform.rotation = slide.transform.rotation;
        weight = 100000;
        // Time.timeScale = TIMESCALE_LOW;


    }

    public void ReleaseSlide()
    {
        inSlide = false;
        //Time.timeScale = 1;
        weight = 0;
    }

}
