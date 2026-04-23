using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WrenUtils;


[ExecuteAlways]
public class Collectable : MonoBehaviour
{

    public UnityEvent OnCollectEvent;
    public UnityEvent SetCollectedEvent;

    public UnityEvent OnStartEvent;
    public UnityEvent SetStartedEvent;

    public UnityEvent ResetEvent;



    public CollectableController collectableController;
    public CollectablePoint[] points;

    public bool[] pointCollected;
    public int pointsCollected;

    public bool started;
    public bool collected;

    public bool active;

    public Renderer platformRenderer;

    public MeshFilter collectedMeshFilter;


    private MaterialPropertyBlock mpb;


    void OnEnable()
    {

        mpb = new MaterialPropertyBlock();
        mpb.SetInt("_CollectablePoints", points.Length);
        mpb.SetFloat("_Inside", 0);
        mpb.SetFloat("_CollectedAmount", 0);
        mpb.SetFloat("_Finished", 0);
        platformRenderer.SetPropertyBlock(mpb);
    }

    // Update is called once per frame
    void Update()
    {

        if (active)
        {


        }

    }


    void OnTriggerEnter(Collider c)
    {

        if (God.IsOurWren(c))
        {
            OnEnter();
        }

    }

    void OnTriggerExit(Collider c)
    {
        if (God.IsOurWren(c))
        {
            OnExit();
        }
    }

    void OnEnter()
    {
        God.audio.Play(God.sounds.enterPlatform);
        active = true;
        mpb.SetFloat("_Inside", 1);
        platformRenderer.SetPropertyBlock(mpb);
        for (int i = 0; i < points.Length; i++)
        {
            points[i].OnEnter();
            points[i].enabled = true;
        }

        if (!started && collected == false)
        {
            started = true;
            collectableController.FullStart(this);
            OnStartEvent.Invoke();
        }

    }


    void OnExit()
    {


        God.audio.Play(God.sounds.exitPlatform);


        mpb.SetFloat("_Inside", 0);

        platformRenderer.SetPropertyBlock(mpb);

        active = false;

        for (int i = 0; i < points.Length; i++)
        {
            points[i].OnExit();
            points[i].enabled = false;
        }

    }


    public void PointCollected(CollectablePoint point)
    {


        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == point)
            {

                pointCollected[i] = point;
                pointsCollected += 1;

                mpb.SetFloat("_CollectedAmount", pointsCollected);
                platformRenderer.SetPropertyBlock(mpb);

                if (pointsCollected == points.Length && collected == false)
                {
                    OnAllPointsCollected();
                }


            }
        }




    }

    void OnAllPointsCollected()
    {

        God.particleSystems.largeSuccessParticleSystem.transform.position = transform.position;
        God.particleSystems.largeSuccessParticleSystem.Play();

        mpb.SetFloat("_CollectedAmount", points.Length);
        mpb.SetFloat("_Finished", 1);
        platformRenderer.SetPropertyBlock(mpb);
        God.audio.Play(God.sounds.collectableCollected);
        print("all Points Collected");

        collectableController.FullCollect(this);

        OnCollectEvent.Invoke();

    }

    public void SetCollected()
    {

        mpb.SetFloat("_CollectedAmount", points.Length);
        mpb.SetFloat("_Finished", 1);
        platformRenderer.SetPropertyBlock(mpb);
        collected = true;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].SetCollected();
        }

        SetCollectedEvent.Invoke();
    }



    public void SetStarted()
    {
        started = true;
        SetStartedEvent.Invoke();
    }



    public void FakePointPickup()
    {
        print("ff" + pointsCollected);
        points[pointsCollected].OnCollect();
    }

    public void FakeDrop()
    {

        pointsCollected = 0;

        collected = false;

        collectableController.Uncollect(this);

        for (int i = 0; i < points.Length; i++)
        {
            points[i].Uncollect();
        }

        ResetEvent.Invoke();

    }

    public void ResetState()
    {

        pointsCollected = 0;

        collected = false;
        collectableController.Uncollect(this);

        for (int i = 0; i < points.Length; i++)
        {
            points[i].Uncollect();
        }

        ResetEvent.Invoke();
    }
}
