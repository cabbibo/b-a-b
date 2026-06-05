using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Selectable handle for any of the shared God.particleSystems. Keep in sync with the fields below.
public enum GodParticleType
{
    None,
    SmallCollection, LargeCollection,
    SmallSuccess, LargeSuccess, SmallBoost, LargeBoost,
    SmallFail, LargeFail,
    Eat, Collision,
    Trail, Water,
    Chirp, Carrying, Death,
    ShardCollect, Fountain
}

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


    // Resolve a selectable type to its ParticleSystem (null for None / unassigned).
    public ParticleSystem Get(GodParticleType type)
    {
        switch (type) {
            case GodParticleType.SmallCollection: return smallCollectionParticleSystem;
            case GodParticleType.LargeCollection: return largeCollectionParticleSystem;
            case GodParticleType.SmallSuccess:    return smallSuccessParticleSystem;
            case GodParticleType.LargeSuccess:    return largeSuccessParticleSystem;
            case GodParticleType.SmallBoost:      return smallBoostParticleSystem;
            case GodParticleType.LargeBoost:      return largeBoostParticleSystem;
            case GodParticleType.SmallFail:       return smallFailParticleSystem;
            case GodParticleType.LargeFail:       return largeFailParticleSystem;
            case GodParticleType.Eat:             return eatParticleSystem;
            case GodParticleType.Collision:       return collisionParticleSystem;
            case GodParticleType.Trail:           return trailParticleSystem;
            case GodParticleType.Water:           return waterParticleSystem;
            case GodParticleType.Chirp:           return chirpParticleSystem;
            case GodParticleType.Carrying:        return carryingParticleSystem;
            case GodParticleType.Death:           return deathParticleSystem;
            case GodParticleType.ShardCollect:    return shardCollect;
            case GodParticleType.Fountain:        return fountainParticleSystem;
            default:                              return null;
        }
    }

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
