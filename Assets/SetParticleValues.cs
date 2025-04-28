using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticleValues : MonoBehaviour
{
    public ParticleSystem particlesActive;
    public ParticleSystem particlesPassive;

    public int activeEmissionRate  = 100;
    public int passiveEmissionRate = 10;

    public void SetParticlesActive()
    {

        if ( particlesActive != null ) {
            var emission = particlesActive.emission;
            emission.rateOverTime = activeEmissionRate;
        }

        if ( particlesPassive != null ) {
            var emission = particlesPassive.emission;
            emission.rateOverTime = 0;
        }

    }


    public void SetParticlesPassive()
    {

        if ( particlesActive != null ) {
            var emission = particlesActive.emission;
            emission.rateOverTime = 0;
        }

        if ( particlesPassive != null ) {
            var emission = particlesPassive.emission;
            emission.rateOverTime = passiveEmissionRate;
        }

    }

    public void SetParticlesOff()
    {
        if ( particlesActive != null ) {
            var emission = particlesActive.emission;
            emission.rateOverTime = 0;
        }

        if ( particlesPassive != null ) {
            var emission = particlesPassive.emission;
            emission.rateOverTime = 0;
        }
    }
}