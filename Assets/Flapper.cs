using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;
using UnityEngine.Audio;

public class Flapper : MonoBehaviour
{

    public Wren wren;
    public SampleSynth diveSynth;
    public SampleSynth speedSynth;
    public SampleSynth closenessSynth;
    public SampleSynth bankSynth;
    public SampleSynth liftSynth;

    public SampleSynth brakeSynth;

    public AudioClip[] openClips;
    public AudioClip[] closeClips;



    public float leftFlap;
    public float rightFlap;

    public float oLeftFlap;
    public float oRightFlap;

    public float pitchMultiplier_open = 1;
    public float volumeMultiplier_open = 1;



    public float pitchMultiplier_close = 1;
    public float volumeMultiplier_close = 1;

    public string group = "reverby";
    public AudioMixer mixer;



    // Update is called once per frame
    void Update()
    {


        DoWingFlap();
        DoDiveSynth();

        DoSpeedSynth();
        DoClosenessSynth();
        DoBrakeSynth();
        DoLiftSynth();
        DoBankSynth();

    }




    void DoWingFlap()
    {
        if (wren.input.left2 > wren.input.o_left2)
        {
            float d = wren.input.left2 - wren.input.o_left2;
            God.audio.Play(closeClips, d * volumeMultiplier_close, d * pitchMultiplier_close, mixer, group);
        }

        if (wren.input.left2 < wren.input.o_left2)
        {
            float d = -(wren.input.o_left2 - wren.input.left2);
            God.audio.Play(openClips, d * volumeMultiplier_open, d * pitchMultiplier_open, mixer, group);
        }


        if (wren.input.right2 > wren.input.o_right2)
        {
            float d = wren.input.right2 - wren.input.o_right2;
            God.audio.Play(closeClips, d * volumeMultiplier_close, d * pitchMultiplier_close, mixer, group);
        }

        if (wren.input.right2 < wren.input.o_right2)
        {
            float d = (wren.input.o_right2 - wren.input.right2);
            God.audio.Play(openClips, d * volumeMultiplier_open, d * pitchMultiplier_open, mixer, group);
        }

    }


    [Header("Dive Synth")]
    public AudioClip diveClip;
    public float baseDiveVelocity;
    public float topDiveVelocity;

    public float baseDiveClipPosition;
    public float topDiveClipPosition;

    public float baseDivePitch;
    public float topDivePitch;

    public float baseDiveSpeed;
    public float topDiveSpeed;

    public float diveVal;

    public float bothWingDivePower;
    public float bothWingDiveMultiplier;




    void DoDiveSynth()
    {
        // while wings are tucked
        // check the speed, and go from location 1 to location 2 in the audio clip pitching up;
        // Stop when wings stop


        if (wren.input.left2 > 0 || wren.input.right2 > 0)
        {

            float total = (wren.input.left2 + wren.input.right2) / 2;
            float speed = wren.physics.rb.velocity.magnitude;
            float val = Mathf.InverseLerp(baseDiveVelocity, topDiveVelocity, speed);
            val *= Mathf.Pow(total, bothWingDivePower) * bothWingDiveMultiplier;

            diveVal = val;
            diveSynth.location = Mathf.Lerp(baseDiveClipPosition, topDiveClipPosition, val);
            diveSynth.pitch = Mathf.Lerp(baseDivePitch, topDivePitch, val);
            diveSynth.speed = Mathf.Lerp(baseDiveSpeed, topDiveSpeed, val);
            diveSynth.volume = val;
        }
        else
        {
            diveVal = 0;
            diveSynth.pitch = 0;
            diveSynth.speed = 11111;
            diveSynth.location = 1;
            diveSynth.volume = 0;

        }


        diveSynth.clip = diveClip;

    }



    [Header("Speed Synth")]

    public AudioClip speedClip;
    public float baseSpeedVelocity;
    public float topSpeedVelocity;

    public float baseSpeedClipPosition;
    public float topSpeedClipPosition;

    public float baseSpeedPitch;
    public float topSpeedPitch;

    public float baseSpeedSpeed;
    public float topSpeedSpeed;


    public float baseSpeedVolume;
    public float topSpeedVolume;

    public float speedVal;

    public float speedPow;

    void DoSpeedSynth()
    {



        float speed = wren.physics.rb.velocity.magnitude;
        float val = Mathf.InverseLerp(baseSpeedVelocity, topSpeedVelocity, speed);

        val = Mathf.Pow(val, speedPow);
        speedVal = val;
        speedSynth.location = Mathf.Lerp(baseSpeedClipPosition, topSpeedClipPosition, val);
        speedSynth.pitch = Mathf.Lerp(baseSpeedPitch, topSpeedPitch, val);
        speedSynth.speed = Mathf.Lerp(baseSpeedSpeed, topSpeedSpeed, val);
        speedSynth.volume = Mathf.Lerp(baseSpeedVolume, topSpeedVolume, val);

        speedSynth.clip = speedClip;

    }


    [Header("Closeness Synth")]

    public AudioClip closenessClip;
    public float baseClosenessHeight;
    public float topClosenessHeight;

    public float baseClosenessClipPosition;
    public float topClosenessClipPosition;

    public float baseClosenessPitch;
    public float topClosenessPitch;

    public float baseClosenessSpeed;
    public float topClosenessSpeed;


    public float baseClosenessVolume;
    public float topClosenessVolume;

    public float baseClosenessVelocity;
    public float topClosenessVelocity;

