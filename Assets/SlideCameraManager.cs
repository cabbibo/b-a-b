using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;


// Gets positions that are static based on 'Slides'
public class SlideCameraManager : BaseCameraManager
{






    public bool lerp;
    public float lerpSpeed;
    public float slideStartTime;

    public Vector3 slideStartPosition;
    public Quaternion slideStartRotation;

    public Slide currentSlide;


    public float currentLerpVal;

    // Update is called once per frame
    public override void WhileInUse()
    {
        // Will be doing orbiting and stuff here

        if (inSlide && currentSlide != null)
        {


            if (currentSlide.transitionType == Slide.TransitionType.instant)
            {

                transform.position = currentSlide.transform.position;
                transform.rotation = currentSlide.transform.rotation;
                transitioning = false;
                weight = 10;

            }
            else if (currentSlide.transitionType == Slide.TransitionType.lerp)
            {

                float t = (Time.time - slideStartTime) / lerpSpeed;


                if (Time.time - slideStartTime > lerpSpeed)
                {
                    transitioning = false;
                    transform.position = currentSlide.transform.position;
                    transform.rotation = currentSlide.transform.rotation;
                    currentLerpVal = 1;
                    weight = 1000;
                }
                else
                {
                    t = Mathf.Clamp01(t);
                    weight = Mathf.Lerp(0, 10, t);

                    currentLerpVal = t;

                    transform.position = Vector3.Lerp(slideStartPosition, currentSlide.transform.position, t);
                    transform.rotation = Quaternion.Slerp(slideStartRotation, currentSlide.transform.rotation, t);

                    weight = Mathf.Lerp(weight, 10, .1f); // weight hits faster because we are lerping
                }

            }
            else if (currentSlide.transitionType == Slide.TransitionType.animated)
            {

                // evaluate the animation in here!

                float t = Time.time - slideStartTime;
                float timelineTime = t - lerpSpeed;

                God.playableDirector.playableAsset = currentSlide.timeline;

                if (transitioning)
                {

                    if (timelineTime < 0)
                    {
                        God.playableDirector.time = 0;
                        God.playableDirector.Evaluate();

                        float lerpVal = Mathf.Clamp01(t / lerpSpeed);
                        currentLerpVal = lerpVal;


                        //        print("Evaluating");  
                        transform.position = Vector3.Lerp(slideStartPosition, currentSlide.cameraTarget.transform.position, lerpVal);
                        transform.rotation = Quaternion.Slerp(slideStartRotation, currentSlide.cameraTarget.transform.rotation, lerpVal);

                        weight = lerpVal * 10;

                    }
                    else if (timelineTime < slideAnimationLength) // we are animating lock it in
                    {
                        God.playableDirector.time = timelineTime;
                        God.playableDirector.Evaluate();

                        transform.position = currentSlide.cameraTarget.transform.position;
                        transform.rotation = currentSlide.cameraTarget.transform.rotation;

                        weight = 100;

                    }
                    else
                    {
                        God.playableDirector.time = slideAnimationLength;
                        God.playableDirector.Evaluate();

                        float lerpVal = ((timelineTime - slideAnimationLength) / lerpSpeed);
                        currentLerpVal = lerpVal;

                        if (lerpVal > 1)
                        {
                            transitioning = false;
                            transform.position = currentSlide.transform.position;
                            transform.rotation = currentSlide.transform.rotation;
                            lerpVal = 1;
                            currentLerpVal = 1;

                        }
                        else
                        {

                            transform.position = Vector3.Lerp(currentSlide.cameraTarget.transform.position, currentSlide.transform.position, lerpVal);
                            transform.rotation = Quaternion.Slerp(currentSlide.cameraTarget.transform.rotation, currentSlide.transform.rotation, lerpVal);

                        }

                        weight = 100;

                    }
                }




            }
            else if (currentSlide.transitionType == Slide.TransitionType.locked)
            {
                transform.position = currentSlide.transform.position;
                transform.rotation = currentSlide.transform.rotation;
                transitioning = false;
                weight = 10;

            }
            else
            {
                print("Invalid Transition Type");
            }


            //  weight = Mathf.Lerp(weight, 0, .01f);

        }
        else
        {
            weight = Mathf.Lerp(weight, 0, .01f);
        }

        // OverallCameraManager.REqustpriotut(this);
    }


    public bool inSlide = false;

    public float slideAnimationLength;
    public bool transitioning = false;



    // Sets the Slide!
    public void SetSlide(Slide slide)
    {

        inSlide = true;
        FOV = slide.FOV;

        transitioning = true;

        lerpSpeed = slide.lerpSpeed;
        slideStartTime = Time.time;
        currentSlide = slide;
        // Time.timeScale = TIMESCALE_LOW;

        slideStartPosition = God.camera.transform.position;
        slideStartRotation = God.camera.transform.rotation;


        if (currentSlide.transitionType == Slide.TransitionType.instant)
        {

            transform.position = slide.transform.position;
            transform.rotation = slide.transform.rotation;

        }
        else if (currentSlide.transitionType == Slide.TransitionType.lerp)
        {
            // do nothing
        }
        else if (currentSlide.transitionType == Slide.TransitionType.animated)
        {

            // get the total length of the animation

            slideAnimationLength = (float)slide.timeline.duration;
            God.playableDirector.playableAsset = slide.timeline;
            God.playableDirector.time = 0;
            God.playableDirector.Evaluate();


        }
        else if (currentSlide.transitionType == Slide.TransitionType.locked)
        {
            // tween to locked position
            transform.position = slide.transform.position;
            transform.rotation = slide.transform.rotation;

        }
        else
        {
            print("Invalid Transition Type");
        }

        RequestPriority();


    }

    public void ReleaseSlide()
    {

        inSlide = false;
        currentSlide = null;
        weight = 0;
        ReleasePriority();

        // overallManager.releasePriority

    }

}
