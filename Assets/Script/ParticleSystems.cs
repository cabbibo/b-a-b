using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystems : MonoBehaviour
{

    public ParticleSystem smallCollectionParticleSystem;
    public ParticleSystem largeCollectionParticleSystem;


    public ParticleSystem smallSuccessParticleSystem;
    public ParticleSystem largeSuccessParticleSystem;
    public ParticleSystem smallBoostParticleSystem;
    public ParticleSystem largeBoostParticleSystem;

    public ParticleSystem smallFailParticleSystem;
    public ParticleSystem largeFailParticleSystem;

    public ParticleSystem eatParticleSystem;
    public ParticleSystem collisionParticleSystem;


    public ParticleSystem trailParticleSystem;
    public ParticleSystem waterParticleSystem;


    public ParticleSystem chirpParticleSystem;
    public ParticleSystem carryingParticleSystem;
    public ParticleSystem deathParticleSystem;

    public ParticleSystem shardCollect;


    public ParticleSystem fountainParticleSystem;


    public void Emit(ParticleSystem particleSystem, Vector3 position, int amount)
    {
        particleSystem.transform.position = position;
        particleSystem.Emit(amount);
    }

    public void EmitForTime(ParticleSystem particleSystem, Vector3 position, float emitRate, float time)
    {
        particleSystem.transform.position = position;
        var emission = particleSystem.emission;
        emission.rateOverTime = emitRate;
        particleSystem.Play();
        StartCoroutine(StopParticleSystem(particleSystem, time));

    }
    private IEnumerator StopParticleSystem(ParticleSystem particleSystem, float time)
    {
        yield return new WaitForSeconds(time);
        particleSystem.Stop();
    }




}
