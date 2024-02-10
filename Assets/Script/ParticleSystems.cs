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

    public ParticleSystem eatParticleSystem;
    public ParticleSystem collisionParticleSystem;


    public ParticleSystem trailParticleSystem;
    public ParticleSystem waterParticleSystem;


    public ParticleSystem chirpParticleSystem;
    public ParticleSystem carryingParticleSystem;
    public ParticleSystem deathParticleSystem;


    public void Emit(ParticleSystem particleSystem, Vector3 position, int amount)
    {
        particleSystem.transform.position = position;
        particleSystem.Emit(amount);
    }




}
