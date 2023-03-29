using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IMMATERIA;

using static Unity.Mathematics.math;
using Unity.Mathematics;

[ExecuteAlways]
public class Booster : Cycle
{
    public bool debug;
    public float boostVal = 1;

    public Vector3 lifeBoostVal;

    public Life life;

    public Renderer renderer;
    public MaterialPropertyBlock mpb;


    public float currentScore;

    public AudioClip[] boostSounds;

    public Vector2 lastHitLocation;

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

        print(currentScore);

        for (int i = 0; i < (int)currentScore / 100; i++)
        {
            // Get Random clip
            AudioClip clip = boostSounds[UnityEngine.Random.Range(0, boostSounds.Length)];

            WrenUtils.God.audio.Play(clip, 10 - i, (float)i, w.transform.position, 100);// int step, float volume, Vector3 location, float falloff )
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

        lifeBoostVal = v;

    }

    public override void OnLive()
    {

        lastHitLocation = Vector2.one * 100000;

        life.BindVector3("_BoostVal", () => this.lifeBoostVal);

        if (debug)
        {
            OnBoost(transform.forward);
        }

    }

    public override void WhileLiving(float v)
    {

        if (renderer == null) { renderer = GetComponent<Renderer>(); }
        if (mpb == null) { mpb = new MaterialPropertyBlock(); }


        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_CurrentScore", currentScore);
        mpb.SetVector("_LastHitLocation", lastHitLocation);
        renderer.SetPropertyBlock(mpb);


        WrenUtils.God.instance.SetWrenCompute(0, life.shader);
        lifeBoostVal *= .9f;// Vector3.Scale( lifeBoostVal , .9f );

    }

}
