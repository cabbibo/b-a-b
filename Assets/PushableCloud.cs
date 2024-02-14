using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PushableCloud : MonoBehaviour
{
    public ParticleSystem m_System;
    ParticleSystem.Particle[] m_Particles;
    List<ParticleCollisionEvent> collisionEvents;


    struct ParticleCollision
    {
        public int particleIndex;
        public float lifetime;
    }
    List<ParticleCollision> collisions;

    void OnEnable()
    {
        InitializeIfNeeded();
    }
    void InitializeIfNeeded()
    {
        if (m_System == null)
            m_System = GetComponent<ParticleSystem>();

        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
        
        if (collisions == null)
            collisions = new List<ParticleCollision>();
        
        if (collisionEvents == null)
            collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void LateUpdate()
    {
        int numParticlesAlive = m_System.GetParticles(m_Particles);

        // Change only the particles that are alive
        for (int i = 0; i < numParticlesAlive; i++)
        {
            // m_Particles[i].velocity += Vector3.up * m_Drift;
        }

        // Apply the particle changes to the Particle System
        m_System.SetParticles(m_Particles, numParticlesAlive);
    }

    void OnParticleCollision(GameObject other)
    {
        // Debug.Log(other.name);

        var c = new ParticleCollision();

        int numCollisionEvents = m_System.GetCollisionEvents(other, collisionEvents);

        Rigidbody rb = other.GetComponent<Rigidbody>();
        int i = 0;

        while (i < numCollisionEvents)
        {
            if (rb)
            {
                Vector3 pos = collisionEvents[i].intersection;
                Vector3 force = collisionEvents[i].velocity * 10;
                // rb.AddForce(force);
                
            }
            i++;
        }
    }
}
