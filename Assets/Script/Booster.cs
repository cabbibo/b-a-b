using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

using static Unity.Mathematics.math;
using Unity.Mathematics;

[ExecuteAlways]
public class Booster : Cycle
{
    public bool debug2;
    public float boostVal = 1;

    public Vector3 lifeBoostVal;

    public Life life;

    public Renderer renderer;
    public MaterialPropertyBlock mpb;


    public float currentScore;

    public AudioClip[] boostSounds;

    public Vector2 lastHitLocation;

    public BoostSim boostSim;

    public void OnBoost(Wren w)
    {


        float velMatch = Vector3.Dot(w.physics.vel, transform.forward);
        Vector3 fVel = transform.forward;


        float dist = GetNormalizedDistanceToCenterOfTheTransform(w.transform.position);

        if (velMatch < 0)
        {
            fVel *= -1;
        }

        // get local x and y of w.transform;
        Vector3 localPos = transform.InverseTransformPoint(w.transform.position);
        lastHitLocation = new Vector2(localPos.x, localPos.y);

        lifeBoostVal = .01f * fVel * boostVal * 1 / dist;
        w.physics.rb.AddForce(-w.physics.vel);
        w.physics.rb.AddForce(fVel * boostVal * 1 / dist);


        currentScore = length(fVel * boostVal * 1 / dist);


        for (int i = 0; i < (int)currentScore / 100; i++)
        {
            // Get Random clip
            AudioClip clip = boostSounds[UnityEngine.Random.Range(0, boostSounds.Length)];

            WrenUtils.God.audio.Play(clip, 10 - i, (float)i, w.transform.position, 100);// int step, float volume, Vector3 location, float falloff )
        }



        if (boostSim != null)
        {
            boostSim.OnBoost(this);
        }


    }



    public float GetNormalizedDistanceToCenterOfTheTransform(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        float dist = Vector3.Distance(localPos, Vector3.zero);

        print(dist);

        return dist;

    }




    public void OnBoost(Vector3 v)
    {

        lifeBoostVal = v * boostVal;
        if (boostSim != null)
        {
            boostSim.OnBoost(this);
        }

    }

    public override void OnLive()
    {


        if (renderer == null) { renderer = GetComponent<Renderer>(); }
        if (mpb == null) { mpb = new MaterialPropertyBlock(); }

        lastHitLocation = Vector2.one * 100 * UnityEngine.Random.Range(0.5f, 1.0f);
        currentScore = UnityEngine.Random.Range(0.5f, 1.0f);

        if (tv2 == null) { tv2 = new Vector2(0, 0); }

        tv2 = Vector2.one * 100 * UnityEngine.Random.Range(0.5f, 1.0f);
        tScore = UnityEngine.Random.Range(0.5f, 1.0f);

        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_CurrentScore", tScore);
        mpb.SetVector("_LastHitLocation", tv2);
        renderer.SetPropertyBlock(mpb);


    }

    public Vector2 tv2;
    public float tScore;

    public override void WhileLiving(float v)
    {



        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
            mpb = new MaterialPropertyBlock();

            lastHitLocation = Vector2.one * 100 * UnityEngine.Random.Range(0.5f, 1.0f);
            currentScore = UnityEngine.Random.Range(0.5f, 1.0f);

            if (tv2 == null) { tv2 = new Vector2(0, 0); }

            tv2 = Vector2.one * 100 * UnityEngine.Random.Range(0.5f, 1.0f);
            tScore = UnityEngine.Random.Range(0.5f, 1.0f);

        }

        tv2 = Vector2.Lerp(tv2, lastHitLocation, .2f);
        tScore = Mathf.Lerp(tScore, currentScore, .2f);

        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_CurrentScore", tScore);
        mpb.SetVector("_LastHitLocation", tv2);
        renderer.SetPropertyBlock(mpb);



    }

}
