using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

[ExecuteAlways]
public class WingSounds : MonoBehaviour
{

    public GranularSynth synth;

    public WrenInput input;

    public int leftRightClipID = 0;
    public int upDownClipID = 1;

    public int[] flapOpenClips;
    public int[] flapClosedClips;



    // Start is called before the first frame update
    void Start()
    {

    }

    public float lastGrainTime_LX;
    public float lastGrainTime_LY;


    public float lastGrainTime_RX;
    public float lastGrainTime_RY;


    public float lastGrainTime_LF;
    public float lastGrainTime_RF;

    public float minTimeBetweenGrains = .1f;

    // Update is called once per frame
    void Update()
    {

        //        print(God.input.left.x);
        if (God.wren == null)
        {
            return;
        }

        input = God.wren.input;




        float leftXDelta = Mathf.Abs(input.leftX - input.o_leftX);
        float rightXDelta = Mathf.Abs(input.rightX - input.o_rightX);
        float leftYDelta = Mathf.Abs(input.leftY - input.o_leftY);
        float rightYDelta = Mathf.Abs(input.rightY - input.o_rightY);
        float leftFlapDelta = input.left2 - input.o_left2;
        float rightFlapDelta = input.right2 - input.o_right2;



        float overallValue = Mathf.Clamp(God.wren.shards.GetBodyShardPercentage(), .3f, 1);





        // Left X
        if (Time.time - lastGrainTime_LX > minTimeBetweenGrains)
        {
            if (leftXDelta > .03f)
            {
                lastGrainTime_LX = Time.time;

                float leftBlend = .2f + input.leftX * .2f;


                float location = leftXDelta * 400;


                print(location);

                // synth.NewGrain(2.8f, 3 * overallValue, leftXDelta * 6 * overallValue, Random.Range(0, 1f), 0, leftBlend);
                synth.NewGrain(.5f, 1 * overallValue, leftXDelta * 6 * overallValue, location, 1, leftBlend);
            }

        }


        // Right X
        if (Time.time - lastGrainTime_RX > minTimeBetweenGrains)
        {
            if (rightXDelta > .03f)
            {

                lastGrainTime_RX = Time.time;
                float rightBlend = .8f + input.rightX * .2f;
                //synth.NewGrain(2.8f, 3 * overallValue, rightXDelta * 6 * overallValue, Random.Range(0, 1f), 0, rightBlend);


                float location = Random.Range(0f, 100f);


                synth.NewGrain(.5f, 1 * overallValue, rightXDelta * 6 * overallValue, location, 1, rightBlend);
            }

        }

        // Left Y
        if (Time.time - lastGrainTime_LY > minTimeBetweenGrains)
        {
            if (leftYDelta > .03f)
            {
                lastGrainTime_LY = Time.time;
                float leftBlend = .2f;
                float location = Random.Range(0f, 100f);
                synth.NewGrain(1.8f, 1 * overallValue, leftYDelta * 6 * overallValue, location, 1, leftBlend);
            }

        }



        // Right Y

        if (Time.time - lastGrainTime_RY > minTimeBetweenGrains)
        {
            if (rightYDelta > .03f)
            {
                lastGrainTime_RY = Time.time;
                float rightBlend = .8f;

                float location = Random.Range(0f, 100f);
                synth.NewGrain(1.8f, 1 * overallValue, rightYDelta * 6 * overallValue, Random.Range(0, 1f), 1, rightBlend);
            }

        }


        if (Time.time - lastGrainTime_LF > minTimeBetweenGrains)
        {
            float leftBlend = .2f;
            if (leftFlapDelta < 0)
            {
                lastGrainTime_LF = Time.time;
                synth.NewGrain(.8f, Mathf.Abs(leftFlapDelta) * 6 * overallValue, Mathf.Abs(leftFlapDelta) * 26 * overallValue, 0, flapOpenClips[Random.Range(0, flapOpenClips.Length)], leftBlend);
            }

            else if (leftFlapDelta > .0)
            {
                lastGrainTime_LF = Time.time;
                synth.NewGrain(.8f, Mathf.Abs(leftFlapDelta) * 6 * overallValue, Mathf.Abs(leftFlapDelta) * 26 * overallValue, 0, flapClosedClips[Random.Range(0, flapClosedClips.Length)], leftBlend);
            }
        }





        if (Time.time - lastGrainTime_RF > minTimeBetweenGrains)
        {
            float rightBlend = .8f;
            if (rightFlapDelta < 0)
            {
                lastGrainTime_RF = Time.time;
                synth.NewGrain(.8f, Mathf.Abs(rightFlapDelta) * 6 * overallValue, Mathf.Abs(rightFlapDelta) * 26 * overallValue, 0, flapOpenClips[Random.Range(0, flapOpenClips.Length)], rightBlend);
            }

            else if (rightFlapDelta > .0)
            {
                lastGrainTime_RF = Time.time;
                synth.NewGrain(.8f, Mathf.Abs(rightFlapDelta) * 6 * overallValue, Mathf.Abs(rightFlapDelta) * 26 * overallValue, 0, flapClosedClips[Random.Range(0, flapClosedClips.Length)], rightBlend);
            }
        }








        /* if (input.rightX < .5 && input.o_leftX > .5)
         {
             synth.NewGrain();
         }
 */


    }

}