    public float velocityClosenessMultiplier;
    public float velocityClosenessBase;

    public float closenessVal;

    void DoClosenessSynth()
    {



        float height = wren.physics.distToGround;
        float val = Mathf.InverseLerp(baseClosenessHeight, topClosenessHeight, height);
        float vVel = Mathf.InverseLerp(baseClosenessVelocity, topClosenessVelocity, wren.physics.rb.velocity.magnitude);


        val *= vVel * velocityClosenessMultiplier + velocityClosenessBase;


        closenessVal = val;


        closenessSynth.location = Mathf.Lerp(baseClosenessClipPosition, topClosenessClipPosition, val);
        closenessSynth.pitch = Mathf.Lerp(baseClosenessPitch, topClosenessPitch, val);
        closenessSynth.speed = Mathf.Lerp(baseClosenessSpeed, topClosenessSpeed, val);
        closenessSynth.volume = Mathf.Lerp(baseClosenessVolume, topClosenessVolume, val);

        closenessSynth.clip = closenessClip;

    }

    [Header("Bank Synth")]
    public AudioClip bankClip;
    public float baseBankHeight;
    public float topBankHeight;

    public float baseBankClipPosition;
    public float topBankClipPosition;

    public float baseBankPitch;
    public float topBankPitch;

    public float baseBankSpeed;
    public float topBankSpeed;


    public float baseBankVolume;
    public float topBankVolume;

    public float baseBankVelocity;
    public float topBankVelocity;

    public float velocityBankMultiplier;
    public float velocityBankBase;


    public float bankVal;


    void DoBankSynth()
    {




        float total = Mathf.Abs(wren.input.leftX + wren.input.rightX) / 2;

        float val = Mathf.InverseLerp(baseBankHeight, topBankHeight, total);
        float vVel = Mathf.InverseLerp(baseBankVelocity, topBankVelocity, wren.physics.rb.velocity.magnitude);


        val *= vVel * velocityBankMultiplier + velocityBankBase;
        bankVal = val;


        bankSynth.location = Mathf.Lerp(baseBankClipPosition, topBankClipPosition, val);
        bankSynth.pitch = Mathf.Lerp(baseBankPitch, topBankPitch, val);
        bankSynth.speed = Mathf.Lerp(baseBankSpeed, topBankSpeed, val);
        bankSynth.volume = Mathf.Lerp(baseBankVolume, topBankVolume, val);

        bankSynth.clip = bankClip;
    }



    [Header("Lift Synth")]
    public AudioClip liftClip;
    public float baseLiftHeight;
    public float topLiftHeight;

    public float baseLiftClipPosition;
    public float topLiftClipPosition;

    public float baseLiftPitch;
    public float topLiftPitch;

    public float baseLiftSpeed;
    public float topLiftSpeed;


    public float baseLiftVolume;
    public float topLiftVolume;

    public float baseLiftVelocity;
    public float topLiftVelocity;

    public float velocityLiftMultiplier;
    public float velocityLiftBase;


    public float liftVal;


    void DoLiftSynth()
    {




        float total = Mathf.Abs(wren.input.leftY + wren.input.rightY) / 2;

        float val = Mathf.InverseLerp(baseLiftHeight, topLiftHeight, total);
        float vVel = Mathf.InverseLerp(baseLiftVelocity, topLiftVelocity, wren.physics.rb.velocity.magnitude);


        val *= vVel * velocityLiftMultiplier + velocityLiftBase;
        liftVal = val;


        liftSynth.location = Mathf.Lerp(baseLiftClipPosition, topLiftClipPosition, val);
        liftSynth.pitch = Mathf.Lerp(baseLiftPitch, topLiftPitch, val);
        liftSynth.speed = Mathf.Lerp(baseLiftSpeed, topLiftSpeed, val);
        liftSynth.volume = Mathf.Lerp(baseLiftVolume, topLiftVolume, val);
        liftSynth.clip = liftClip;


    }




    [Header("Brake Synth")]
    public AudioClip brakeClip;
    public float baseBrakeHeight;
    public float topBrakeHeight;

    public float baseBrakeClipPosition;
    public float topBrakeClipPosition;

    public float baseBrakePitch;
    public float topBrakePitch;

    public float baseBrakeSpeed;
    public float topBrakeSpeed;


    public float baseBrakeVolume;
    public float topBrakeVolume;

    public float baseBrakeVelocity;
    public float topBrakeVelocity;

    public float velocityBrakeMultiplier;
    public float velocityBrakeBase;


    public float brakeVal;


    void DoBrakeSynth()
    {




        float total = Mathf.Abs(wren.input.left3 + wren.input.right3) / 2;

        float val = Mathf.InverseLerp(baseBrakeHeight, topBrakeHeight, total);
        float vVel = Mathf.InverseLerp(baseBrakeVelocity, topBrakeVelocity, wren.physics.rb.velocity.magnitude);


        val *= vVel * velocityBrakeMultiplier + velocityBrakeBase;
        brakeVal = val;


        brakeSynth.location = Mathf.Lerp(baseBrakeClipPosition, topBrakeClipPosition, val);
        brakeSynth.pitch = Mathf.Lerp(baseBrakePitch, topBrakePitch, val);
        brakeSynth.speed = Mathf.Lerp(baseBrakeSpeed, topBrakeSpeed, val);
        brakeSynth.volume = Mathf.Lerp(baseBrakeVolume, topBrakeVolume, val);
        brakeSynth.clip = brakeClip;


    }








}
