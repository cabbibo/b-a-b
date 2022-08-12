using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalCollision : MonoBehaviour
{

    public ParticleSystem successParticles;

    public bool hasFired;
    public int sceneID;

    public Collider collider;

    public bool toEther;
    
   


    public void OnCollisionEnter(Collision c){



        if( c.collider.attachedRigidbody == God.wren.physics.rb && hasFired == false){
            hasFired = true;
            successParticles.Play();
            God.sceneController.LoadScene(sceneID);

            
            //God.wren.physics.rb.position += transform.forward * 2;

            collider.enabled = false;
        }

    }

}
