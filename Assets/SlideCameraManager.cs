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

    public float FOV;


    public bool lerp;
    public float lerpSpeed;
    public float slideStartTime;

    public Vector3 slideStartPosition;
    public Quaternion slideStartRotation;

    public Slide currentSlide;


    // Update is called once per frame
    void Update()
    {
        // Will be doing orbiting and stuff here

        if (inSlide && currentSlide != null)
        {

            if (currentSlide.lerp)
            {

                print("Lerping");
                float t = (Time.time - slideStartTime) / lerpSpeed;

                t = Mathf.Clamp01(t);
                weight = Mathf.Lerp(0, 10, t);

                transform.position = Vector3.Lerp(slideStartPosition, currentSlide.transform.position, t);
                transform.rotation = Quaternion.Slerp(slideStartRotation, currentSlide.transform.rotation, t);

                weight = Mathf.Lerp(weight, 10, .1f); // weight hits faster because we are lerping
            }

            weight = Mathf.Lerp(weight, 10, .01f);
        }
        else
        {
            weight = Mathf.Lerp(weight, 0, .01f);
        }


    }


    public bool inSlide = false;

    // Sets the Slide!
    public void SetSlide(Slide slide)
    {
        inSlide = true;
        weight = 100000;
        FOV = slide.FOV;
        lerp = slide.lerp;

        lerpSpeed = slide.lerpSpeed;
        slideStartTime = Time.time;
        currentSlide = slide;
        // Time.timeScale = TIMESCALE_LOW;

        slideStartPosition = God.camera.transform.position;
        slideStartRotation = God.camera.transform.rotation;

        if (!lerp)
        {

            transform.position = slide.transform.position;
            transform.rotation = slide.transform.rotation;

        }


    }

    public void ReleaseSlide()
    {

        inSlide = false;
        currentSlide = null;
        weight = 0;


    }

}
