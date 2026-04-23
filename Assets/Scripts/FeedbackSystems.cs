using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrenUtils;

public class FeedbackSystems : MonoBehaviour
{


    public LineRenderer currentTargetLineRenderer;
    public ParticleSystem skimParticles;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        currentTargetLineRenderer.enabled = false;

    }

    public void UpdateTargetLineRenderer(Transform t)
    {
        if (God.wren)
        {
            currentTargetLineRenderer.enabled = true;
            currentTargetLineRenderer.SetPosition(0, t.position);
            currentTargetLineRenderer.SetPosition(1, (God.wren.transform.position - t.position) * .8f + t.position);
            currentTargetLineRenderer.SetPosition(2, God.wren.transform.position);
        }
    }


    public void DoSmallSuccess()
    {
        God.particleSystems.Emit(God.particleSystems.smallSuccessParticleSystem, God.wren.transform.position, 50);
        God.audio.Play(God.sounds.smallSuccessSound);
    }

    public void DoSmallSuccess(Vector3 position)
    {
        God.particleSystems.Emit(God.particleSystems.smallSuccessParticleSystem, position, 50);
        God.audio.Play(God.sounds.smallSuccessSound);
    }


    public void DoLargeSuccess()
    {
        God.particleSystems.Emit(God.particleSystems.largeSuccessParticleSystem, God.wren.transform.position, 50);
        God.audio.Play(God.sounds.largeSuccessSound);
    }

    public void DoLargeSuccess(Vector3 position)
    {
        God.particleSystems.Emit(God.particleSystems.largeSuccessParticleSystem, position, 50);
        God.audio.Play(God.sounds.largeSuccessSound);
    }


    public void DoSmallFail()
    {
        God.particleSystems.Emit(God.particleSystems.smallFailParticleSystem, God.wren.transform.position, 50);
        God.audio.Play(God.sounds.smallFailSound);
    }

    public void DoSmallFail(Vector3 position)
    {
        God.particleSystems.Emit(God.particleSystems.smallFailParticleSystem, position, 50);
        God.audio.Play(God.sounds.smallFailSound);
    }

    public void DoLargeFail()
    {
        God.particleSystems.Emit(God.particleSystems.largeFailParticleSystem, God.wren.transform.position, 50);
        God.audio.Play(God.sounds.largeFailSound);
    }

    public void DoLargeFail(Vector3 position)
    {
        God.particleSystems.Emit(God.particleSystems.largeFailParticleSystem, position, 50);
        God.audio.Play(God.sounds.largeFailSound);
    }


}
